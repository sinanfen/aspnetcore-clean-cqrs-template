using FluentValidation;

namespace Template.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Token yenileme komutu doğrulama kuralları
/// </summary>
public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    /// <summary>
    /// Constructor - Doğrulama kuralları tanımlanır
    /// </summary>
    public RefreshTokenCommandValidator()
    {
        // Access Token kuralları
        RuleFor(x => x.AccessToken)
            .NotEmpty().WithMessage("Access token boş olamaz.")
            .MinimumLength(10).WithMessage("Access token en az 10 karakter olmalıdır.")
            .MaximumLength(2048).WithMessage("Access token en fazla 2048 karakter olabilir.");

        // Refresh Token kuralları
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token boş olamaz.")
            .MinimumLength(10).WithMessage("Refresh token en az 10 karakter olmalıdır.")
            .MaximumLength(512).WithMessage("Refresh token en fazla 512 karakter olabilir.");
    }
} 