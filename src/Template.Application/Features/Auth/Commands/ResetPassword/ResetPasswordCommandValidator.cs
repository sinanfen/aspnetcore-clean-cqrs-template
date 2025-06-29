using FluentValidation;

namespace Template.Application.Features.Auth.Commands.ResetPassword;

/// <summary>
/// Şifre sıfırlama komutu doğrulayıcısı
/// </summary>
public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    /// <summary>
    /// Constructor - Validasyon kurallarını tanımlar
    /// </summary>
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("E-posta adresi gereklidir.")
            .EmailAddress()
            .WithMessage("Geçerli bir e-posta adresi giriniz.")
            .MaximumLength(256)
            .WithMessage("E-posta adresi en fazla 256 karakter olabilir.");

        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Token gereklidir.")
            .MinimumLength(10)
            .WithMessage("Token çok kısa.")
            .MaximumLength(2048)
            .WithMessage("Token çok uzun.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("Yeni şifre gereklidir.")
            .MinimumLength(8)
            .WithMessage("Şifre en az 8 karakter olmalıdır.")
            .MaximumLength(128)
            .WithMessage("Şifre en fazla 128 karakter olabilir.")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$")
            .WithMessage("Şifre en az bir küçük harf, bir büyük harf, bir rakam ve bir özel karakter içermelidir.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithMessage("Şifre onayı gereklidir.")
            .Equal(x => x.NewPassword)
            .WithMessage("Şifre onayı, yeni şifre ile aynı olmalıdır.");
    }
} 