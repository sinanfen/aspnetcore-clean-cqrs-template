using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Template.Application.Services.Token;
using Template.Domain.Entities.Identity;
using Template.Infrastructure.Configuration;

namespace Template.Infrastructure.Services.Token;

/// <summary>
/// JWT Token işlemleri için servis implementasyonu
/// </summary>
public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    /// <summary>
    /// Constructor - JWT ayarları dependency injection ile alınır
    /// Validation IValidateOptions<JwtSettings> ile yapılır
    /// </summary>
    /// <param name="jwtSettings">JWT konfigürasyonu</param>
    public TokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
        _tokenHandler = new JwtSecurityTokenHandler();

        // Validation IValidateOptions ile yapılıyor, burada ekstra kontrol gerekmiyor
        // Development ortamında JwtSettingsValidator fallback değerler sağlıyor
    }

    /// <summary>
    /// Kullanıcı için access token oluşturur
    /// </summary>
    /// <param name="user">Kullanıcı entity'si</param>
    /// <param name="roles">Kullanıcının rolleri</param>
    /// <returns>JWT access token</returns>
    public async Task<string> GenerateAccessTokenAsync(AppUser user, IList<string>? roles = null)
    {
        var claims = await CreateClaimsAsync(user, roles);
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var tokenExpiration = GetAccessTokenExpirationTime();
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = tokenExpiration,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = credentials
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        return _tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Kullanıcı için refresh token oluşturur
    /// </summary>
    /// <param name="user">Kullanıcı entity'si</param>
    /// <returns>Refresh token</returns>
    public async Task<RefreshToken> GenerateRefreshTokenAsync(AppUser user)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        
        var refreshToken = new RefreshToken
        {
            AppUserId = user.Id,
            Token = Convert.ToBase64String(randomBytes),
            Expires = GetRefreshTokenExpirationTime(),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        return await Task.FromResult(refreshToken);
    }

    /// <summary>
    /// JWT token'ı doğrular ve claim'leri döndürür
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <returns>Token geçerli ise ClaimsPrincipal, değilse null</returns>
    public async Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // Token süresinde tolerans yok
            };

            var principal = _tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
            
            if (validatedToken is JwtSecurityToken jwtToken &&
                jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return await Task.FromResult(principal);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// JWT token'dan kullanıcı ID'sini çıkarır
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <returns>Kullanıcı ID'si veya null</returns>
    public async Task<Guid?> GetUserIdFromTokenAsync(string token)
    {
        var principal = await ValidateTokenAsync(token);
        if (principal == null) return null;

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }

    /// <summary>
    /// Token'ın süresinin dolup dolmadığını kontrol eder
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <returns>Token süresi dolmuş mu?</returns>
    public async Task<bool> IsTokenExpiredAsync(string token)
    {
        try
        {
            var jwtToken = _tokenHandler.ReadJwtToken(token);
            return await Task.FromResult(jwtToken.ValidTo <= DateTime.UtcNow);
        }
        catch
        {
            return true; // Token okunamıyorsa expired kabul et
        }
    }

    /// <summary>
    /// Access token için geçerlilik süresini hesaplar
    /// </summary>
    /// <returns>Token bitiş zamanı</returns>
    public DateTime GetAccessTokenExpirationTime()
    {
        return DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
    }

    /// <summary>
    /// Refresh token için geçerlilik süresini hesaplar
    /// </summary>
    /// <returns>Refresh token bitiş zamanı</returns>
    public DateTime GetRefreshTokenExpirationTime()
    {
        return DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
    }

    /// <summary>
    /// Kullanıcı için JWT claim'lerini oluşturur
    /// </summary>
    /// <param name="user">Kullanıcı entity'si</param>
    /// <param name="roles">Kullanıcının rolleri</param>
    /// <returns>Claim listesi</returns>
    private async Task<List<Claim>> CreateClaimsAsync(AppUser user, IList<string>? roles = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName),
            new("FullName", user.FullName),
            new("EmailConfirmed", user.EmailConfirmed.ToString()),
            new("2FAEnabled", user.Is2FAEnabled.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Rolleri ekle
        if (roles != null)
        {
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        return await Task.FromResult(claims);
    }
} 