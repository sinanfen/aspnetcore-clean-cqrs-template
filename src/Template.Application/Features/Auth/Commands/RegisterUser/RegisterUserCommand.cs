using MediatR;
using Template.Application.Common.Results;

namespace Template.Application.Features.Auth.Commands.RegisterUser;

/// <summary>
/// Yeni kullanıcı kayıt komutu
/// </summary>
public class RegisterUserCommand : IRequest<IResult<RegisterUserResponse>>
{
    /// <summary>
    /// Kullanıcı adı (opsiyonel - boş bırakılırsa email'den otomatik generate edilir)
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// E-posta adresi
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Şifre
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Şifre onayı
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// Ad
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Soyad
    /// </summary>
    public string LastName { get; set; } = string.Empty;
}

/// <summary>
/// Kullanıcı kayıt yanıtı
/// </summary>
public class RegisterUserResponse
{
    /// <summary>
    /// Oluşturulan kullanıcının benzersiz kimliği
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
    /// E-posta doğrulama token'ı gönderildi mi?
    /// </summary>
    public bool EmailConfirmationSent { get; set; }
} 