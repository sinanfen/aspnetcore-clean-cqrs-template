using MediatR;
using Microsoft.AspNetCore.Identity;
using Template.Application.Common.Results;
using Template.Application.Services.TwoFactor;
using Template.Application.Services.User;
using Template.Domain.Entities.Identity;

namespace Template.Application.Features.Auth.Commands.Verify2FA;

/// <summary>
/// İki faktörlü kimlik doğrulama token doğrulama komutu işleyicisi
/// </summary>
public class Verify2FACommandHandler : IRequestHandler<Verify2FACommand, IResult<Verify2FAResponse>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IUserAccessor _userAccessor;

    /// <summary>
    /// Constructor - Gerekli servisler dependency injection ile alınır
    /// </summary>
    /// <param name="userManager">ASP.NET Identity kullanıcı yöneticisi</param>
    /// <param name="twoFactorService">İki faktörlü kimlik doğrulama servisi</param>
    /// <param name="userAccessor">Kullanıcı erişim servisi</param>
    public Verify2FACommandHandler(
        UserManager<AppUser> userManager, 
        ITwoFactorService twoFactorService,
        IUserAccessor userAccessor)
    {
        _userManager = userManager;
        _twoFactorService = twoFactorService;
        _userAccessor = userAccessor;
    }

    /// <summary>
    /// İki faktörlü kimlik doğrulama token doğrulama işlemini gerçekleştirir
    /// </summary>
    /// <param name="request">Verify2FA komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Doğrulama sonucu ve 2FA durumu</returns>
    public async Task<IResult<Verify2FAResponse>> Handle(Verify2FACommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Kullanıcının kimlik doğrulaması yapılmış mı kontrol et
            if (!_userAccessor.IsAuthenticated())
            {
                return Result.Failure<Verify2FAResponse>("Kimlik doğrulaması yapılmamış.");
            }

            // JWT token'dan kullanıcı ID'sini al
            var userId = _userAccessor.GetUserId();
            if (!userId.HasValue)
            {
                return Result.Failure<Verify2FAResponse>("Kullanıcı bilgisi alınamadı.");
            }

            // Kullanıcıyı bul
            var user = await _userManager.FindByIdAsync(userId.Value.ToString());
            if (user == null)
            {
                return Result.Failure<Verify2FAResponse>("Kullanıcı bulunamadı.");
            }

            // TOTP kodunu doğrula
            var isValidCode = await _twoFactorService.ValidateTotpAsync(user, request.SecretKey, request.TotpCode);
            
            if (!isValidCode)
            {
                // Backup kod kontrolü (eğer TOTP başarısız ise)
                var backupCodeResult = await TryValidateBackupCodeAsync(user, request.TotpCode);
                if (!backupCodeResult.IsSuccess)
                {
                    return Result.Failure<Verify2FAResponse>("Girilen kod geçersiz.");
                }

                // Backup kod kullanıldı, response'u güncelle
                var backupResponse = new Verify2FAResponse
                {
                    IsVerified = true,
                    Is2FAEnabled = user.Is2FAEnabled,
                    UsedBackupCodes = new[] { request.TotpCode },
                    RemainingBackupCodes = backupCodeResult.Data - 1 // Kullanılan kod sayısını çıkar
                };

                return Result.Success(backupResponse, "Backup kod ile doğrulama başarılı.");
            }

            // TOTP başarılı - 2FA'yı etkinleştir
            if (!user.Is2FAEnabled)
            {
                user.Is2FAEnabled = true;
                var updateResult = await _userManager.UpdateAsync(user);
                
                if (!updateResult.Succeeded)
                {
                    return Result.Failure<Verify2FAResponse>("2FA etkinleştirilemedi: " + 
                        string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                }
            }

            // Machine token oluştur (RememberMachine true ise)
            string? machineToken = null;
            if (request.RememberMachine)
            {
                machineToken = await _twoFactorService.GenerateMachineTokenAsync(user);
            }

            // Başarılı yanıt
            var response = new Verify2FAResponse
            {
                IsVerified = true,
                Is2FAEnabled = true,
                UsedBackupCodes = Array.Empty<string>(),
                RemainingBackupCodes = await GetRemainingBackupCodesCountAsync(user),
                MachineToken = machineToken
            };

            var message = user.Is2FAEnabled 
                ? "İki faktörlü kimlik doğrulama başarıyla doğrulandı."
                : "İki faktörlü kimlik doğrulama başarıyla etkinleştirildi.";

            return Result.Success(response, message);
        }
        catch (Exception ex)
        {
            return Result.Failure<Verify2FAResponse>($"2FA doğrulama sırasında hata oluştu: {ex.Message}");
        }
    }

    /// <summary>
    /// Backup kod doğrulaması yapar
    /// </summary>
    /// <param name="user">Kullanıcı</param>
    /// <param name="backupCode">Backup kod</param>
    /// <returns>Doğrulama sonucu ve kalan kod sayısı</returns>
    private async Task<IResult<int>> TryValidateBackupCodeAsync(AppUser user, string backupCode)
    {
        try
        {
            // Bu mock implementasyon - gerçekte backup kodlar veritabanında saklanmalı
            var isValidBackupCode = await _twoFactorService.ValidateBackupCodeAsync(user, backupCode);
            
            if (isValidBackupCode)
            {
                // Backup kodu kullanıldı olarak işaretle ve kalan sayıyı döndür
                var remainingCount = await GetRemainingBackupCodesCountAsync(user);
                return Result.Success(remainingCount);
            }

            return Result.Failure<int>("Geçersiz backup kod.");
        }
        catch
        {
            return Result.Failure<int>("Backup kod doğrulama hatası.");
        }
    }

    /// <summary>
    /// Kullanıcının kalan backup kod sayısını getirir
    /// </summary>
    /// <param name="user">Kullanıcı</param>
    /// <returns>Kalan backup kod sayısı</returns>
    private async Task<int> GetRemainingBackupCodesCountAsync(AppUser user)
    {
        try
        {
            // Mock implementasyon - gerçekte veritabanından alınmalı
            return await Task.FromResult(8); // 10 kod oluşturuldu, 2 kullanıldı diyelim
        }
        catch
        {
            return 0;
        }
    }
} 