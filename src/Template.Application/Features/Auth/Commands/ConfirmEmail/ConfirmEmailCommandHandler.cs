using MediatR;
using Microsoft.AspNetCore.Identity;
using Template.Application.Common.Results;
using Template.Domain.Entities.Identity;

namespace Template.Application.Features.Auth.Commands.ConfirmEmail;

/// <summary>
/// Email onaylama komutu işleyicisi
/// </summary>
public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, IResult<ConfirmEmailResponse>>
{
    private readonly UserManager<AppUser> _userManager;

    /// <summary>
    /// Constructor - Gerekli servisler dependency injection ile alınır
    /// </summary>
    /// <param name="userManager">ASP.NET Identity kullanıcı yöneticisi</param>
    public ConfirmEmailCommandHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    /// <summary>
    /// Email onaylama işlemini gerçekleştirir
    /// </summary>
    /// <param name="request">Onaylama komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Onaylama sonucu</returns>
    public async Task<IResult<ConfirmEmailResponse>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Email adresine göre kullanıcıyı bul
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result.Failure<ConfirmEmailResponse>("Bu email adresi ile kayıtlı kullanıcı bulunamadı.");
            }

            // Email zaten onaylanmış mı kontrol et
            if (user.EmailConfirmed)
            {
                return Result.Failure<ConfirmEmailResponse>("Bu email adresi zaten onaylanmış.");
            }

            // Base64 encoded token'ı decode et
            string decodedToken;
            try
            {
                var tokenBytes = Convert.FromBase64String(request.Token);
                decodedToken = System.Text.Encoding.UTF8.GetString(tokenBytes);
            }
            catch (FormatException)
            {
                return Result.Failure<ConfirmEmailResponse>("Geçersiz token formatı.");
            }

            // Token'ı doğrula ve email'i onayla
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failure<ConfirmEmailResponse>($"Email onaylanamadı: {errors}");
            }

            // Başarılı yanıt oluştur
            var response = new ConfirmEmailResponse
            {
                IsConfirmed = true,
                UserId = user.Id,
                Email = user.Email!,
                UserName = user.UserName!
            };

            return Result.Success(response, "Email adresiniz başarıyla onaylandı. Artık giriş yapabilirsiniz.");
        }
        catch (Exception ex)
        {
            return Result.Failure<ConfirmEmailResponse>($"Beklenmeyen bir hata oluştu: {ex.Message}");
        }
    }
} 