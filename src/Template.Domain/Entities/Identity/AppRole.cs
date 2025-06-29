using Microsoft.AspNetCore.Identity;

namespace Template.Domain.Entities.Identity;

/// <summary>
/// Uygulama rolü entity'si.
/// Microsoft.AspNetCore.Identity.IdentityRole<Guid> sınıfından türetilmiştir.
/// </summary>
public class AppRole : IdentityRole<Guid>
{
    /// <summary>
    /// Constructor - Guid tipinde Id oluşturur
    /// </summary>
    public AppRole() : base()
    {
        Id = Guid.NewGuid();
    }

    /// <summary>
    /// Constructor - Rol adı ile birlikte Guid tipinde Id oluşturur
    /// </summary>
    /// <param name="roleName">Rol adı</param>
    public AppRole(string roleName) : base(roleName)
    {
        Id = Guid.NewGuid();
    }
} 