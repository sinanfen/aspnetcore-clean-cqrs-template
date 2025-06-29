using MediatR;
using Template.Application.Common.Results;

namespace Template.Application.Features.Auth.Commands.ResetPassword;

/// <summary>
/// Şifre sıfırlama komutu
/// </summary>
public class ResetPasswordCommand : IRequest<IResult<ResetPasswordResponse>>
{
    /// <summary>
    /// Kullanıcı e-posta adresi
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Şifre sıfırlama token'ı (Base64UrlEncoded)
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Yeni şifre
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Yeni şifre onayı
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Şifre sıfırlama yanıtı
/// </summary>
public class ResetPasswordResponse
{
    /// <summary>
    /// Şifre başarıyla sıfırlandı mı?
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Kullanıcı e-posta adresi
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Şifre sıfırlama tarihi
    /// </summary>
    public DateTime ResetDate { get; set; }

    /// <summary>
    /// Güvenlik bildirimi e-postası gönderildi mi?
    /// </summary>
    public bool SecurityNotificationSent { get; set; }
} 