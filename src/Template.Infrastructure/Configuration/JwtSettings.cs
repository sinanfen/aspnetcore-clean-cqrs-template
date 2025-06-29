namespace Template.Infrastructure.Configuration;

/// <summary>
/// JWT Token ayarları konfigürasyonu
/// appsettings.json'dan yüklenir
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// appsettings.json'daki section adı
    /// </summary>
    public const string SectionName = "JwtSettings";

    /// <summary>
    /// Token'ı imzalayan gizli anahtar
    /// Minimum 32 karakter olmalıdır
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Token'ı yayınlayan (issuer)
    /// Genellikle uygulama adı veya domain
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Token'ın hedef kitlesi (audience)
    /// Genellikle client uygulama adı
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Access token geçerlilik süresi (dakika)
    /// Varsayılan: 15 dakika
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    /// <summary>
    /// Refresh token geçerlilik süresi (gün)
    /// Varsayılan: 7 gün
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;

    /// <summary>
    /// Token'da kullanılacak şifreleme algoritması
    /// Varsayılan: HS256
    /// </summary>
    public string Algorithm { get; set; } = "HS256";

    /// <summary>
    /// JWT ayarlarının geçerli olup olmadığını kontrol eder
    /// </summary>
    /// <returns>Ayarlar geçerli mi?</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(SecretKey) &&
               SecretKey.Length >= 32 &&
               !string.IsNullOrWhiteSpace(Issuer) &&
               !string.IsNullOrWhiteSpace(Audience) &&
               AccessTokenExpirationMinutes > 0 &&
               RefreshTokenExpirationDays > 0;
    }
} 