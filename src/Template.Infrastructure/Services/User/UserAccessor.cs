using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Template.Application.Services.User;

namespace Template.Infrastructure.Services.User;

/// <summary>
/// Aktif kullanıcı bilgilerine erişim servisi implementasyonu
/// IHttpContextAccessor üzerinden çalışır
/// </summary>
public class UserAccessor : IUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Constructor - HttpContextAccessor dependency injection ile alınır
    /// </summary>
    /// <param name="httpContextAccessor">HttpContext erişim arayüzü</param>
    public UserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Aktif kullanıcının ID'sini döndürür
    /// </summary>
    /// <returns>Kullanıcı ID'si veya null</returns>
    public Guid? GetUserId()
    {
        var userIdClaim = GetClaimValue(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }

    /// <summary>
    /// Aktif kullanıcının kullanıcı adını döndürür
    /// </summary>
    /// <returns>Kullanıcı adı veya null</returns>
    public string? GetUserName()
    {
        return GetClaimValue(ClaimTypes.Name);
    }

    /// <summary>
    /// Aktif kullanıcının e-posta adresini döndürür
    /// </summary>
    /// <returns>E-posta adresi veya null</returns>
    public string? GetUserEmail()
    {
        return GetClaimValue(ClaimTypes.Email);
    }

    /// <summary>
    /// Aktif kullanıcının tam adını döndürür
    /// </summary>
    /// <returns>Tam ad veya null</returns>
    public string? GetFullName()
    {
        return GetClaimValue("FullName");
    }

    /// <summary>
    /// Kullanıcının belirli bir role sahip olup olmadığını kontrol eder
    /// </summary>
    /// <param name="role">Kontrol edilecek rol adı</param>
    /// <returns>Kullanıcının bu role sahip olup olmadığı</returns>
    public bool IsInRole(string role)
    {
        return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    }

    /// <summary>
    /// Kullanıcının belirli rollerin herhangi birine sahip olup olmadığını kontrol eder
    /// </summary>
    /// <param name="roles">Kontrol edilecek rol adları</param>
    /// <returns>Kullanıcının bu rollerden herhangi birine sahip olup olmadığı</returns>
    public bool IsInAnyRole(params string[] roles)
    {
        return roles.Any(role => IsInRole(role));
    }

    /// <summary>
    /// Kullanıcının tüm rollerini döndürür
    /// </summary>
    /// <returns>Kullanıcının rolleri</returns>
    public IEnumerable<string> GetUserRoles()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) return Enumerable.Empty<string>();

        return user.FindAll(ClaimTypes.Role).Select(c => c.Value);
    }

    /// <summary>
    /// Kullanıcının kimlik doğrulaması yapılmış mı?
    /// </summary>
    /// <returns>Kimlik doğrulaması yapılmış mı?</returns>
    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }

    /// <summary>
    /// Kullanıcının e-posta adresi doğrulanmış mı?
    /// </summary>
    /// <returns>E-posta doğrulanmış mı?</returns>
    public bool IsEmailConfirmed()
    {
        var emailConfirmedClaim = GetClaimValue("EmailConfirmed");
        return bool.TryParse(emailConfirmedClaim, out var isConfirmed) && isConfirmed;
    }

    /// <summary>
    /// Kullanıcının 2FA aktif mi?
    /// </summary>
    /// <returns>2FA aktif mi?</returns>
    public bool Is2FAEnabled()
    {
        var twoFAEnabledClaim = GetClaimValue("2FAEnabled");
        return bool.TryParse(twoFAEnabledClaim, out var isEnabled) && isEnabled;
    }

    /// <summary>
    /// Belirli bir claim'e sahip olup olmadığını kontrol eder
    /// </summary>
    /// <param name="claimType">Claim tipi</param>
    /// <param name="claimValue">Claim değeri (opsiyonel)</param>
    /// <returns>Claim'e sahip olup olmadığı</returns>
    public bool HasClaim(string claimType, string? claimValue = null)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) return false;

        if (claimValue != null)
        {
            return user.HasClaim(claimType, claimValue);
        }

        return user.Claims.Any(c => c.Type == claimType);
    }

    /// <summary>
    /// Belirli bir claim'in değerini döndürür
    /// </summary>
    /// <param name="claimType">Claim tipi</param>
    /// <returns>Claim değeri veya null</returns>
    public string? GetClaimValue(string claimType)
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(claimType)?.Value;
    }

    /// <summary>
    /// JWT token'dan kullanıcı bilgilerini döndürür
    /// </summary>
    /// <returns>JWT token payload'ı veya null</returns>
    public Dictionary<string, object>? GetTokenPayload()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) return null;

        var payload = new Dictionary<string, object>();

        foreach (var claim in user.Claims)
        {
            if (payload.ContainsKey(claim.Type))
            {
                // Aynı tip claim'den birden fazla varsa array yap
                if (payload[claim.Type] is string existingValue)
                {
                    payload[claim.Type] = new[] { existingValue, claim.Value };
                }
                else if (payload[claim.Type] is string[] existingArray)
                {
                    var newArray = new string[existingArray.Length + 1];
                    existingArray.CopyTo(newArray, 0);
                    newArray[existingArray.Length] = claim.Value;
                    payload[claim.Type] = newArray;
                }
            }
            else
            {
                payload[claim.Type] = claim.Value;
            }
        }

        return payload;
    }
} 