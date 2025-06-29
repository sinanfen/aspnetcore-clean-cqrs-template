using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Template.Domain.Entities.Identity;

namespace Template.Persistence.Configurations;

/// <summary>
/// RefreshToken entity'si için Entity Framework konfigürasyonu
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    /// <summary>
    /// RefreshToken entity'si için veritabanı konfigürasyonlarını uygular
    /// </summary>
    /// <param name="builder">Entity type builder</param>
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        // Tablo adı
        builder.ToTable("RefreshTokens");

        // Primary Key
        builder.HasKey(rt => rt.Id);

        // Token alanı konfigürasyonu
        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(512)
            .HasComment("Refresh token değeri");

        // Expires alanı konfigürasyonu
        builder.Property(rt => rt.Expires)
            .IsRequired()
            .HasComment("Token'ın son kullanma tarihi");

        // IsRevoked alanı konfigürasyonu
        builder.Property(rt => rt.IsRevoked)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Token iptal edildi mi?");

        // CreatedAt alanı konfigürasyonu
        builder.Property(rt => rt.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql(GetUtcNowSql(builder))
            .HasComment("Token'ın oluşturulma tarihi");

        // AppUserId foreign key konfigürasyonu
        builder.Property(rt => rt.AppUserId)
            .IsRequired()
            .HasComment("Token'ın ait olduğu kullanıcının Id'si");

        // AppUser ile ilişki (Foreign Key)
        builder.HasOne(rt => rt.AppUser)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.AppUserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_RefreshTokens_Users_AppUserId");

        // Index'ler - Performans optimizasyonu
        builder.HasIndex(rt => rt.Token)
            .IsUnique()
            .HasDatabaseName("IX_RefreshTokens_Token");

        builder.HasIndex(rt => rt.AppUserId)
            .HasDatabaseName("IX_RefreshTokens_AppUserId");

        builder.HasIndex(rt => rt.IsRevoked)
            .HasDatabaseName("IX_RefreshTokens_IsRevoked");

        builder.HasIndex(rt => rt.Expires)
            .HasDatabaseName("IX_RefreshTokens_Expires");

        // Compound index - Aktif token'ları hızlı bulmak için
        builder.HasIndex(rt => new { rt.AppUserId, rt.IsRevoked, rt.Expires })
            .HasDatabaseName("IX_RefreshTokens_AppUserId_IsRevoked_Expires");

        // Computed columns için ignore (EF Core tarafından hesaplanacak)
        builder.Ignore(rt => rt.IsActive);
        builder.Ignore(rt => rt.IsExpired);
    }

    /// <summary>
    /// Database provider'a göre UTC NOW fonksiyonu döndürür
    /// </summary>
    /// <param name="builder">Entity type builder</param>
    /// <returns>Database-specific UTC NOW SQL</returns>
    private string GetUtcNowSql(EntityTypeBuilder<RefreshToken> builder)
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