using MediatR;
using Template.Application.Common.Results;

namespace Template.Application.Features.Auth.Commands.ConfirmEmail;

/// <summary>
/// Email onaylama komutu
/// </summary>
public class ConfirmEmailCommand : IRequest<IResult<ConfirmEmailResponse>>
{
    /// <summary>
    /// Email onaylama token'ı
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Onaylanacak email adresi
    /// </summary>
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Email onaylama yanıtı
/// </summary>
public class ConfirmEmailResponse
{
    /// <summary>
    /// Email onaylandı mı?
    /// </summary>
    public bool IsConfirmed { get; set; }

    /// <summary>
    /// Kullanıcı ID'si
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Email adresi
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Kullanıcı adı
    /// </summary>
    public string UserName { get; set; } = string.Empty;
} 