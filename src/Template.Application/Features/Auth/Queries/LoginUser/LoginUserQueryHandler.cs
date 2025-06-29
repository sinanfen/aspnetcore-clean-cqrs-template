using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Template.Application.Common.Results;
using Template.Domain.Entities.Identity;
using Template.Application.Services.Token;
using Template.Application.Services.TwoFactor;
using Template.Persistence.Data;

namespace Template.Application.Features.Auth.Queries.LoginUser;

/// <summary>
/// Kullanıcı giriş sorgusu işleyicisi
/// </summary>
public class LoginUserQueryHandler : IRequestHandler<LoginUserQuery, IResult<LoginUserResponse>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ITokenService _tokenService;
    private readonly ITwoFactorService _twoFactorService;

    /// <summary>
    /// Constructor - Gerekli servisler dependency injection ile alınır
    /// </summary>
    /// <param name="userManager">ASP.NET Identity kullanıcı yöneticisi</param>
    /// <param name="signInManager">ASP.NET Identity giriş yöneticisi</param>
    /// <param name="context">Veritabanı bağlamı</param>
    /// <param name="mapper">AutoMapper instance</param>
    /// <param name="tokenService">JWT token servisi</param>
    /// <param name="twoFactorService">İki faktörlü kimlik doğrulama servisi</param>
    public LoginUserQueryHandler(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        ApplicationDbContext context,
        IMapper mapper,
        ITokenService tokenService,
        ITwoFactorService twoFactorService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _mapper = mapper;
        _tokenService = tokenService;
        _twoFactorService = twoFactorService;
    }

    /// <summary>
    /// Kullanıcı giriş işlemini gerçekleştirir
    /// </summary>
    /// <param name="request">Giriş sorgusu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Giriş sonucu</returns>
    public async Task<IResult<LoginUserResponse>> Handle(LoginUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Kullanıcıyı username veya email ile bul
            var user = await FindUserAsync(request.UsernameOrEmail);
            if (user == null)
            {
                return Result.Failure<LoginUserResponse>("Kullanıcı adı/e-posta veya şifre hatalı.");
            }

            // E-posta doğrulandı mı kontrol et
            if (!user.EmailConfirmed)
            {
                return Result.Failure<LoginUserResponse>("E-posta adresinizi doğrulamanız gerekmektedir.");
            }

            // Hesap kilitli mi kontrol et
            if (await _userManager.IsLockedOutAsync(user))
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                return Result.Failure<LoginUserResponse>($"Hesabınız {lockoutEnd:dd.MM.yyyy HH:mm} tarihine kadar kilitlenmiştir.");
            }

            // Şifre kontrolü
            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);
            
            if (signInResult.IsLockedOut)
            {
                return Result.Failure<LoginUserResponse>("Çok fazla başarısız giriş denemesi. Hesabınız geçici olarak kilitlenmiştir.");
            }

            if (!signInResult.Succeeded)
            {
                return Result.Failure<LoginUserResponse>("Kullanıcı adı/e-posta veya şifre hatalı.");
            }

            // 2FA kontrolü - eğer aktifse 2FA gereksinimi döndür
            if (user.Is2FAEnabled)
            {
                return Result.Success(new LoginUserResponse
                {
                    UserId = user.Id,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    FullName = user.FullName,
                    Requires2FA = true,
                    EmailConfirmed = user.EmailConfirmed
                }, "İki faktörlü kimlik doğrulama gerekiyor. Lütfen authenticator uygulamanızdan kodu girin.");
            }

            // Normal login - JWT token oluştur
            return await CreateSuccessfulLoginResponse(user, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<LoginUserResponse>($"Beklenmeyen bir hata oluştu: {ex.Message}");
        }
    }

    /// <summary>
    /// Başarılı login response'u oluşturur
    /// </summary>
    /// <param name="user">Kullanıcı entity'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Login response</returns>
    private async Task<IResult<LoginUserResponse>> CreateSuccessfulLoginResponse(AppUser user, CancellationToken cancellationToken)
    {
        // JWT token oluştur
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = await _tokenService.GenerateAccessTokenAsync(user, roles);
        var tokenExpires = _tokenService.GetAccessTokenExpirationTime();

        // Refresh token oluştur ve kaydet
        var refreshTokenEntity = await _tokenService.GenerateRefreshTokenAsync(user);
        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync(cancellationToken);

        // Başarılı yanıt oluştur
        var response = new LoginUserResponse
        {
            UserId = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            FullName = user.FullName,
            AccessToken = accessToken,
            RefreshToken = refreshTokenEntity.Token,
            TokenExpires = tokenExpires,
            Requires2FA = false,
            EmailConfirmed = user.EmailConfirmed
        };

        return Result.Success(response, "Giriş başarılı.");
    }

    /// <summary>
    /// Kullanıcıyı username veya email ile bulur
    /// </summary>
    /// <param name="usernameOrEmail">Kullanıcı adı veya e-posta</param>
    /// <returns>Kullanıcı entity'si</returns>
    private async Task<AppUser?> FindUserAsync(string usernameOrEmail)
    {
        if (usernameOrEmail.Contains('@'))
        {
            return await _userManager.FindByEmailAsync(usernameOrEmail);
        }
        return await _userManager.FindByNameAsync(usernameOrEmail);
    }
}

/// <summary>
/// 2FA doğrulama tamamlama sorgusu işleyicisi
/// </summary>
public class Complete2FALoginQueryHandler : IRequestHandler<Complete2FALoginQuery, IResult<LoginUserResponse>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly ITwoFactorService _twoFactorService;

    /// <summary>
    /// Constructor - Gerekli servisler dependency injection ile alınır
    /// </summary>
    /// <param name="userManager">ASP.NET Identity kullanıcı yöneticisi</param>
    /// <param name="context">Veritabanı bağlamı</param>
    /// <param name="tokenService">JWT token servisi</param>
    /// <param name="twoFactorService">İki faktörlü kimlik doğrulama servisi</param>
    public Complete2FALoginQueryHandler(
        UserManager<AppUser> userManager,
        ApplicationDbContext context,
        ITokenService tokenService,
        ITwoFactorService twoFactorService)
    {
        _userManager = userManager;
        _context = context;
        _tokenService = tokenService;
        _twoFactorService = twoFactorService;
    }

    /// <summary>
    /// 2FA doğrulama tamamlama işlemini gerçekleştirir
    /// </summary>
    /// <param name="request">2FA doğrulama sorgusu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Login sonucu</returns>
    public async Task<IResult<LoginUserResponse>> Handle(Complete2FALoginQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Kullanıcıyı bul
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                return Result.Failure<LoginUserResponse>("Kullanıcı bulunamadı.");
            }

            // 2FA aktif mi kontrol et
            if (!user.Is2FAEnabled)
            {
                return Result.Failure<LoginUserResponse>("İki faktörlü kimlik doğrulama aktif değil.");
            }

            // 2FA kodunu doğrula
            bool isValidCode = false;
            
            // TOTP kodu mu yoksa backup kod mu kontrol et
            if (request.TwoFactorCode.Length == 6 && request.TwoFactorCode.All(char.IsDigit))
            {
                // TOTP kodu - kaydedilmiş secret key'i kullan
                if (string.IsNullOrEmpty(user.TwoFactorSecretKey))
                {
                    return Result.Failure<LoginUserResponse>("2FA secret key bulunamadı. Lütfen 2FA'yı yeniden aktifleştirin.");
                }
                
                isValidCode = await _twoFactorService.ValidateCodeAsync(user.TwoFactorSecretKey, request.TwoFactorCode);
            }
            else
            {
                // Backup kod
                isValidCode = await _twoFactorService.ValidateBackupCodeAsync(user, request.TwoFactorCode);
            }

            if (!isValidCode)
            {
                return Result.Failure<LoginUserResponse>("Geçersiz 2FA kodu.");
            }

            // 2FA başarılı - JWT token oluştur
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = await _tokenService.GenerateAccessTokenAsync(user, roles);
            var tokenExpires = _tokenService.GetAccessTokenExpirationTime();

            // Refresh token oluştur ve kaydet
            var refreshTokenEntity = await _tokenService.GenerateRefreshTokenAsync(user);
            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync(cancellationToken);

            // Başarılı yanıt oluştur
            var response = new LoginUserResponse
            {
                UserId = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                FullName = user.FullName,
                AccessToken = accessToken,
                RefreshToken = refreshTokenEntity.Token,
                TokenExpires = tokenExpires,
                Requires2FA = false,
                EmailConfirmed = user.EmailConfirmed
            };

            return Result.Success(response, "2FA doğrulama başarılı. Giriş yapıldı.");
        }
        catch (Exception ex)
        {
            return Result.Failure<LoginUserResponse>($"2FA doğrulama sırasında hata oluştu: {ex.Message}");
        }
    }
} 