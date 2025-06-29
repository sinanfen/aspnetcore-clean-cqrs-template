using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Template.Application.Features.Auth.Commands.ConfirmEmail;
using Template.Application.Features.Auth.Commands.Enable2FA;
using Template.Application.Features.Auth.Commands.ForgotPassword;
using Template.Application.Features.Auth.Commands.RefreshToken;
using Template.Application.Features.Auth.Commands.RegisterUser;
using Template.Application.Features.Auth.Commands.ResendConfirmationEmail;
using Template.Application.Features.Auth.Commands.ResetPassword;
using Template.Application.Features.Auth.Commands.Verify2FA;
using Template.Application.Features.Auth.Queries.LoginUser;
using Template.Application.Services.User;
using ApplicationResult = Template.Application.Common.Results.Result;

namespace Template.API.Controllers;

/// <summary>
/// Kimlik doğrulama işlemleri için API controller'ı
/// </summary>
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("AuthPolicy")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;
    private readonly IUserAccessor _userAccessor;

    /// <summary>
    /// Constructor - Gerekli servisleri dependency injection ile alır
    /// </summary>
    /// <param name="mediator">MediatR instance'ı</param>
    /// <param name="logger">Logger instance'ı</param>
    /// <param name="userAccessor">UserAccessor instance'ı</param>
    public AuthController(IMediator mediator, ILogger<AuthController> logger, IUserAccessor userAccessor)
    {
        _mediator = mediator;
        _logger = logger;
        _userAccessor = userAccessor;
    }

    /// <summary>
    /// Yeni kullanıcı kaydı oluşturur
    /// </summary>
    /// <param name="command">Kullanıcı kayıt bilgileri</param>
    /// <returns>Kayıt sonucu</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult<RegisterUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Template.Application.Common.Results.IResult<RegisterUserResponse>>> Register([FromBody] RegisterUserCommand command)
    {
        _logger.LogInformation("User registration request received for email: {Email}", command.Email);

        try
        {
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("User registration successful for email: {Email}", command.Email);
                return Ok(result);
            }

            _logger.LogWarning("User registration failed for email: {Email}. ErrorMessage: {ErrorMessage}", 
                command.Email, result.ErrorMessage);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during user registration for email: {Email}", command.Email);
            return StatusCode(500, ApplicationResult.Failure("Kayıt işlemi sırasında beklenmeyen bir hata oluştu."));
        }
    }

    /// <summary>
    /// Email adresini onaylar
    /// </summary>
    /// <param name="token">Email onaylama token'ı</param>
    /// <param name="email">Onaylanacak email adresi</param>
    /// <returns>Onaylama sonucu</returns>
    [HttpGet("confirm-email")]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult<ConfirmEmailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Template.Application.Common.Results.IResult<ConfirmEmailResponse>>> ConfirmEmail([FromQuery] string token, [FromQuery] string email)
    {
        _logger.LogInformation("Email confirmation request received for: {Email}", email);

        try
        {
            var command = new ConfirmEmailCommand
            {
                Token = token,
                Email = email
            };

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Email confirmation successful for: {Email}", email);
                return Ok(result);
            }

            _logger.LogWarning("Email confirmation failed for: {Email}. ErrorMessage: {ErrorMessage}", 
                email, result.ErrorMessage);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during email confirmation for: {Email}", email);
            return StatusCode(500, ApplicationResult.Failure("Email onaylama sırasında beklenmeyen bir hata oluştu."));
        }
    }

    /// <summary>
    /// Email onaylama mesajını yeniden gönderir
    /// </summary>
    /// <param name="command">Yeniden gönderme bilgileri</param>
    /// <returns>Yeniden gönderme sonucu</returns>
    [HttpPost("resend-confirmation")]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult<ResendConfirmationEmailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Template.Application.Common.Results.IResult<ResendConfirmationEmailResponse>>> ResendConfirmationEmail([FromBody] ResendConfirmationEmailCommand command)
    {
        _logger.LogInformation("Resend confirmation email request received for: {Email}", command.Email);

        try
        {
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Resend confirmation email successful for: {Email}", command.Email);
                return Ok(result);
            }

            _logger.LogWarning("Resend confirmation email failed for: {Email}. ErrorMessage: {ErrorMessage}", 
                command.Email, result.ErrorMessage);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during resend confirmation email for: {Email}", command.Email);
            return StatusCode(500, ApplicationResult.Failure("Email onaylama mesajı yeniden gönderme sırasında beklenmeyen bir hata oluştu."));
        }
    }

    /// <summary>
    /// Kullanıcı girişi yapar
    /// </summary>
    /// <param name="query">Giriş bilgileri</param>
    /// <returns>Giriş sonucu ve JWT token veya 2FA gereksinimı</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult<LoginUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Template.Application.Common.Results.IResult<LoginUserResponse>>> Login([FromBody] LoginUserQuery query)
    {
        _logger.LogInformation("User login request received for: {UsernameOrEmail}", query.UsernameOrEmail);

        try
        {
            var result = await _mediator.Send(query);

            if (result.IsSuccess)
            {
                if (result.Data!.Requires2FA)
                {
                    _logger.LogInformation("2FA required for user: {UserId}", result.Data.UserId);
                    return Ok(result);
                }

                _logger.LogInformation("User login successful for: {UserId}", result.Data.UserId);
                return Ok(result);
            }

            _logger.LogWarning("User login failed for: {UsernameOrEmail}. ErrorMessage: {ErrorMessage}", 
                query.UsernameOrEmail, result.ErrorMessage);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during login for: {UsernameOrEmail}", query.UsernameOrEmail);
            return StatusCode(500, ApplicationResult.Failure("Giriş sırasında beklenmeyen bir hata oluştu."));
        }
    }

    /// <summary>
    /// 2FA doğrulamasını tamamlar ve giriş yapar
    /// </summary>
    /// <param name="query">2FA doğrulama bilgileri</param>
    /// <returns>Giriş sonucu ve JWT token</returns>
    [HttpPost("complete-2fa-login")]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult<LoginUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Template.Application.Common.Results.IResult<LoginUserResponse>>> Complete2FALogin([FromBody] Complete2FALoginQuery query)
    {
        _logger.LogInformation("2FA completion request received for user: {UserId}", query.UserId);

        try
        {
            var result = await _mediator.Send(query);

            if (result.IsSuccess)
            {
                _logger.LogInformation("2FA login successful for user: {UserId}", query.UserId);
                return Ok(result);
            }

            _logger.LogWarning("2FA login failed for user: {UserId}. ErrorMessage: {ErrorMessage}", 
                query.UserId, result.ErrorMessage);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during 2FA completion for user: {UserId}", query.UserId);
            return StatusCode(500, ApplicationResult.Failure("2FA doğrulama sırasında beklenmeyen bir hata oluştu."));
        }
    }

    /// <summary>
    /// Access token'ı yeniler
    /// </summary>
    /// <param name="command">Refresh token bilgileri</param>
    /// <returns>Yeni token'lar</returns>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult<RefreshTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Template.Application.Common.Results.IResult<RefreshTokenResponse>>> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        _logger.LogInformation("Token refresh request received");

        try
        {
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Token refresh successful");
                return Ok(result);
            }

            _logger.LogWarning("Token refresh failed. ErrorMessage: {ErrorMessage}", result.ErrorMessage);
            return Unauthorized(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during token refresh");
            return StatusCode(500, ApplicationResult.Failure("Token yenileme sırasında beklenmeyen bir hata oluştu."));
        }
    }

    /// <summary>
    /// İki faktörlü kimlik doğrulamayı etkinleştirir
    /// </summary>
    /// <param name="command">2FA etkinleştirme bilgileri</param>
    /// <returns>QR kod URI'si ve backup kodları</returns>
    [HttpPost("enable-2fa")]
    [Authorize]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult<Enable2FAResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Template.Application.Common.Results.IResult<Enable2FAResponse>>> Enable2FA([FromBody] Enable2FACommand command)
    {
        var userId = _userAccessor.GetUserId();
        _logger.LogInformation("2FA enable request received for user: {UserId}", userId);

        try
        {
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("2FA enable successful for user: {UserId}", userId);
                return Ok(result);
            }

            _logger.LogWarning("2FA enable failed for user: {UserId}. ErrorMessage: {ErrorMessage}", 
                userId, result.ErrorMessage);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during 2FA enable for user: {UserId}", userId);
            return StatusCode(500, ApplicationResult.Failure("2FA etkinleştirme sırasında beklenmeyen bir hata oluştu."));
        }
    }

    /// <summary>
    /// İki faktörlü kimlik doğrulama token'ını doğrular
    /// </summary>
    /// <param name="command">2FA doğrulama bilgileri</param>
    /// <returns>Doğrulama sonucu</returns>
    [HttpPost("verify-2fa")]
    [Authorize]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult<Verify2FAResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Template.Application.Common.Results.IResult<Verify2FAResponse>>> Verify2FA([FromBody] Verify2FACommand command)
    {
        var userId = _userAccessor.GetUserId();
        _logger.LogInformation("2FA verify request received for user: {UserId}", userId);

        try
        {
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("2FA verify successful for user: {UserId}", userId);
                return Ok(result);
            }

            _logger.LogWarning("2FA verify failed for user: {UserId}. ErrorMessage: {ErrorMessage}", 
                userId, result.ErrorMessage);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during 2FA verify for user: {UserId}", userId);
            return StatusCode(500, ApplicationResult.Failure("2FA doğrulama sırasında beklenmeyen bir hata oluştu."));
        }
    }

    /// <summary>
    /// Şifre sıfırlama talebi oluşturur
    /// </summary>
    /// <param name="command">Şifre sıfırlama bilgileri</param>
    /// <returns>Şifre sıfırlama sonucu</returns>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult<ForgotPasswordResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Template.Application.Common.Results.IResult<ForgotPasswordResponse>>> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        _logger.LogInformation("Forgot password request received for email: {Email}", command.Email);

        try
        {
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Forgot password successful for email: {Email}", command.Email);
                return Ok(result);
            }

            _logger.LogWarning("Forgot password failed for email: {Email}. ErrorMessage: {ErrorMessage}", 
                command.Email, result.ErrorMessage);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during forgot password for email: {Email}", command.Email);
            return StatusCode(500, ApplicationResult.Failure("Şifre sıfırlama talebi sırasında beklenmeyen bir hata oluştu."));
        }
    }

    /// <summary>
    /// Şifre sıfırlama işlemini gerçekleştirir
    /// </summary>
    /// <param name="command">Şifre sıfırlama bilgileri</param>
    /// <returns>Şifre sıfırlama sonucu</returns>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult<ResetPasswordResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Template.Application.Common.Results.IResult<ResetPasswordResponse>>> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        _logger.LogInformation("Reset password request received for email: {Email}", command.Email);

        try
        {
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Reset password successful for email: {Email}", command.Email);
                return Ok(result);
            }

            _logger.LogWarning("Reset password failed for email: {Email}. ErrorMessage: {ErrorMessage}", 
                command.Email, result.ErrorMessage);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during reset password for email: {Email}", command.Email);
            return StatusCode(500, ApplicationResult.Failure("Şifre sıfırlama sırasında beklenmeyen bir hata oluştu."));
        }
    }

    /// <summary>
    /// JWT token'ının geçerliliğini kontrol eder
    /// </summary>
    /// <returns>Token geçerlilik durumu</returns>
    [HttpGet("validate-token")]
    [Authorize]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Template.Application.Common.Results.IResult), StatusCodes.Status401Unauthorized)]
    public ActionResult<Template.Application.Common.Results.IResult<object>> ValidateToken()
    {
        _logger.LogInformation("Token validation request received");

        try
        {
            var userClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            
            var response = new
            {
                IsValid = true,
                Claims = userClaims,
                Message = "Token geçerli"
            };

            _logger.LogInformation("Token validation successful");
            return Ok(ApplicationResult.Success(response, "Token geçerli"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during token validation");
            return StatusCode(500, ApplicationResult.Failure("Token doğrulama sırasında beklenmeyen bir hata oluştu."));
        }
    }
} 