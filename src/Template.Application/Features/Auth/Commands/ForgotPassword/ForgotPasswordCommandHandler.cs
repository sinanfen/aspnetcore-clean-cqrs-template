using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Template.Application.Common.Results;
using Template.Application.Services.Email;
using Template.Domain.Entities.Identity;

namespace Template.Application.Features.Auth.Commands.ForgotPassword;

/// <summary>
/// Şifre sıfırlama talebi komutu işleyicisi
/// </summary>
public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, IResult<ForgotPasswordResponse>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailSender _emailSender;
    private const int TokenValidityMinutes = 30; // Token 30 dakika geçerli

    /// <summary>
    /// Constructor - Gerekli servisler dependency injection ile alınır
    /// </summary>
    /// <param name="userManager">ASP.NET Identity kullanıcı yöneticisi</param>
    /// <param name="emailSender">E-posta gönderim servisi</param>
    public ForgotPasswordCommandHandler(
        UserManager<AppUser> userManager, 
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }

    /// <summary>
    /// Şifre sıfırlama talebi işlemini gerçekleştirir
    /// </summary>
    /// <param name="request">ForgotPassword komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>E-posta gönderim durumu</returns>
    public async Task<IResult<ForgotPasswordResponse>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Kullanıcıyı e-posta ile bul
            var user = await _userManager.FindByEmailAsync(request.Email);

            // Güvenlik için kullanıcı bulunamasa bile başarılı yanıt döndürür (email enumeration saldırısını önler)
            if (user == null)
            {
                return Result.Success(new ForgotPasswordResponse
                {
                    EmailSent = true,
                    Email = request.Email,
                    TokenExpires = DateTime.UtcNow.AddMinutes(TokenValidityMinutes),
                    TokenValidityMinutes = TokenValidityMinutes
                }, "Eğer bu e-posta adresi sistemde kayıtlıysa, şifre sıfırlama bağlantısı gönderilmiştir.");
            }

            // E-posta doğrulanmış mı kontrol et
            if (!user!.EmailConfirmed)
            {
                return Result.Failure<ForgotPasswordResponse>("E-posta adresi doğrulanmadan şifre sıfırlanamaz.");
            }

            // Password reset token oluştur
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Token'ı URL-safe hale getir
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));

            // Şifre sıfırlama URL'i oluştur
            var resetUrl = $"{request.ResetUrl}?token={encodedToken}&email={Uri.EscapeDataString(user.Email!)}";

            // Token son kullanma tarihi
            var tokenExpires = DateTime.UtcNow.AddMinutes(TokenValidityMinutes);

            // E-posta gönder
            var emailSent = await SendPasswordResetEmailAsync(user.Email!, user.FullName, resetUrl, tokenExpires);

            // Yanıt oluştur
            var response = new ForgotPasswordResponse
            {
                EmailSent = emailSent,
                Email = user.Email!,
                TokenExpires = tokenExpires,
                TokenValidityMinutes = TokenValidityMinutes
            };

            var message = emailSent 
                ? "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi."
                : "Şifre sıfırlama talebi alındı ancak e-posta gönderilirken hata oluştu.";

            return Result.Success(response, message);
        }
        catch (Exception ex)
        {
            return Result.Failure<ForgotPasswordResponse>($"Şifre sıfırlama talebi sırasında hata oluştu: {ex.Message}");
        }
    }

    /// <summary>
    /// Şifre sıfırlama e-postası gönderir
    /// </summary>
    /// <param name="email">Kullanıcı e-posta adresi</param>
    /// <param name="fullName">Kullanıcı adı soyadı</param>
    /// <param name="resetUrl">Şifre sıfırlama URL'i</param>
    /// <param name="tokenExpires">Token son kullanma tarihi</param>
    /// <returns>E-posta gönderim durumu</returns>
    private async Task<bool> SendPasswordResetEmailAsync(string email, string fullName, string resetUrl, DateTime tokenExpires)
    {
        try
        {
            var subject = "Şifre Sıfırlama Talebi";
            
            var body = $@"
                <h2>Şifre Sıfırlama Talebi</h2>
                <p>Merhaba {fullName},</p>
                <p>Hesabınız için şifre sıfırlama talebi aldık.</p>
                
                <h3>Şifre Sıfırlama</h3>
                <p>Yeni şifrenizi belirlemek için aşağıdaki bağlantıya tıklayın:</p>
                <p style=""margin: 20px 0;"">
                    <a href=""{resetUrl}"" 
                       style=""background-color: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block;"">
                        Şifremi Sıfırla
                    </a>
                </p>
                
                <h3>Güvenlik Bilgileri</h3>
                <p><strong>Geçerlilik Süresi:</strong> {tokenExpires:dd.MM.yyyy HH:mm} (UTC)</p>
                <p><strong>Süre:</strong> {TokenValidityMinutes} dakika</p>
                
                <h3>Güvenlik Uyarıları</h3>
                <ul>
                    <li>Bu talebi siz yapmadıysanız, bu e-postayı dikkate almayın</li>
                    <li>Şifre sıfırlama bağlantısını kimseyle paylaşmayın</li>
                    <li>Bağlantı yalnızca {TokenValidityMinutes} dakika geçerlidir</li>
                    <li>Şüpheli aktivite fark ederseniz bizimle iletişime geçin</li>
                </ul>
                
                <hr style=""margin: 20px 0; border: none; border-top: 1px solid #eee;"" />
                <p style=""font-size: 12px; color: #666;"">
                    Bu e-posta otomatik olarak gönderilmiştir. Lütfen yanıtlamayın.
                </p>
                <p style=""font-size: 12px; color: #666;"">
                    Güvenlik ekibiniz
                </p>
            ";

            return await _emailSender.SendEmailAsync(email, subject, body);
        }
        catch
        {
            return false;
        }
    }
} 