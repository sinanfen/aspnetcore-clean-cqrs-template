using Microsoft.AspNetCore.Identity;

namespace Template.Domain.Entities.Identity;

/// <summary>
/// Uygulama kullanıcısı entity'si.
/// Microsoft.AspNetCore.Identity.IdentityUser<Guid> sınıfından türetilmiştir.
/// </summary>
public class AppUser : IdentityUser<Guid>
{
    /// <summary>
    /// Kullanıcının adı
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Kullanıcının soyadı
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// İki faktörlü kimlik doğrulama aktif mi?
    /// </summary>
    public bool Is2FAEnabled { get; set; } = false;

    /// <summary>
    /// İki faktörlü kimlik doğrulama secret key'i (Base32 format)
    /// </summary>
    public string? TwoFactorSecretKey { get; set; }

    /// <summary>
    /// Kullanıcının tam adı (Computed Property)
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Kullanıcıya ait refresh token'lar (Navigation Property)
    /// </summary>
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    /// <summary>
    /// Constructor - Guid tipinde Id oluşturur
    /// </summary>
    public AppUser()
    {
        Id = Guid.NewGuid();
    }
} 