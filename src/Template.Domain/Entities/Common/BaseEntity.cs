using System.ComponentModel.DataAnnotations;

namespace Template.Domain.Entities.Common;

/// <summary>
/// Tüm entity sınıflarının kalıtım aldığı temel sınıf.
/// Audit alanları ve soft delete desteği sağlar.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Benzersiz kimlik değeri (Primary Key)
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Kaydın oluşturulma tarihi
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Kaydın son güncellenme tarihi
    /// </summary>
    public DateTime? UpdatedDate { get; set; }

    /// <summary>
    /// Kaydın silinme tarihi (soft delete)
    /// </summary>
    public DateTime? DeletedDate { get; set; }

    /// <summary>
    /// Kaydı oluşturan kullanıcı
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Kaydı güncelleyen kullanıcı
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Soft delete için kullanılan bayrak
    /// </summary>
    public bool IsDeleted { get; set; } = false;
} 