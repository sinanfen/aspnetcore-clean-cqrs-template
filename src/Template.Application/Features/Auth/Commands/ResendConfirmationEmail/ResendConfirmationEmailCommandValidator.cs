using FluentValidation;

namespace Template.Application.Features.Auth.Commands.ResendConfirmationEmail;

/// <summary>
/// Email onaylama mesajını yeniden gönderme komutu doğrulayıcısı
/// </summary>
public class ResendConfirmationEmailCommandValidator : AbstractValidator<ResendConfirmationEmailCommand>
{
    /// <summary>
    /// Constructor - Doğrulama kurallarını tanımlar
    /// </summary>
    public ResendConfirmationEmailCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email adresi gereklidir.")
            .EmailAddress()
            .WithMessage("Geçerli bir email adresi giriniz.")
            .MaximumLength(256)
            .WithMessage("Email adresi çok uzun.");
    }
} 