using MediatR;
using Template.Application.Common.Results;

namespace Template.Application.Features.Auth.Commands.ForgotPassword;

/// <summary>
/// Şifre sıfırlama talebi komutu
/// </summary>
public class ForgotPasswordCommand : IRequest<IResult<ForgotPasswordResponse>>
{
    /// <summary>
    /// Kullanıcı e-posta adresi
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Şifre sıfırlama sayfasının URL'i (client tarafından sağlanır)
    /// </summary>
    public string ResetUrl { get; set; } = string.Empty;
}

/// <summary>
/// Şifre sıfırlama talebi yanıtı
/// </summary>
public class ForgotPasswordResponse
{
    /// <summary>
    /// E-posta gönderildi mi?
    /// </summary>
    public bool EmailSent { get; set; }

    /// <summary>
    /// Kullanıcı e-posta adresi
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Şifre sıfırlama token'ının son kullanma tarihi
    /// </summary>
    public DateTime TokenExpires { get; set; }

    /// <summary>
    /// Token geçerlilik süresi (dakika)
    /// </summary>
    public int TokenValidityMinutes { get; set; }
} 