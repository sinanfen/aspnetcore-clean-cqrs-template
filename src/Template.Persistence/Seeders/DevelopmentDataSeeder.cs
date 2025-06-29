using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Template.Domain.Entities.Identity;

namespace Template.Persistence.Seeders;

/// <summary>
/// Development ortamı için sample veri seeder'ı
/// </summary>
public class DevelopmentDataSeeder : DevelopmentSeeder
{
    public override int Priority => 10; // Son olarak çalışır

    public override async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
        var logger = serviceProvider.GetRequiredService<ILogger<DevelopmentDataSeeder>>();

        logger.LogInformation("🛠️ Development sample data seeding başlatılıyor...");

        // Sample user'lar oluştur
        var sampleUsers = new[]
        {
            new { Email = "john.doe@example.com", FirstName = "John", LastName = "Doe", Role = "User" },
            new { Email = "jane.smith@example.com", FirstName = "Jane", LastName = "Smith", Role = "Manager" },
            new { Email = "test.user@example.com", FirstName = "Test", LastName = "User", Role = "User" }
        };

        var createdUsers = 0;
        var password = "Test123!";

        foreach (var userInfo in sampleUsers)
        {
            var existingUser = await userManager.FindByEmailAsync(userInfo.Email);
            if (existingUser == null)
            {
                var user = new AppUser
                {
                    UserName = userInfo.Email,
                    Email = userInfo.Email,
                    EmailConfirmed = true,
                    FirstName = userInfo.FirstName,
                    LastName = userInfo.LastName,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    LockoutEnabled = true,
                    TwoFactorEnabled = false
                };

                var result = await userManager.CreateAsync(user, password);
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, userInfo.Role);
                    createdUsers++;
                    logger.LogInformation("✅ Sample user oluşturuldu: {Email} ({Role})", 
                        userInfo.Email, userInfo.Role);
                }
                else
                {
                    logger.LogWarning("⚠️ Sample user oluşturulamadı: {Email}. Hatalar: {Errors}", 
                        userInfo.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        logger.LogInformation("✅ Development sample data seeding tamamlandı. Oluşturulan kullanıcı: {Count}", 
            createdUsers);
        
        if (createdUsers > 0)
        {
            logger.LogInformation("🔑 Sample kullanıcı şifresi: {Password}", password);
        }
    }
} 