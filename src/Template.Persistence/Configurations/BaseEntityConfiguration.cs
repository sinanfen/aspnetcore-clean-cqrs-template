using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Template.Domain.Entities.Common;

namespace Template.Persistence.Configurations;

/// <summary>
/// BaseEntity için Entity Framework konfigürasyonu
/// Çoklu veritabanı desteği ile (PostgreSQL/SQL Server)
/// </summary>
public class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        // Primary Key
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id)
            .HasComment("Benzersiz kimlik değeri (Primary Key)");

        // CreatedDate - Database provider'a göre default value
        builder.Property(e => e.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql(GetUtcNowSql(builder))
            .HasComment("Kaydın oluşturulma tarihi (UTC)");

        // UpdatedDate
        builder.Property(e => e.UpdatedDate)
            .HasComment("Kaydın son güncellenme tarihi (UTC)");

        // DeletedDate (Soft delete)
        builder.Property(e => e.DeletedDate)
            .HasComment("Kaydın silinme tarihi - Soft delete (UTC)");

        // CreatedBy
        builder.Property(e => e.CreatedBy)
            .HasMaxLength(256)
            .HasComment("Kaydı oluşturan kullanıcı");

        // UpdatedBy
        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(256)
            .HasComment("Kaydı güncelleyen kullanıcı");

        // IsDeleted - Soft delete flag
        builder.Property(e => e.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Soft delete için kullanılan bayrak");

        // Index'ler - Performance optimization
        builder.HasIndex(e => e.CreatedDate)
            .HasDatabaseName($"IX_{typeof(T).Name}_CreatedDate");

        builder.HasIndex(e => e.IsDeleted)
            .HasDatabaseName($"IX_{typeof(T).Name}_IsDeleted");

        // Compound index for soft delete queries
        builder.HasIndex(e => new { e.IsDeleted, e.CreatedDate })
            .HasDatabaseName($"IX_{typeof(T).Name}_IsDeleted_CreatedDate");
    }

    /// <summary>
    /// Database provider'a göre UTC NOW fonksiyonu döndürür
    /// </summary>
    /// <param name="builder">Entity type builder</param>
    /// <returns>Database-specific UTC NOW SQL</returns>
    protected virtual string GetUtcNowSql(EntityTypeBuilder<T> builder)
    {
        // Database provider'ı context'ten al
        var database = builder.Metadata.Model.GetAnnotations()
            .FirstOrDefault(a => a.Name == "Relational:DatabaseProvider")?.Value?.ToString();

        return database switch
        {
            "Microsoft.EntityFrameworkCore.SqlServer" => "GETUTCDATE()",
            "Npgsql.EntityFrameworkCore.PostgreSQL" => "NOW() AT TIME ZONE 'UTC'",
            "Microsoft.EntityFrameworkCore.Sqlite" => "DATETIME('now')",
            "Pomelo.EntityFrameworkCore.MySql" => "UTC_TIMESTAMP()",
            _ => "NOW() AT TIME ZONE 'UTC'" // Default to PostgreSQL
        };
    }
} 