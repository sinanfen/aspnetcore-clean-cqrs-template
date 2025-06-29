using FluentValidation;

namespace Template.Application.Features.Auth.Queries.LoginUser;

/// <summary>
/// Kullanıcı giriş sorgusu doğrulama kuralları
/// </summary>
public class LoginUserQueryValidator : AbstractValidator<LoginUserQuery>
{
    /// <summary>
    /// Constructor - Doğrulama kuralları tanımlanır
    /// </summary>
    public LoginUserQueryValidator()
    {
        // Kullanıcı adı veya e-posta kuralları
        RuleFor(x => x.UsernameOrEmail)
            .NotEmpty().WithMessage("Kullanıcı adı veya e-posta adresi boş olamaz.")
            .MinimumLength(3).WithMessage("Kullanıcı adı veya e-posta en az 3 karakter olmalıdır.")
            .MaximumLength(255).WithMessage("Kullanıcı adı veya e-posta en fazla 255 karakter olabilir.");

        // Şifre kuralları
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre boş olamaz.")
            .MinimumLength(1).WithMessage("Şifre en az 1 karakter olmalıdır.")
            .MaximumLength(100).WithMessage("Şifre en fazla 100 karakter olabilir.");
    }
} 