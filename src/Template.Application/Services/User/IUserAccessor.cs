namespace Template.Application.Services.User;

/// <summary>
/// Aktif kullanıcı bilgilerine erişim için servis arayüzü
/// HttpContext üzerinden kullanıcı bilgilerini alır
/// </summary>
public interface IUserAccessor
{
    /// <summary>
    /// Aktif kullanıcının ID'sini döndürür
    /// </summary>
    /// <returns>Kullanıcı ID'si veya null</returns>
    Guid? GetUserId();

    /// <summary>
    /// Aktif kullanıcının kullanıcı adını döndürür
    /// </summary>
    /// <returns>Kullanıcı adı veya null</returns>
    string? GetUserName();

    /// <summary>
    /// Aktif kullanıcının e-posta adresini döndürür
    /// </summary>
    /// <returns>E-posta adresi veya null</returns>
    string? GetUserEmail();

    /// <summary>
    /// Aktif kullanıcının tam adını döndürür
    /// </summary>
    /// <returns>Tam ad veya null</returns>
    string? GetFullName();

    /// <summary>
    /// Kullanıcının belirli bir role sahip olup olmadığını kontrol eder
    /// </summary>
    /// <param name="role">Kontrol edilecek rol adı</param>
    /// <returns>Kullanıcının bu role sahip olup olmadığı</returns>
    bool IsInRole(string role);

    /// <summary>
    /// Kullanıcının belirli rollerin herhangi birine sahip olup olmadığını kontrol eder
    /// </summary>
    /// <param name="roles">Kontrol edilecek rol adları</param>
    /// <returns>Kullanıcının bu rollerden herhangi birine sahip olup olmadığı</returns>
    bool IsInAnyRole(params string[] roles);

    /// <summary>
    /// Kullanıcının tüm rollerini döndürür
    /// </summary>
    /// <returns>Kullanıcının rolleri</returns>
    IEnumerable<string> GetUserRoles();

    /// <summary>
    /// Kullanıcının kimlik doğrulaması yapılmış mı?
    /// </summary>
    /// <returns>Kimlik doğrulaması yapılmış mı?</returns>
    bool IsAuthenticated();

    /// <summary>
    /// Kullanıcının e-posta adresi doğrulanmış mı?
    /// </summary>
    /// <returns>E-posta doğrulanmış mı?</returns>
    bool IsEmailConfirmed();

    /// <summary>
    /// Kullanıcının 2FA aktif mi?
    /// </summary>
    /// <returns>2FA aktif mi?</returns>
    bool Is2FAEnabled();

    /// <summary>
    /// Belirli bir claim'e sahip olup olmadığını kontrol eder
    /// </summary>
    /// <param name="claimType">Claim tipi</param>
    /// <param name="claimValue">Claim değeri (opsiyonel)</param>
    /// <returns>Claim'e sahip olup olmadığı</returns>
    bool HasClaim(string claimType, string? claimValue = null);

    /// <summary>
    /// Belirli bir claim'in değerini döndürür
    /// </summary>
    /// <param name="claimType">Claim tipi</param>
    /// <returns>Claim değeri veya null</returns>
    string? GetClaimValue(string claimType);

    /// <summary>
    /// JWT token'dan kullanıcı bilgilerini döndürür
    /// </summary>
    /// <returns>JWT token payload'ı veya null</returns>
    Dictionary<string, object>? GetTokenPayload();
} 