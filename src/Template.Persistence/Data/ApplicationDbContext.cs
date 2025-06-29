using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Template.Domain.Entities.Common;
using Template.Domain.Entities.Identity;


namespace Template.Persistence.Data;

/// <summary>
/// Uygulama veritabanı bağlamı (DbContext).
/// ASP.NET Core Identity desteği ile birlikte tüm entity'leri yönetir.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{

    /// <summary>
    /// Refresh Token'lar için DbSet
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    /// <summary>
    /// Constructor - DbContext seçeneklerini alır
    /// </summary>
    /// <param name="options">Entity Framework DbContext seçenekleri</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Model oluşturma işlemi - Entity konfigürasyonları burada uygulanır
    /// </summary>
    /// <param name="builder">Model builder</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Identity tablo adlarını özelleştir
        ConfigureIdentityTableNames(builder);

        // Tüm entity konfigürasyonlarını assembly'den otomatik yükle
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Soft delete desteği için global query filter'lar
        ApplyGlobalQueryFilters(builder);
    }

    /// <summary>
    /// Değişiklikleri asenkron olarak kaydet
    /// Audit alanlarını (CreatedDate, UpdatedDate, etc.) otomatik günceller
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Etkilenen kayıt sayısı</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Audit alanlarını güncelle
        UpdateAuditFields();

        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Identity tablolarının adlarını özelleştirir
    /// </summary>
    /// <param name="builder">Model builder</param>
    private static void ConfigureIdentityTableNames(ModelBuilder builder)
    {
        // Identity tablo adlarını Türkçe/özel adlarla değiştir
        builder.Entity<AppUser>().ToTable("Users");
        builder.Entity<AppRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
    }

    /// <summary>
    /// Soft delete desteği için global query filter'ları uygular
    /// </summary>
    /// <param name="builder">Model builder</param>
    private static void ApplyGlobalQueryFilters(ModelBuilder builder)
    {
        // BaseEntity'den türeyen tüm entity'ler için soft delete filter
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                // Modern EF Core syntax ile global query filter
                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(GetSoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);

                var filter = method.Invoke(null, Array.Empty<object>());
                builder.Entity(entityType.ClrType).HasQueryFilter((LambdaExpression)filter!);
            }
        }
    }

    /// <summary>
    /// Soft delete filter lambda expression'ını oluşturur
    /// </summary>
    /// <typeparam name="T">BaseEntity türevli entity tipi</typeparam>
    /// <returns>Lambda expression</returns>
    private static LambdaExpression GetSoftDeleteFilter<T>() where T : BaseEntity
    {
        Expression<Func<T, bool>> filter = e => !e.IsDeleted;
        return filter;
    }

    /// <summary>
    /// Audit alanlarını günceller (CreatedDate, UpdatedDate, vs.)
    /// </summary>
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
            .ToList();

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            var now = DateTime.UtcNow;

            switch (entry.State)
            {
                case EntityState.Added:
                    entity.CreatedDate = now;
                    entity.CreatedBy ??= "System"; // User info will be set by Application layer
                    break;

                case EntityState.Modified:
                    entity.UpdatedDate = now;
                    entity.UpdatedBy ??= "System"; // User info will be set by Application layer
                    break;

                case EntityState.Deleted:
                    // Soft delete - fiziksel silme yerine IsDeleted = true yap
                    entry.State = EntityState.Modified;
                    entity.IsDeleted = true;
                    entity.DeletedDate = now;
                    break;
            }
        }
    }


} 