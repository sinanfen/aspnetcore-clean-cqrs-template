namespace Template.Application.Services.Email;

/// <summary>
/// E-posta gönderim işlemleri için servis arayüzü
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// E-posta gönderir
    /// </summary>
    /// <param name="to">Alıcı e-posta adresi</param>
    /// <param name="subject">E-posta konusu</param>
    /// <param name="body">E-posta içeriği (HTML destekli)</param>
    /// <returns>Gönderim başarılı mı?</returns>
    Task<bool> SendEmailAsync(string to, string subject, string body);

    /// <summary>
    /// E-posta doğrulama bağlantısı gönderir
    /// </summary>
    /// <param name="email">Kullanıcı e-posta adresi</param>
    /// <param name="confirmationToken">Doğrulama token'ı</param>
    /// <param name="confirmationUrl">Doğrulama URL'i</param>
    /// <returns>Gönderim başarılı mı?</returns>
    Task<bool> SendEmailConfirmationAsync(string email, string confirmationToken, string confirmationUrl);

    /// <summary>
    /// Şifre sıfırlama bağlantısı gönderir
    /// </summary>
    /// <param name="email">Kullanıcı e-posta adresi</param>
    /// <param name="resetToken">Şifre sıfırlama token'ı</param>
    /// <param name="resetUrl">Şifre sıfırlama URL'i</param>
    /// <returns>Gönderim başarılı mı?</returns>
    Task<bool> SendPasswordResetAsync(string email, string resetToken, string resetUrl);

    /// <summary>
    /// Çoklu alıcıya e-posta gönderir
    /// </summary>
    /// <param name="recipients">Alıcı e-posta adresleri</param>
    /// <param name="subject">E-posta konusu</param>
    /// <param name="body">E-posta içeriği (HTML destekli)</param>
    /// <returns>Gönderim başarılı mı?</returns>
    Task<bool> SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string body);

    /// <summary>
    /// Şablon tabanlı e-posta gönderir
    /// </summary>
    /// <param name="to">Alıcı e-posta adresi</param>
    /// <param name="templateName">Şablon adı</param>
    /// <param name="templateData">Şablon verisi</param>
    /// <returns>Gönderim başarılı mı?</returns>
    Task<bool> SendTemplatedEmailAsync(string to, string templateName, object templateData);
} 