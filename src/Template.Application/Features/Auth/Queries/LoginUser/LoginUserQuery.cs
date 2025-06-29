using MediatR;
using Template.Application.Common.Results;

namespace Template.Application.Features.Auth.Queries.LoginUser;

/// <summary>
/// Kullanıcı giriş sorgusu
/// </summary>
public class LoginUserQuery : IRequest<IResult<LoginUserResponse>>
{
    /// <summary>
    /// Kullanıcı adı veya e-posta adresi
    /// </summary>
    public string UsernameOrEmail { get; set; } = string.Empty;

    /// <summary>
    /// Şifre
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// 2FA doğrulama sorgusu (ayrı endpoint için)
/// </summary>
public class Complete2FALoginQuery : IRequest<IResult<LoginUserResponse>>
{
    /// <summary>
    /// Kullanıcı ID'si (ilk login'den alınır)
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// İki faktörlü kimlik doğrulama kodu
    /// </summary>
    public string TwoFactorCode { get; set; } = string.Empty;

    /// <summary>
    /// Bu makineyi güvenilir olarak işaretle
    /// </summary>
    public bool RememberMachine { get; set; } = false;
}

/// <summary>
/// Kullanıcı giriş yanıtı
/// </summary>
public class LoginUserResponse
{
    /// <summary>
    /// Kullanıcı ID'si
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Kullanıcı adı
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// E-posta adresi
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Tam ad
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// JWT Access Token (2FA gerektiğinde boş olur)
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh Token (2FA gerektiğinde boş olur)
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Token geçerlilik süresi (2FA gerektiğinde null olur)
    /// </summary>
    public DateTime? TokenExpires { get; set; }

    /// <summary>
    /// 2FA gerekiyor mu?
    /// </summary>
    public bool Requires2FA { get; set; }

    /// <summary>
    /// E-posta doğrulandı mı?
    /// </summary>
    public bool EmailConfirmed { get; set; }
} 