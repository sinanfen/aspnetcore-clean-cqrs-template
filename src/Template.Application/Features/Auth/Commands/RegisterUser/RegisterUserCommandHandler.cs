using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Template.Application.Common.Results;
using Template.Domain.Entities.Identity;
using Template.Application.Services.Email;

namespace Template.Application.Features.Auth.Commands.RegisterUser;

/// <summary>
/// Kullanıcı kayıt komutu işleyicisi
/// </summary>
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, IResult<RegisterUserResponse>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IMapper _mapper;
    private readonly IEmailSender _emailSender;

    /// <summary>
    /// Constructor - Gerekli servisler dependency injection ile alınır
    /// </summary>
    /// <param name="userManager">ASP.NET Identity kullanıcı yöneticisi</param>
    /// <param name="mapper">AutoMapper instance</param>
    /// <param name="emailSender">E-posta gönderim servisi</param>
    public RegisterUserCommandHandler(UserManager<AppUser> userManager, IMapper mapper, IEmailSender emailSender)
    {
        _userManager = userManager;
        _mapper = mapper;
        _emailSender = emailSender;
    }

    /// <summary>
    /// Kullanıcı kayıt işlemini gerçekleştirir
    /// </summary>
    /// <param name="request">Kayıt komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kayıt sonucu</returns>
    public async Task<IResult<RegisterUserResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // E-posta adresi zaten kullanılıyor mu kontrol et
            var existingUserByEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existingUserByEmail != null)
            {
                return Result.Failure<RegisterUserResponse>("Bu e-posta adresi zaten kullanılıyor.");
            }

            // UserName otomatik generate et (email'in @ öncesi kısmı)
            var generatedUserName = string.IsNullOrEmpty(request.UserName) 
                ? request.Email.Split('@')[0] 
                : request.UserName;

            // Kullanıcı adı zaten kullanılıyor mu kontrol et
            var existingUserByUsername = await _userManager.FindByNameAsync(generatedUserName);
            if (existingUserByUsername != null)
            {
                // Eğer username çakışıyorsa, email'e suffix ekle
                generatedUserName = $"{request.Email.Split('@')[0]}_{Guid.NewGuid().ToString("N")[..8]}";
            }

            // Yeni kullanıcı oluştur
            var user = new AppUser
            {
                UserName = generatedUserName,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmailConfirmed = false // E-posta doğrulaması gerekiyor
            };

            // Kullanıcıyı veritabanına kaydet
            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failure<RegisterUserResponse>($"Kullanıcı oluşturulamadı: {errors}");
            }

            // E-posta doğrulama token'ı oluştur
            var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            
            // Token'ı base64 ile encode et (URL'de güvenli kullanım için)
            var encodedToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(emailConfirmationToken));
            
            // E-posta doğrulama URL'i oluştur (gerçek uygulamada frontend URL'i kullanılacak)
            var confirmationUrl = $"https://localhost:7176/api/auth/confirm-email?token={encodedToken}&email={Uri.EscapeDataString(user.Email!)}";
            
            // E-posta doğrulama bağlantısı gönder
            var emailSent = await _emailSender.SendEmailConfirmationAsync(user.Email!, emailConfirmationToken, confirmationUrl);

            // Başarılı yanıt oluştur
            var response = new RegisterUserResponse
            {
                UserId = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                FullName = user.FullName,
                EmailConfirmationSent = emailSent
            };

            return Result.Success(response, "Kullanıcı başarıyla oluşturuldu. E-posta doğrulama bağlantısı gönderildi.");
        }
        catch (Exception ex)
        {
            return Result.Failure<RegisterUserResponse>($"Beklenmeyen bir hata oluştu: {ex.Message}");
        }
    }
} 