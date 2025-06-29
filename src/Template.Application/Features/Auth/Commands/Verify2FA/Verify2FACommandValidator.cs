using FluentValidation;

namespace Template.Application.Features.Auth.Commands.Verify2FA;

/// <summary>
/// İki faktörlü kimlik doğrulama token doğrulama komutu doğrulayıcısı
/// </summary>
public class Verify2FACommandValidator : AbstractValidator<Verify2FACommand>
{
    /// <summary>
    /// Constructor - Validasyon kurallarını tanımlar
    /// </summary>
    public Verify2FACommandValidator()
    {     
        RuleFor(x => x.TotpCode)
            .NotEmpty()
            .WithMessage("TOTP kodu gereklidir.")
            .Length(6)
            .WithMessage("TOTP kodu 6 haneli olmalıdır.")
            .Matches(@"^\d{6}$")
            .WithMessage("TOTP kodu yalnızca rakamlardan oluşmalıdır.");

        RuleFor(x => x.SecretKey)
            .NotEmpty()
            .WithMessage("Secret key gereklidir.")
            .MinimumLength(16)
            .WithMessage("Secret key en az 16 karakter olmalıdır.")
            .Matches(@"^[A-Z2-7]+$")
            .WithMessage("Secret key Base32 formatında olmalıdır (A-Z ve 2-7 karakterleri).");
    }
} 