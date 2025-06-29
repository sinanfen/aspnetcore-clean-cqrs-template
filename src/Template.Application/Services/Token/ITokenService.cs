using System.Security.Claims;
using Template.Domain.Entities.Identity;

namespace Template.Application.Services.Token;

/// <summary>
/// JWT Token işlemleri için servis arayüzü
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Kullanıcı için access token oluşturur
    /// </summary>
    /// <param name="user">Kullanıcı entity'si</param>
    /// <param name="roles">Kullanıcının rolleri</param>
    /// <returns>JWT access token</returns>
    Task<string> GenerateAccessTokenAsync(AppUser user, IList<string>? roles = null);

    /// <summary>
    /// Kullanıcı için refresh token oluşturur
    /// </summary>
    /// <param name="user">Kullanıcı entity'si</param>
    /// <returns>Refresh token</returns>
    Task<RefreshToken> GenerateRefreshTokenAsync(AppUser user);

    /// <summary>
    /// JWT token'ı doğrular ve claim'leri döndürür
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <returns>Token geçerli ise ClaimsPrincipal, değilse null</returns>
    Task<ClaimsPrincipal?> ValidateTokenAsync(string token);

    /// <summary>
    /// JWT token'dan kullanıcı ID'sini çıkarır
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <returns>Kullanıcı ID'si veya null</returns>
    Task<Guid?> GetUserIdFromTokenAsync(string token);

    /// <summary>
    /// Token'ın süresinin dolup dolmadığını kontrol eder
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <returns>Token süresi dolmuş mu?</returns>
    Task<bool> IsTokenExpiredAsync(string token);

    /// <summary>
    /// Access token için geçerlilik süresini hesaplar
    /// </summary>
    /// <returns>Token bitiş zamanı</returns>
    DateTime GetAccessTokenExpirationTime();

    /// <summary>
    /// Refresh token için geçerlilik süresini hesaplar
    /// </summary>
    /// <returns>Refresh token bitiş zamanı</returns>
    DateTime GetRefreshTokenExpirationTime();
} 