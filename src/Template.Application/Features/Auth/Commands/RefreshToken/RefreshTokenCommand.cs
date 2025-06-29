using MediatR;
using Template.Application.Common.Results;

namespace Template.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Access token yenileme komutu
/// </summary>
public class RefreshTokenCommand : IRequest<IResult<RefreshTokenResponse>>
{
    /// <summary>
    /// Mevcut access token
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh token
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Token yenileme yanıtı
/// </summary>
public class RefreshTokenResponse
{
    /// <summary>
    /// Yeni access token
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Yeni refresh token
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Token son kullanma tarihi
    /// </summary>
    public DateTime TokenExpires { get; set; }

    /// <summary>
    /// Kullanıcı benzersiz kimliği
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Kullanıcı adı
    /// </summary>
    public string UserName { get; set; } = string.Empty;
} 