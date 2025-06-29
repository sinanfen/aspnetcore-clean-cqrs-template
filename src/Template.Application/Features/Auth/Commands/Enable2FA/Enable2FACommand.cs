using MediatR;
using Template.Application.Common.Results;

namespace Template.Application.Features.Auth.Commands.Enable2FA;

/// <summary>
/// İki faktörlü kimlik doğrulamayı etkinleştir komutu
/// </summary>
public class Enable2FACommand : IRequest<IResult<Enable2FAResponse>>
{
    /// <summary>
    /// Uygulama adı (QR kodda görünecek)
    /// </summary>
    public string? ApplicationName { get; set; }
}

/// <summary>
/// İki faktörlü kimlik doğrulama etkinleştirme yanıtı
/// </summary>
public class Enable2FAResponse
{
    /// <summary>
    /// QR kod URI'si (Google Authenticator için)
    /// </summary>
    public string QrCodeUri { get; set; } = string.Empty;

    /// <summary>
    /// Manual giriş için secret key (Base32 format)
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Backup kodları listesi
    /// </summary>
    public string[] BackupCodes { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Kullanıcı e-posta adresi
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// QR kod bilgileri e-posta ile gönderildi mi?
    /// </summary>
    public bool EmailSent { get; set; }
} 