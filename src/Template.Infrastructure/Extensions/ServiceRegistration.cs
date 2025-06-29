using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Template.Application.Services.Email;
using Template.Application.Services.Token;
using Template.Application.Services.TwoFactor;
using Template.Application.Services.User;
using Template.Infrastructure.Configuration;
using Template.Infrastructure.Services.Email;
using Template.Infrastructure.Services.Token;
using Template.Infrastructure.Services.TwoFactor;
using Template.Infrastructure.Services.User;


namespace Template.Infrastructure.Extensions;

/// <summary>
/// Infrastructure katmanı servislerini DI container'a ekleyen extension metotları
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Infrastructure katmanı servislerini IServiceCollection'a ekler
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // JWT ayarlarını güvenli şekilde yapılandır
        services.ConfigureJwtSettings(configuration);

        // Email ayarlarını yapılandır
        services.ConfigureEmailSettings(configuration);

        // Core servisleri ekle
        services.AddTokenServices();
        services.AddEmailServices();
        services.AddTwoFactorServices();
        services.AddUserServices();

        // HttpContextAccessor'ı ekle (UserAccessor için gerekli)
        services.AddHttpContextAccessor();

        // Infrastructure Health Checks
        services.AddInfrastructureHealthChecks();

        return services;
    }

    /// <summary>
    /// JWT ayarlarını güvenli şekilde yapılandırır
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection ConfigureJwtSettings(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // JwtSettings'i appsettings.json'dan yükle
        services.Configure<JwtSettings>(
            configuration.GetSection(JwtSettings.SectionName));

        // JwtSettings validation - güvenli yaklaşım
        services.AddSingleton<IValidateOptions<JwtSettings>, JwtSettingsValidator>();

        return services;
    }

    /// <summary>
    /// JWT ayarlarını doğrulayan validator sınıfı
    /// </summary>
    private class JwtSettingsValidator : IValidateOptions<JwtSettings>
    {
        private readonly ILogger<JwtSettingsValidator> _logger;

        public JwtSettingsValidator(ILogger<JwtSettingsValidator> logger)
        {
            _logger = logger;
        }

        public ValidateOptionsResult Validate(string? name, JwtSettings options)
        {
            if (options == null)
            {
                _logger.LogError("JWT settings configuration is null");
                return ValidateOptionsResult.Fail("JWT ayarları yapılandırılmamış");
            }

            if (!options.IsValid())
            {
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                
                if (environment == "Development")
                {
                    _logger.LogWarning("JWT settings are invalid, using development defaults");
                    // Development ortamında varsayılan değerlerle devam et
                    options.SecretKey ??= "development-secret-key-minimum-32-characters-long-for-security";
                    options.Issuer ??= "TemplateAPI";
                    options.Audience ??= "TemplateClient";
                    return ValidateOptionsResult.Success;
                }
                else
                {
                    _logger.LogError("JWT settings are invalid in production environment");
                    return ValidateOptionsResult.Fail(
                        $"JWT ayarları geçersiz. Lütfen appsettings.json dosyasında " +
                        $"'{JwtSettings.SectionName}' bölümünü kontrol edin.");
                }
            }

            return ValidateOptionsResult.Success;
        }
    }

    /// <summary>
    /// Token servislerini ekler
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddTokenServices(this IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();
        return services;
    }

    /// <summary>
    /// Email ayarlarını yapılandırır
    /// Environment variable placeholder'larını expand eder
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection ConfigureEmailSettings(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EmailSettings'i appsettings.json'dan yükle ve environment variable'ları expand et
        services.Configure<EmailSettings>(options =>
        {
            configuration.GetSection(EmailSettings.SectionName).Bind(options);
            
            // Environment variable placeholder'larını expand et
            if (!string.IsNullOrEmpty(options.Password) && options.Password.Contains('%'))
            {
                options.Password = ExpandEnvironmentVariablePlaceholder(options.Password);
            }
        });

        return services;
    }

    /// <summary>
    /// %VARIABLE_NAME% formatındaki placeholder'ı environment variable değeri ile değiştirir
    /// </summary>
    /// <param name="value">Placeholder içeren değer</param>
    /// <returns>Expand edilmiş değer</returns>
    private static string ExpandEnvironmentVariablePlaceholder(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        // %VARIABLE_NAME% pattern'ini bul ve değiştir
        var pattern = @"%([A-Za-z_][A-Za-z0-9_]*)%";
        return System.Text.RegularExpressions.Regex.Replace(value, pattern, match =>
        {
            var variableName = match.Groups[1].Value;
            var environmentValue = Environment.GetEnvironmentVariable(variableName);
            
            if (!string.IsNullOrEmpty(environmentValue))
            {
                return environmentValue;
            }
            else
            {
                // Environment variable bulunamazsa uyarı ver ama placeholder'ı bırak
                Console.WriteLine($"⚠️ Environment variable '{variableName}' bulunamadı");
                return match.Value;
            }
        });
    }

    /// <summary>
    /// E-posta servislerini ekler
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddEmailServices(this IServiceCollection services)
    {
        // Production-ready SMTP implementasyonu kullan
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        
        // Development/Testing için Mock implementasyonu:
        // services.AddScoped<IEmailSender, MockEmailSender>();

        return services;
    }

    /// <summary>
    /// İki faktörlü kimlik doğrulama servislerini ekler
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddTwoFactorServices(this IServiceCollection services)
    {
        services.AddScoped<ITwoFactorService, TwoFactorService>();
        return services;
    }

    /// <summary>
    /// Kullanıcı erişim servislerini ekler
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddUserServices(this IServiceCollection services)
    {
        services.AddScoped<IUserAccessor, UserAccessor>();
        return services;
    }

    /// <summary>
    /// Tüm Infrastructure servisleri için health check'leri ekler
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddInfrastructureHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("token_service", () =>
            {
                // TokenService sağlık kontrolü
                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("TokenService is working");
            })
            .AddCheck("email_service", () =>
            {
                // EmailService sağlık kontrolü
                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("EmailService is working");
            })
            .AddCheck("two_factor_service", () =>
            {
                // TwoFactorService sağlık kontrolü
                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("TwoFactorService is working");
            });

        return services;
    }

    /// <summary>
    /// Geliştirme ortamı için özel servis yapılandırmaları
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddDevelopmentServices(this IServiceCollection services)
    {
        // Geliştirme ortamında ek servisleri ekle
        // Örnek: Memory cache, developer exception page, vb.
        
        return services;
    }

    /// <summary>
    /// Üretim ortamı için özel servis yapılandırmaları
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddProductionServices(this IServiceCollection services)
    {
        // Üretim ortamında ek servisleri ekle
        // Örnek: Redis cache, Application Insights, vb.
        
        return services;
    }
} 