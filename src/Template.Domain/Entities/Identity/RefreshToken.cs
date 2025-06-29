using System.ComponentModel.DataAnnotations;

namespace Template.Domain.Entities.Identity;

/// <summary>
/// Refresh Token entity'si.
/// JWT access token yenileme işlemleri için kullanılır.
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// Benzersiz kimlik değeri (Primary Key)
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Refresh token değeri
    /// </summary>
    [Required]
    [StringLength(512)]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Token'ın son kullanma tarihi
    /// </summary>
    public DateTime Expires { get; set; }

    /// <summary>
    /// Token iptal edildi mi?
    /// </summary>
    public bool IsRevoked { get; set; } = false;

    /// <summary>
    /// Token'ın oluşturulma tarihi
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Token'ın ait olduğu kullanıcının Id'si (Foreign Key)
    /// </summary>
    [Required]
    public Guid AppUserId { get; set; }

    /// <summary>
    /// Token'ın ait olduğu kullanıcı (Navigation Property)
    /// </summary>
    public virtual AppUser AppUser { get; set; } = null!;

    /// <summary>
    /// Token'ın aktif olup olmadığını kontrol eder
    /// </summary>
    public bool IsActive => !IsRevoked && DateTime.UtcNow <= Expires;

    /// <summary>
    /// Token'ın süresi doldu mu kontrol eder
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= Expires;
} 