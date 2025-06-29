using MediatR;
using Template.Application.Common.Results;

namespace Template.Application.Features.Auth.Commands.ResendConfirmationEmail;

/// <summary>
/// Email onaylama mesajını yeniden gönderme komutu
/// </summary>
public class ResendConfirmationEmailCommand : IRequest<IResult<ResendConfirmationEmailResponse>>
{
    /// <summary>
    /// Email adresi
    /// </summary>
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Email onaylama mesajını yeniden gönderme yanıtı
/// </summary>
public class ResendConfirmationEmailResponse
{
    /// <summary>
    /// Email gönderildi mi?
    /// </summary>
    public bool EmailSent { get; set; }

    /// <summary>
    /// Kullanıcı ID'si
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Email adresi
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Email zaten onaylanmış mı?
    /// </summary>
    public bool IsAlreadyConfirmed { get; set; }
} 