using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Template.Application.Common.Results;
using Template.Domain.Entities.Identity;
using Template.Application.Services.Token;
using Template.Persistence.Data;

namespace Template.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Token yenileme komutu işleyicisi
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, IResult<RefreshTokenResponse>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;

    /// <summary>
    /// Constructor - Gerekli servisler dependency injection ile alınır
    /// </summary>
    /// <param name="userManager">ASP.NET Identity kullanıcı yöneticisi</param>
    /// <param name="context">Veritabanı bağlamı</param>
    /// <param name="tokenService">JWT token servisi</param>
    public RefreshTokenCommandHandler(UserManager<AppUser> userManager, ApplicationDbContext context, ITokenService tokenService)
    {
        _userManager = userManager;
        _context = context;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Token yenileme işlemini gerçekleştirir
    /// </summary>
    /// <param name="request">Token yenileme komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Token yenileme sonucu</returns>
    public async Task<IResult<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Refresh token'ı veritabanından bul
            var refreshToken = await _context.RefreshTokens
                .Include(rt => rt.AppUser)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

            if (refreshToken == null)
            {
                return Result.Failure<RefreshTokenResponse>("Geçersiz refresh token.");
            }

            // Token iptal edildi mi kontrol et
            if (refreshToken.IsRevoked)
            {
                return Result.Failure<RefreshTokenResponse>("Refresh token iptal edilmiş.");
            }

            // Token süresi doldu mu kontrol et
            if (refreshToken.IsExpired)
            {
                return Result.Failure<RefreshTokenResponse>("Refresh token süresi dolmuş.");
            }

            // Kullanıcı mevcut mu kontrol et
            var user = refreshToken.AppUser;
            if (user == null)
            {
                return Result.Failure<RefreshTokenResponse>("Kullanıcı bulunamadı.");
            }

            // Kullanıcı aktif mi kontrol et
            if (!user.EmailConfirmed)
            {
                return Result.Failure<RefreshTokenResponse>("E-posta doğrulanmamış kullanıcı.");
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                return Result.Failure<RefreshTokenResponse>("Kullanıcı hesabı kilitlenmiş.");
            }

            // Eski refresh token'ı iptal et (Token Rotation)
            refreshToken.IsRevoked = true;
            
            // Yeni refresh token oluştur
            var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(user);
            _context.RefreshTokens.Add(newRefreshToken);

            // Yeni access token oluştur
            var roles = await _userManager.GetRolesAsync(user);
            var newAccessToken = await _tokenService.GenerateAccessTokenAsync(user, roles);
            var tokenExpires = _tokenService.GetAccessTokenExpirationTime();

            // Değişiklikleri kaydet
            await _context.SaveChangesAsync(cancellationToken);

            // Başarılı yanıt oluştur
            var response = new RefreshTokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                TokenExpires = tokenExpires,
                UserId = user.Id,
                UserName = user.UserName!
            };

            return Result.Success(response, "Token başarıyla yenilendi.");
        }
        catch (Exception ex)
        {
            return Result.Failure<RefreshTokenResponse>($"Token yenileme sırasında hata oluştu: {ex.Message}");
        }
    }
} 