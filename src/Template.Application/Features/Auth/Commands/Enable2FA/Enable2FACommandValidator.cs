using FluentValidation;

namespace Template.Application.Features.Auth.Commands.Enable2FA;

/// <summary>
/// İki faktörlü kimlik doğrulamayı etkinleştir komutu doğrulayıcısı
/// </summary>
public class Enable2FACommandValidator : AbstractValidator<Enable2FACommand>
{
    /// <summary>
    /// Constructor - Validasyon kurallarını tanımlar
    /// </summary>
    public Enable2FACommandValidator()
    {
        RuleFor(x => x.ApplicationName)
            .MaximumLength(50)
            .WithMessage("Uygulama adı en fazla 50 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.ApplicationName));
    }
} 