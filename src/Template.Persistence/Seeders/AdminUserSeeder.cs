using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Template.Domain.Entities.Identity;

namespace Template.Persistence.Seeders;

/// <summary>
/// Varsayılan admin kullanıcısını seed eden sınıf
/// </summary>
public class AdminUserSeeder : ISeeder
{
    public int Priority => 2; // Roller oluşturulduktan sonra çalışır

    public bool ShouldRunInEnvironment(string environmentName) => true; // Tüm ortamlarda çalışır

    public async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var logger = serviceProvider.GetRequiredService<ILogger<AdminUserSeeder>>();

        logger.LogInformation("🔄 Admin kullanıcı seeding başlatılıyor...");

        // Configuration'dan admin bilgileri alın, yoksa varsayılan değerleri kullan
        var adminEmail = configuration["DefaultAdmin:Email"] ?? "admin@template.com";
        var adminPassword = configuration["DefaultAdmin:Password"] ?? "Admin123!";
        var adminFirstName = configuration["DefaultAdmin:FirstName"] ?? "System";
        var adminLastName = configuration["DefaultAdmin:LastName"] ?? "Administrator";

        // Admin kullanıcısı zaten var mı kontrol et
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin != null)
        {
            logger.LogInformation("ℹ️ Admin kullanıcı zaten mevcut: {Email}", adminEmail);
            
            // Rolü kontrol et ve gerekirse ekle
            if (!await userManager.IsInRoleAsync(existingAdmin, "Admin"))
            {
                await userManager.AddToRoleAsync(existingAdmin, "Admin");
                logger.LogInformation("✅ Mevcut kullanıcıya Admin rolü eklendi: {Email}", adminEmail);
            }
            
            return;
        }

        // Yeni admin kullanıcısı oluştur
        var adminUser = new AppUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = adminFirstName,
            LastName = adminLastName,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            LockoutEnabled = false, // Admin asla kilitlenemez
            TwoFactorEnabled = false // İlk başta 2FA kapalı
        };

        var createResult = await userManager.CreateAsync(adminUser, adminPassword);
        
        if (!createResult.Succeeded)
        {
            logger.LogError("❌ Admin kullanıcı oluşturulamadı: {Email}. Hatalar: {Errors}", 
                adminEmail, string.Join(", ", createResult.Errors.Select(e => e.Description)));
            return;
        }

        logger.LogInformation("✅ Admin kullanıcı oluşturuldu: {Email}", adminEmail);

        // Admin rolünü ata
        var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
        if (roleResult.Succeeded)
        {
            logger.LogInformation("✅ Admin rolü atandı: {Email}", adminEmail);
        }
        else
        {
            logger.LogError("❌ Admin rolü atanamadı: {Email}. Hatalar: {Errors}", 
                adminEmail, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }

        // User rolünü de ata (Admin aynı zamanda User'dır)
        await userManager.AddToRoleAsync(adminUser, "User");
        
        logger.LogInformation("✅ Admin kullanıcı seeding tamamlandı: {Email}", adminEmail);
        logger.LogWarning("⚠️ Güvenlik Uyarısı: Üretim ortamında varsayılan admin şifresini değiştirin!");
    }
} 