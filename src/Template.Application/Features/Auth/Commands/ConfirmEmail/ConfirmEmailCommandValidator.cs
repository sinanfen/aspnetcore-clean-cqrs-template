using FluentValidation;

namespace Template.Application.Features.Auth.Commands.ConfirmEmail;

/// <summary>
/// Email onaylama komutu doğrulayıcısı
/// </summary>
public class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    /// <summary>
    /// Constructor - Doğrulama kurallarını tanımlar
    /// </summary>
    public ConfirmEmailCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Onaylama token'ı gereklidir.")
            .MaximumLength(1000)
            .WithMessage("Onaylama token'ı çok uzun.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email adresi gereklidir.")
            .EmailAddress()
            .WithMessage("Geçerli bir email adresi giriniz.")
            .MaximumLength(256)
            .WithMessage("Email adresi çok uzun.");
    }
} 