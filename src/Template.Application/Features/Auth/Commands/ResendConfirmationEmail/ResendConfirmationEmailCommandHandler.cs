using MediatR;
using Microsoft.AspNetCore.Identity;
using Template.Application.Common.Results;
using Template.Application.Services.Email;
using Template.Domain.Entities.Identity;

namespace Template.Application.Features.Auth.Commands.ResendConfirmationEmail;

/// <summary>
/// Email onaylama mesajını yeniden gönderme komutu işleyicisi
/// </summary>
public class ResendConfirmationEmailCommandHandler : IRequestHandler<ResendConfirmationEmailCommand, IResult<ResendConfirmationEmailResponse>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailSender _emailSender;

    /// <summary>
    /// Constructor - Gerekli servisler dependency injection ile alınır
    /// </summary>
    /// <param name="userManager">ASP.NET Identity kullanıcı yöneticisi</param>
    /// <param name="emailSender">E-posta gönderim servisi</param>
    public ResendConfirmationEmailCommandHandler(UserManager<AppUser> userManager, IEmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }

    /// <summary>
    /// Email onaylama mesajını yeniden gönderme işlemini gerçekleştirir
    /// </summary>
    /// <param name="request">Yeniden gönderme komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Yeniden gönderme sonucu</returns>
    public async Task<IResult<ResendConfirmationEmailResponse>> Handle(ResendConfirmationEmailCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Email adresine göre kullanıcıyı bul
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Güvenlik için kullanıcı bulunamadığında da başarılı dön
                // Email enumeration saldırılarını önlemek için
                return Result.Success(new ResendConfirmationEmailResponse
                {
                    EmailSent = true,
                    Email = request.Email,
                    IsAlreadyConfirmed = false
                }, "Email onaylama mesajı gönderildi.");
            }

            // Email zaten onaylanmış mı kontrol et
            if (user.EmailConfirmed)
            {
                return Result.Success(new ResendConfirmationEmailResponse
                {
                    EmailSent = false,
                    UserId = user.Id,
                    Email = user.Email!,
                    IsAlreadyConfirmed = true
                }, "Bu email adresi zaten onaylanmış.");
            }

            // Yeni email onaylama token'ı oluştur
            var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            
            // Token'ı base64 ile encode et (URL'de güvenli kullanım için)
            var encodedToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(emailConfirmationToken));
            
            // Email onaylama URL'i oluştur
            var confirmationUrl = $"https://localhost:7176/api/auth/confirm-email?token={encodedToken}&email={Uri.EscapeDataString(user.Email!)}";
            
            // Email onaylama mesajını gönder
            var emailSent = await _emailSender.SendEmailConfirmationAsync(user.Email!, emailConfirmationToken, confirmationUrl);

            // Başarılı yanıt oluştur
            var response = new ResendConfirmationEmailResponse
            {
                EmailSent = emailSent,
                UserId = user.Id,
                Email = user.Email!,
                IsAlreadyConfirmed = false
            };

            return Result.Success(response, "Email onaylama mesajı yeniden gönderildi.");
        }
        catch (Exception ex)
        {
            return Result.Failure<ResendConfirmationEmailResponse>($"Beklenmeyen bir hata oluştu: {ex.Message}");
        }
    }
} 