using FluentValidation;

namespace Template.Application.Features.Auth.Commands.ForgotPassword;

/// <summary>
/// Şifre sıfırlama talebi komutu doğrulayıcısı
/// </summary>
public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    /// <summary>
    /// Constructor - Validasyon kurallarını tanımlar
    /// </summary>
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("E-posta adresi gereklidir.")
            .EmailAddress()
            .WithMessage("Geçerli bir e-posta adresi giriniz.")
            .MaximumLength(256)
            .WithMessage("E-posta adresi en fazla 256 karakter olabilir.");

        RuleFor(x => x.ResetUrl)
            .NotEmpty()
            .WithMessage("Şifre sıfırlama URL'i gereklidir.")
            .Must(BeValidUrl)
            .WithMessage("Geçerli bir URL giriniz.")
            .MaximumLength(2048)
            .WithMessage("URL en fazla 2048 karakter olabilir.");
    }

    /// <summary>
    /// URL formatını doğrular
    /// </summary>
    /// <param name="url">Doğrulanacak URL</param>
    /// <returns>URL geçerli mi?</returns>
    private static bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var result) 
               && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
} 