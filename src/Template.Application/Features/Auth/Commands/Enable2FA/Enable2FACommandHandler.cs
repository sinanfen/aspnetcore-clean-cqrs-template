using MediatR;
using Microsoft.AspNetCore.Identity;
using Template.Application.Common.Results;
using Template.Application.Services.Email;
using Template.Application.Services.TwoFactor;
using Template.Application.Services.User;
using Template.Domain.Entities.Identity;

namespace Template.Application.Features.Auth.Commands.Enable2FA;

/// <summary>
/// İki faktörlü kimlik doğrulamayı etkinleştir komutu işleyicisi
/// </summary>
public class Enable2FACommandHandler : IRequestHandler<Enable2FACommand, IResult<Enable2FAResponse>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IEmailSender _emailSender;
    private readonly IUserAccessor _userAccessor;

    /// <summary>
    /// Constructor - Gerekli servisler dependency injection ile alınır
    /// </summary>
    /// <param name="userManager">ASP.NET Identity kullanıcı yöneticisi</param>
    /// <param name="twoFactorService">İki faktörlü kimlik doğrulama servisi</param>
    /// <param name="emailSender">E-posta gönderim servisi</param>
    /// <param name="userAccessor">Kullanıcı erişim servisi</param>
    public Enable2FACommandHandler(
        UserManager<AppUser> userManager, 
        ITwoFactorService twoFactorService, 
        IEmailSender emailSender,
        IUserAccessor userAccessor)
    {
        _userManager = userManager;
        _twoFactorService = twoFactorService;
        _emailSender = emailSender;
        _userAccessor = userAccessor;
    }

    /// <summary>
    /// İki faktörlü kimlik doğrulamayı etkinleştir işlemini gerçekleştirir
    /// </summary>
    /// <param name="request">Enable2FA komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>QR kod URI'si ve backup kodları</returns>
    public async Task<IResult<Enable2FAResponse>> Handle(Enable2FACommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Kullanıcının kimlik doğrulaması yapılmış mı kontrol et
            if (!_userAccessor.IsAuthenticated())
            {
                return Result.Failure<Enable2FAResponse>("Kimlik doğrulaması yapılmamış.");
            }

            // JWT token'dan kullanıcı ID'sini al
            var userId = _userAccessor.GetUserId();
            if (!userId.HasValue)
            {
                return Result.Failure<Enable2FAResponse>("Kullanıcı bilgisi alınamadı.");
            }

            // Kullanıcıyı bul
            var user = await _userManager.FindByIdAsync(userId.Value.ToString());
            if (user == null)
            {
                return Result.Failure<Enable2FAResponse>("Kullanıcı bulunamadı.");
            }

            // E-posta doğrulanmış mı kontrol et
            if (!user.EmailConfirmed)
            {
                return Result.Failure<Enable2FAResponse>("E-posta adresi doğrulanmadan 2FA etkinleştirilemez.");
            }

            // Zaten 2FA aktif mi?
            if (user.Is2FAEnabled)
            {
                return Result.Failure<Enable2FAResponse>("İki faktörlü kimlik doğrulama zaten etkin.");
            }

            // Secret key oluştur
            var secretKey = await _twoFactorService.GenerateSecretKeyAsync(user);

            // QR kod URI'si oluştur
            var applicationName = request.ApplicationName ?? "Template App";
            var qrCodeUri = await _twoFactorService.GenerateQrCodeUriAsync(user, secretKey, applicationName);

            // Backup kodları oluştur
            var backupCodes = await _twoFactorService.GenerateBackupCodesAsync(10);

            // Secret key'i kullanıcıya kaydet (henüz 2FA aktif değil)
            user.TwoFactorSecretKey = secretKey;
            var updateResult = await _userManager.UpdateAsync(user);
            
            if (!updateResult.Succeeded)
            {
                return Result.Failure<Enable2FAResponse>("Secret key kaydedilemedi: " + 
                    string.Join(", ", updateResult.Errors.Select(e => e.Description)));
            }

            // Kullanıcıya 2FA bilgilerini e-posta ile gönder
            var emailSent = await SendTwoFactorSetupEmailAsync(user.Email!, qrCodeUri, secretKey, backupCodes);

            // Yanıt oluştur
            var response = new Enable2FAResponse
            {
                QrCodeUri = qrCodeUri,
                SecretKey = secretKey,
                BackupCodes = backupCodes,
                Email = user.Email!,
                EmailSent = emailSent
            };

            return Result.Success(response, "İki faktörlü kimlik doğrulama kurulum bilgileri hazırlandı. Lütfen e-postanızı kontrol edin.");
        }
        catch (Exception ex)
        {
            return Result.Failure<Enable2FAResponse>($"2FA etkinleştirme sırasında hata oluştu: {ex.Message}");
        }
    }

    /// <summary>
    /// 2FA kurulum bilgilerini e-posta ile gönderir
    /// </summary>
    /// <param name="email">Kullanıcı e-posta adresi</param>
    /// <param name="qrCodeUri">QR kod URI'si</param>
    /// <param name="secretKey">Secret key</param>
    /// <param name="backupCodes">Backup kodları</param>
    /// <returns>E-posta gönderim durumu</returns>
    private async Task<bool> SendTwoFactorSetupEmailAsync(string email, string qrCodeUri, string secretKey, string[] backupCodes)
    {
        try
        {
            var subject = "İki Faktörlü Kimlik Doğrulama Kurulumu";
            var backupCodesHtml = string.Join("<br/>", backupCodes.Select(code => $"• {code}"));
            
            var body = $@"
                <h2>İki Faktörlü Kimlik Doğrulama Kurulumu</h2>
                <p>Merhaba,</p>
                <p>Hesabınız için iki faktörlü kimlik doğrulama kurulumu başlatıldı.</p>
                
                <h3>Adım 1: Authenticator Uygulaması Yükleyin</h3>
                <p>Google Authenticator, Microsoft Authenticator veya benzer bir TOTP uygulaması yükleyin.</p>
                
                <h3>Adım 2: QR Kodu Tarayın</h3>
                <p>Aşağıdaki QR kodu authenticator uygulamanızla tarayın:</p>
                <p><strong>QR Kod URI:</strong> {qrCodeUri}</p>
                
                <h3>Adım 3: Manuel Giriş (Alternatif)</h3>
                <p>QR kod çalışmıyorsa manuel olarak şu kodu girebilirsiniz:</p>
                <p><strong>Secret Key:</strong> {secretKey}</p>
                
                <h3>Backup Kodları</h3>
                <p>Aşağıdaki backup kodlarını güvenli bir yerde saklayın. Her biri yalnızca bir kez kullanılabilir:</p>
                <div style=""background-color: #f5f5f5; padding: 10px; margin: 10px 0; font-family: monospace;"">
                    {backupCodesHtml}
                </div>
                
                <p><strong>Önemli:</strong> Kurulumu tamamlamak için authenticator uygulamanızda görünen 6 haneli kodu girerek doğrulama yapmanız gerekmektedir.</p>
                
                <p>Güvenlik ekibiniz</p>
            ";

            return await _emailSender.SendEmailAsync(email, subject, body);
        }
        catch
        {
            return false;
        }
    }
} 