using FluentValidation;

namespace Template.Application.Features.Auth.Commands.RegisterUser;

/// <summary>
/// Kullanıcı kayıt komutu doğrulama kuralları
/// </summary>
public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    /// <summary>
    /// Constructor - Doğrulama kuralları tanımlanır
    /// </summary>
    public RegisterUserCommandValidator()
    {
        // Kullanıcı adı kuralları
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Kullanıcı adı boş olamaz.")
            .MinimumLength(3).WithMessage("Kullanıcı adı en az 3 karakter olmalıdır.")
            .MaximumLength(50).WithMessage("Kullanıcı adı en fazla 50 karakter olabilir.")
            .Matches("^[a-zA-Z0-9_.-]+$").WithMessage("Kullanıcı adı sadece harf, rakam, '_', '.' ve '-' karakterlerini içerebilir.");

        // E-posta kuralları
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi boş olamaz.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
            .MaximumLength(255).WithMessage("E-posta adresi en fazla 255 karakter olabilir.");

        // Şifre kuralları
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre boş olamaz.")
            .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.")
            .MaximumLength(100).WithMessage("Şifre en fazla 100 karakter olabilir.")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$")
            .WithMessage("Şifre en az bir küçük harf, bir büyük harf, bir rakam ve bir özel karakter içermelidir.");

        // Şifre onayı kuralları
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Şifre onayı boş olamaz.")
            .Equal(x => x.Password).WithMessage("Şifre onayı, şifre ile aynı olmalıdır.");

        // Ad kuralları
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad boş olamaz.")
            .MinimumLength(2).WithMessage("Ad en az 2 karakter olmalıdır.")
            .MaximumLength(50).WithMessage("Ad en fazla 50 karakter olabilir.")
            .Matches("^[a-zA-ZçÇğĞıİöÖşŞüÜ\\s]+$").WithMessage("Ad sadece harf ve boşluk karakterlerini içerebilir.");

        // Soyad kuralları
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad boş olamaz.")
            .MinimumLength(2).WithMessage("Soyad en az 2 karakter olmalıdır.")
            .MaximumLength(50).WithMessage("Soyad en fazla 50 karakter olabilir.")
            .Matches("^[a-zA-ZçÇğĞıİöÖşŞüÜ\\s]+$").WithMessage("Soyad sadece harf ve boşluk karakterlerini içerebilir.");
    }
} 