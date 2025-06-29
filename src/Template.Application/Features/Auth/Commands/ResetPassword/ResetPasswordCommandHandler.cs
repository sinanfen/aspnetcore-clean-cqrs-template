using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Template.Application.Common.Results;
using Template.Application.Services.Email;
using Template.Domain.Entities.Identity;

namespace Template.Application.Features.Auth.Commands.ResetPassword;

/// <summary>
/// Şifre sıfırlama komutu işleyicisi
/// </summary>
public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, IResult<ResetPasswordResponse>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailSender _emailSender;

    /// <summary>
    /// Constructor - Gerekli servisler dependency injection ile alınır
    /// </summary>
    /// <param name="userManager">ASP.NET Identity kullanıcı yöneticisi</param>
    /// <param name="emailSender">E-posta gönderim servisi</param>
    public ResetPasswordCommandHandler(
        UserManager<AppUser> userManager, 
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }

    /// <summary>
    /// Şifre sıfırlama işlemini gerçekleştirir
    /// </summary>
    /// <param name="request">ResetPassword komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Şifre sıfırlama sonucu</returns>
    public async Task<IResult<ResetPasswordResponse>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Kullanıcıyı e-posta ile bul
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result.Failure<ResetPasswordResponse>("Kullanıcı bulunamadı.");
            }

            // E-posta doğrulanmış mı kontrol et
            if (!user.EmailConfirmed)
            {
                return Result.Failure<ResetPasswordResponse>("E-posta adresi doğrulanmadan şifre sıfırlanamaz.");
            }

            // Token'ı decode et
            byte[] tokenBytes;
            try
            {
                tokenBytes = WebEncoders.Base64UrlDecode(request.Token);
            }
            catch
            {
                return Result.Failure<ResetPasswordResponse>("Geçersiz token formatı.");
            }

            var decodedToken = Encoding.UTF8.GetString(tokenBytes);

            // Şifreyi sıfırla
            var resetResult = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);
            
            if (!resetResult.Succeeded)
            {
                var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
                return Result.Failure<ResetPasswordResponse>($"Şifre sıfırlama başarısız: {errors}");
            }

            // Tüm refresh token'ları revoke et (güvenlik için)
            await RevokeAllRefreshTokensAsync(user);

            // Güvenlik bildirimi e-postası gönder
            var resetDate = DateTime.UtcNow;
            var notificationSent = await SendPasswordResetNotificationAsync(user.Email!, user.FullName, resetDate);

            // Yanıt oluştur
            var response = new ResetPasswordResponse
            {
                IsSuccess = true,
                Email = user.Email!,
                ResetDate = resetDate,
                SecurityNotificationSent = notificationSent
            };

            return Result.Success(response, "Şifreniz başarıyla sıfırlandı. Güvenlik bildirimi e-posta adresinize gönderildi.");
        }
        catch (Exception ex)
        {
            return Result.Failure<ResetPasswordResponse>($"Şifre sıfırlama sırasında hata oluştu: {ex.Message}");
        }
    }

    /// <summary>
    /// Kullanıcının tüm refresh token'larını revoke eder
    /// </summary>
    /// <param name="user">Kullanıcı</param>
    private async Task RevokeAllRefreshTokensAsync(AppUser user)
    {
        try
        {
            // Kullanıcının tüm aktif refresh token'larını revoke et
            foreach (var refreshToken in user.RefreshTokens.Where(rt => rt.IsActive))
            {
                refreshToken.IsRevoked = true;
            }

            await _userManager.UpdateAsync(user);
        }
        catch
        {
            // Hata durumunda sessizce devam et
        }
    }

    /// <summary>
    /// Şifre sıfırlama güvenlik bildirimi e-postası gönderir
    /// </summary>
    /// <param name="email">Kullanıcı e-posta adresi</param>
    /// <param name="fullName">Kullanıcı adı soyadı</param>
    /// <param name="resetDate">Şifre sıfırlama tarihi</param>
    /// <returns>E-posta gönderim durumu</returns>
    private async Task<bool> SendPasswordResetNotificationAsync(string email, string fullName, DateTime resetDate)
    {
        try
        {
            var subject = "Şifreniz Başarıyla Sıfırlandı";
            
            var body = $@"
                <h2>Şifre Sıfırlama Bildirimi</h2>
                <p>Merhaba {fullName},</p>
                <p>Hesabınızın şifresi başarıyla sıfırlandı.</p>
                
                <h3>İşlem Detayları</h3>
                <ul>
                    <li><strong>E-posta:</strong> {email}</li>
                    <li><strong>İşlem Tarihi:</strong> {resetDate:dd.MM.yyyy HH:mm} (UTC)</li>
                    <li><strong>İşlem Türü:</strong> Şifre Sıfırlama</li>
                </ul>
                
                <h3>Güvenlik Bildirimi</h3>
                <p>Güvenlik amacıyla tüm oturum token'larınız iptal edildi. Yeni şifrenizle tekrar giriş yapmanız gerekecek.</p>
                
                <h3>Bu İşlemi Siz Yapmadıysanız</h3>
                <div style=""background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; margin: 15px 0; border-radius: 4px;"">
                    <p><strong>⚠️ Dikkat:</strong> Bu işlemi siz yapmadıysanız, hesabınız tehlikede olabilir.</p>
                    <p>Derhal aşağıdaki adımları uygulayın:</p>
                    <ul>
                        <li>Hesabınıza erişim sağlayabilirseniz şifrenizi tekrar değiştirin</li>
                        <li>İki faktörlü kimlik doğrulamayı etkinleştirin</li>
                        <li>Hesap aktivitelerinizi kontrol edin</li>
                        <li>Güvenlik ekibimizle iletişime geçin</li>
                    </ul>
                </div>
                
                <h3>Güvenlik Önerileri</h3>
                <ul>
                    <li>Güçlü ve benzersiz şifreler kullanın</li>
                    <li>İki faktörlü kimlik doğrulamayı etkinleştirin</li>
                    <li>Şifrenizi düzenli olarak değiştirin</li>
                    <li>Şüpheli aktiviteleri bildirin</li>
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