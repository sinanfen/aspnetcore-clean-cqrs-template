using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Template.Application.Services.Email;
using Template.Infrastructure.Configuration;

namespace Template.Infrastructure.Services.Email;

/// <summary>
/// SMTP üzerinden gerçek e-posta gönderen servis
/// Production-ready implementation with MailKit
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<SmtpEmailSender> _logger;
    private readonly IHostEnvironment _environment;

    public SmtpEmailSender(
        IOptions<EmailSettings> emailSettings, 
        ILogger<SmtpEmailSender> logger,
        IHostEnvironment environment)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Genel e-posta gönderme metodu (interface method)
    /// </summary>
    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        return await SendEmailAsync(to, subject, body, null);
    }

    /// <summary>
    /// Genel e-posta gönderme metodu (overload with plain text)
    /// </summary>
    public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody, string? plainTextBody = null)
    {
        if (_emailSettings.DisableInDevelopment && _environment.IsDevelopment())
        {
            _logger.LogInformation("📧 [DEVELOPMENT] Email gönderimi devre dışı - To: {Email}, Subject: {Subject}", 
                toEmail, subject);
            return true; // Development'ta başarılı olarak dön
        }

        try
        {
            using var message = CreateMessage(toEmail, subject, htmlBody, plainTextBody);
            return await SendMessageAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Email gönderilemedi - To: {Email}, Subject: {Subject}", toEmail, subject);
            return false;
        }
    }

    /// <summary>
    /// Email onay mesajı gönder (interface method)
    /// </summary>
    public async Task<bool> SendEmailConfirmationAsync(string email, string confirmationToken, string confirmationUrl)
    {
        return await SendEmailConfirmationAsync(email, confirmationUrl);
    }

    /// <summary>
    /// Email onay mesajı gönder (overload method)
    /// </summary>
    public async Task<bool> SendEmailConfirmationAsync(string toEmail, string confirmationUrl)
    {
        const string subject = "Email Adresinizi Onaylayın";
        
        var htmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #2563eb;'>Email Adresinizi Onaylayın</h2>
                <p>Merhaba,</p>
                <p>Hesabınızı aktifleştirmek için aşağıdaki linke tıklayın:</p>
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{confirmationUrl}' 
                       style='background-color: #2563eb; color: white; padding: 12px 24px; 
                              text-decoration: none; border-radius: 6px; display: inline-block;'>
                        Email Adresimi Onayla
                    </a>
                </div>
                <p>Bu link 24 saat geçerlidir.</p>
                <p>Bu email'i siz talep etmediyseniz, güvenle yok sayabilirsiniz.</p>
                <hr style='margin: 30px 0; border: none; border-top: 1px solid #e5e5e5;'>
                <p style='color: #666; font-size: 12px;'>
                    Bu otomatik bir mesajdır, lütfen yanıtlamayın.
                </p>
            </div>";

        var plainTextBody = $@"
            Email Adresinizi Onaylayın
            
            Merhaba,
            
            Hesabınızı aktifleştirmek için aşağıdaki linke tıklayın:
            {confirmationUrl}
            
            Bu link 24 saat geçerlidir.
            Bu email'i siz talep etmediyseniz, güvenle yok sayabilirsiniz.
            
            Bu otomatik bir mesajdır, lütfen yanıtlamayın.";

        _logger.LogInformation("📧 Email onay mesajı gönderiliyor - To: {Email}", toEmail);
        return await SendEmailAsync(toEmail, subject, htmlBody, plainTextBody);
    }

    /// <summary>
    /// Şifre sıfırlama mesajı gönder (interface method)
    /// </summary>
    public async Task<bool> SendPasswordResetAsync(string email, string resetToken, string resetUrl)
    {
        return await SendPasswordResetAsync(email, resetUrl);
    }

    /// <summary>
    /// Şifre sıfırlama mesajı gönder (overload method)
    /// </summary>
    public async Task<bool> SendPasswordResetAsync(string toEmail, string resetUrl)
    {
        const string subject = "Şifre Sıfırlama Talebi";
        
        var htmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #dc2626;'>Şifre Sıfırlama Talebi</h2>
                <p>Merhaba,</p>
                <p>Şifrenizi sıfırlamak için bir talep aldık. Aşağıdaki linke tıklayarak yeni şifrenizi belirleyebilirsiniz:</p>
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{resetUrl}' 
                       style='background-color: #dc2626; color: white; padding: 12px 24px; 
                              text-decoration: none; border-radius: 6px; display: inline-block;'>
                        Şifremi Sıfırla
                    </a>
                </div>
                <p><strong>Güvenlik uyarısı:</strong> Bu link 15 dakika geçerlidir.</p>
                <p>Bu talebi siz yapmadıysanız, hesabınızın güvenliği için derhal bizimle iletişime geçin.</p>
                <hr style='margin: 30px 0; border: none; border-top: 1px solid #e5e5e5;'>
                <p style='color: #666; font-size: 12px;'>
                    Bu otomatik bir mesajdır, lütfen yanıtlamayın.
                </p>
            </div>";

        var plainTextBody = $@"
            Şifre Sıfırlama Talebi
            
            Merhaba,
            
            Şifrenizi sıfırlamak için bir talep aldık. Aşağıdaki linke tıklayarak yeni şifrenizi belirleyebilirsiniz:
            {resetUrl}
            
            Güvenlik uyarısı: Bu link 15 dakika geçerlidir.
            Bu talebi siz yapmadıysanız, hesabınızın güvenliği için derhal bizimle iletişime geçin.
            
            Bu otomatik bir mesajdır, lütfen yanıtlamayın.";

        _logger.LogInformation("📧 Şifre sıfırlama mesajı gönderiliyor - To: {Email}", toEmail);
        return await SendEmailAsync(toEmail, subject, htmlBody, plainTextBody);
    }

    /// <summary>
    /// İki faktörlü kimlik doğrulama kodu gönder
    /// </summary>
    public async Task<bool> SendTwoFactorCodeAsync(string toEmail, string code)
    {
        const string subject = "İki Faktörlü Kimlik Doğrulama Kodu";
        
        var htmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #059669;'>İki Faktörlü Kimlik Doğrulama</h2>
                <p>Merhaba,</p>
                <p>Giriş yapabilmek için aşağıdaki güvenlik kodunu kullanın:</p>
                <div style='text-align: center; margin: 30px 0;'>
                    <div style='background-color: #f3f4f6; padding: 20px; border-radius: 8px; 
                                display: inline-block; letter-spacing: 4px; font-size: 24px; 
                                font-weight: bold; color: #059669;'>
                        {code}
                    </div>
                </div>
                <p><strong>Bu kod 5 dakika geçerlidir.</strong></p>
                <p>Bu kodu kimseyle paylaşmayın.</p>
                <hr style='margin: 30px 0; border: none; border-top: 1px solid #e5e5e5;'>
                <p style='color: #666; font-size: 12px;'>
                    Bu otomatik bir mesajdır, lütfen yanıtlamayın.
                </p>
            </div>";

        var plainTextBody = $@"
            İki Faktörlü Kimlik Doğrulama
            
            Merhaba,
            
            Giriş yapabilmek için aşağıdaki güvenlik kodunu kullanın:
            
            Kod: {code}
            
            Bu kod 5 dakika geçerlidir.
            Bu kodu kimseyle paylaşmayın.
            
            Bu otomatik bir mesajdır, lütfen yanıtlamayın.";

        _logger.LogInformation("📧 2FA kodu gönderiliyor - To: {Email}", toEmail);
        return await SendEmailAsync(toEmail, subject, htmlBody, plainTextBody);
    }

    /// <summary>
    /// Toplu e-posta gönderme (interface method)
    /// </summary>
    public async Task<bool> SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string body)
    {
        return await SendBulkEmailAsync(recipients, subject, body, null);
    }

    /// <summary>
    /// Toplu e-posta gönderme (overload with plain text)
    /// </summary>
    public async Task<bool> SendBulkEmailAsync(IEnumerable<string> toEmails, string subject, string htmlBody, string? plainTextBody = null)
    {
        var emailList = toEmails.ToList();
        _logger.LogInformation("📧 Toplu email gönderimi başlatılıyor - Alıcı sayısı: {Count}", emailList.Count);

        var successCount = 0;
        var failureCount = 0;

        foreach (var email in emailList)
        {
            var success = await SendEmailAsync(email, subject, htmlBody, plainTextBody);
            if (success)
                successCount++;
            else
                failureCount++;

            // Rate limiting - spam prevention
            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        _logger.LogInformation("📧 Toplu email gönderimi tamamlandı - Başarılı: {Success}, Başarısız: {Failed}", 
            successCount, failureCount);

        return failureCount == 0; // Tümü başarılı olursa true
    }

    /// <summary>
    /// Şablon tabanlı e-posta gönderme
    /// </summary>
    public async Task<bool> SendTemplatedEmailAsync(string to, string templateName, object templateData)
    {
        try
        {
            // Template dosya yolu oluştur
            var templatePath = Path.Combine(_emailSettings.TemplateFolder, $"{templateName}.html");
            
            // Template dosyasını kontrol et
            if (!File.Exists(templatePath))
            {
                _logger.LogWarning("📧 Email template bulunamadı - Template: {TemplateName}, Path: {Path}", 
                    templateName, templatePath);
                
                // Fallback - simple template
                var fallbackSubject = $"Bildirim - {templateName}";
                var fallbackBody = $"<p>Merhaba,</p><p>Bu bir sistem bildirimidir.</p><p>Template: {templateName}</p>";
                return await SendEmailAsync(to, fallbackSubject, fallbackBody);
            }

            // Template dosyasını oku
            var templateContent = await File.ReadAllTextAsync(templatePath);
            
            // Template data ile placeholder'ları değiştir
            var processedContent = ProcessTemplate(templateContent, templateData);
            
            // Subject'i template'den çıkar (ilk satır)
            var lines = processedContent.Split('\n');
            var emailSubject = lines.Length > 0 && lines[0].StartsWith("Subject:") 
                ? lines[0].Substring(8).Trim() 
                : templateName;
            
            var emailBody = lines.Length > 1 
                ? string.Join('\n', lines.Skip(1)) 
                : processedContent;

            _logger.LogInformation("📧 Template-based email gönderiliyor - To: {Email}, Template: {Template}", 
                to, templateName);
            
            return await SendEmailAsync(to, emailSubject, emailBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Template-based email gönderilemedi - To: {Email}, Template: {Template}", 
                to, templateName);
            return false;
        }
    }

    /// <summary>
    /// Template içindeki placeholder'ları object property'leri ile değiştirir
    /// </summary>
    private string ProcessTemplate(string template, object data)
    {
        if (data == null) return template;

        var result = template;
        var properties = data.GetType().GetProperties();

        foreach (var property in properties)
        {
            var value = property.GetValue(data)?.ToString() ?? string.Empty;
            var placeholder = $"{{{{{property.Name}}}}}";
            result = result.Replace(placeholder, value);
        }

        return result;
    }

    /// <summary>
    /// MimeMessage oluştur
    /// </summary>
    private MimeMessage CreateMessage(string toEmail, string subject, string htmlBody, string? plainTextBody = null)
    {
        var message = new MimeMessage();
        
        // Sender
        message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
        
        // Recipient
        message.To.Add(new MailboxAddress("", toEmail));
        
        // Subject
        message.Subject = subject;

        // Body
        var bodyBuilder = new BodyBuilder();
        
        if (!string.IsNullOrEmpty(htmlBody))
        {
            bodyBuilder.HtmlBody = htmlBody;
        }
        
        if (!string.IsNullOrEmpty(plainTextBody))
        {
            bodyBuilder.TextBody = plainTextBody;
        }
        else if (!string.IsNullOrEmpty(htmlBody))
        {
            // HTML'den basit text oluştur
            bodyBuilder.TextBody = System.Text.RegularExpressions.Regex.Replace(htmlBody, "<.*?>", "");
        }

        message.Body = bodyBuilder.ToMessageBody();
        
        // Headers
        message.Headers.Add("X-Mailer", "Template.API v1.0");
        message.Headers.Add("X-Priority", "3"); // Normal priority
        
        return message;
    }

    /// <summary>
    /// SMTP ile mesaj gönder
    /// </summary>
    private async Task<bool> SendMessageAsync(MimeMessage message)
    {
        using var client = new SmtpClient();
        
        try
        {
            // Timeout ayarı
            client.Timeout = _emailSettings.TimeoutSeconds * 1000;
            
            if (_emailSettings.EnableLogging)
            {
                _logger.LogDebug("📧 SMTP sunucusuna bağlanılıyor - Host: {Host}:{Port}", 
                    _emailSettings.Host, _emailSettings.Port);
            }

            // SMTP sunucusuna bağlan
            await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, 
                _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

            // Kimlik doğrulama
            if (!string.IsNullOrEmpty(_emailSettings.Username) && !string.IsNullOrEmpty(_emailSettings.Password))
            {
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                
                if (_emailSettings.EnableLogging)
                {
                    _logger.LogDebug("📧 SMTP kimlik doğrulaması başarılı - User: {Username}", _emailSettings.Username);
                }
            }

            // Email gönder
            await client.SendAsync(message);
            
            if (_emailSettings.EnableLogging)
            {
                _logger.LogInformation("✅ Email başarıyla gönderildi - To: {To}, Subject: {Subject}", 
                    string.Join(", ", message.To), message.Subject);
            }

            await client.DisconnectAsync(true);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ SMTP email gönderimi başarısız - To: {To}, Subject: {Subject}", 
                string.Join(", ", message.To), message.Subject);
            
            // Bağlantı durumunu kontrol et ve gerekirse kapat
            if (client.IsConnected)
            {
                try
                {
                    await client.DisconnectAsync(true);
                }
                catch (Exception disconnectEx)
                {
                    _logger.LogWarning(disconnectEx, "⚠️ SMTP bağlantısı kapatılırken hata oluştu");
                }
            }
            
            return false;
        }
    }
} 