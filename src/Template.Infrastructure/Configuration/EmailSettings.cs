using System.ComponentModel.DataAnnotations;

namespace Template.Infrastructure.Configuration;

/// <summary>
/// Email/SMTP yapılandırma ayarları
/// </summary>
public class EmailSettings
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "Email:SMTP";

    /// <summary>
    /// SMTP sunucu host adresi
    /// </summary>
    [Required]
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// SMTP sunucu portu
    /// </summary>
    [Range(1, 65535)]
    public int Port { get; set; } = 587;

    /// <summary>
    /// SSL/TLS kullanımı
    /// </summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>
    /// SMTP kimlik doğrulama kullanıcı adı
    /// </summary>
    [Required]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// SMTP kimlik doğrulama şifresi
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gönderici e-posta adresi
    /// </summary>
    [Required]
    [EmailAddress]
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gönderici adı
    /// </summary>
    [Required]
    public string FromName { get; set; } = string.Empty;

    /// <summary>
    /// Bağlantı timeout süresi (saniye)
    /// </summary>
    [Range(1, 300)]
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Email template dosyalarının bulunduğu klasör
    /// </summary>
    public string TemplateFolder { get; set; } = "EmailTemplates";

    /// <summary>
    /// Development ortamında email gönderimini devre dışı bırak
    /// </summary>
    public bool DisableInDevelopment { get; set; } = false;

    /// <summary>
    /// Email loglaması aktif mi?
    /// </summary>
    public bool EnableLogging { get; set; } = true;
} 