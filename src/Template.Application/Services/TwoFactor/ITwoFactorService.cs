using Template.Domain.Entities.Identity;

namespace Template.Application.Services.TwoFactor;

/// <summary>
/// İki faktörlü kimlik doğrulama işlemleri için servis arayüzü
/// Google Authenticator (TOTP) tabanlı
/// </summary>
public interface ITwoFactorService
{
    /// <summary>
    /// Kullanıcı için 2FA secret key oluşturur
    /// </summary>
    /// <param name="user">Kullanıcı entity'si</param>
    /// <returns>Base32 encoded secret key</returns>
    Task<string> GenerateSecretKeyAsync(AppUser user);

    /// <summary>
    /// Google Authenticator için QR kod URI'si oluşturur
    /// </summary>
    /// <param name="user">Kullanıcı entity'si</param>
    /// <param name="secretKey">Secret key</param>
    /// <param name="issuer">Uygulama adı (opsiyonel)</param>
    /// <returns>QR kod URI'si</returns>
    Task<string> GenerateQrCodeUriAsync(AppUser user, string secretKey, string? issuer = null);

    /// <summary>
    /// TOTP kodunu doğrular
    /// </summary>
    /// <param name="secretKey">Kullanıcının secret key'i</param>
    /// <param name="code">6 haneli TOTP kodu</param>
    /// <returns>Kod geçerli mi?</returns>
    Task<bool> ValidateCodeAsync(string secretKey, string code);

    /// <summary>
    /// TOTP kodunu doğrular (kullanıcı ile)
    /// </summary>
    /// <param name="user">Kullanıcı entity'si</param>
    /// <param name="secretKey">Secret key</param>
    /// <param name="totpCode">TOTP kodu</param>
    /// <returns>Kod geçerli mi?</returns>
    Task<bool> ValidateTotpAsync(AppUser user, string secretKey, string totpCode);

    /// <summary>
    /// Backup kodları oluşturur
    /// </summary>
    /// <param name="count">Oluşturulacak kod sayısı (varsayılan: 10)</param>
    /// <returns>Backup kodları listesi</returns>
    Task<string[]> GenerateBackupCodesAsync(int count = 10);

    /// <summary>
    /// Backup kodu doğrular ve kullanılmış olarak işaretler
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <param name="backupCode">Backup kod</param>
    /// <returns>Kod geçerli ve kullanılabilir mi?</returns>
    Task<bool> ValidateBackupCodeAsync(Guid userId, string backupCode);

    /// <summary>
    /// Backup kodu doğrular (kullanıcı ile)
    /// </summary>
    /// <param name="user">Kullanıcı entity'si</param>
    /// <param name="backupCode">Backup kod</param>
    /// <returns>Kod geçerli mi?</returns>
    Task<bool> ValidateBackupCodeAsync(AppUser user, string backupCode);

    /// <summary>
    /// Kullanıcının kalan backup kod sayısını döndürür
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <returns>Kalan backup kod sayısı</returns>
    Task<int> GetRemainingBackupCodesCountAsync(Guid userId);

    /// <summary>
    /// Güvenli random secret key oluşturur
    /// </summary>
    /// <param name="length">Key uzunluğu (varsayılan: 32)</param>
    /// <returns>Base32 encoded random key</returns>
    Task<string> GenerateRandomSecretAsync(int length = 32);

    /// <summary>
    /// Secret key'i Base32 formatına çevirir
    /// </summary>
    /// <param name="input">Çevrilecek byte array</param>
    /// <returns>Base32 encoded string</returns>
    string ToBase32String(byte[] input);

    /// <summary>
    /// Base32 string'i byte array'e çevirir
    /// </summary>
    /// <param name="input">Base32 encoded string</param>
    /// <returns>Byte array</returns>
    byte[] FromBase32String(string input);

    /// <summary>
    /// Machine token oluşturur (Remember Machine için)
    /// </summary>
    /// <param name="user">Kullanıcı entity'si</param>
    /// <returns>Machine token</returns>
    Task<string> GenerateMachineTokenAsync(AppUser user);
} 