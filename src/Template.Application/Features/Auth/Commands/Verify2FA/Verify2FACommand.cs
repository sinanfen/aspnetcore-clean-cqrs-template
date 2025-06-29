using MediatR;
using Template.Application.Common.Results;

namespace Template.Application.Features.Auth.Commands.Verify2FA;

/// <summary>
/// İki faktörlü kimlik doğrulama token doğrulama komutu
/// </summary>
public class Verify2FACommand : IRequest<IResult<Verify2FAResponse>>
{
    /// <summary>
    /// Authenticator uygulamasından alınan 6 haneli kod
    /// </summary>
    public string TotpCode { get; set; } = string.Empty;

    /// <summary>
    /// Secret key (Base32 format)
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Bu makineyi güvenilir olarak işaretle
    /// </summary>
    public bool RememberMachine { get; set; } = false;
}

/// <summary>
/// İki faktörlü kimlik doğrulama token doğrulama yanıtı
/// </summary>
public class Verify2FAResponse
{
    /// <summary>
    /// Doğrulama başarılı mı?
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// 2FA başarıyla etkinleştirildi mi?
    /// </summary>
    public bool Is2FAEnabled { get; set; }

    /// <summary>
    /// Kullanılan backup kodlar (varsa)
    /// </summary>
    public string[] UsedBackupCodes { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Kalan backup kod sayısı
    /// </summary>
    public int RemainingBackupCodes { get; set; }

    /// <summary>
    /// Machine token (RememberMachine true ise)
    /// </summary>
    public string? MachineToken { get; set; }
} 