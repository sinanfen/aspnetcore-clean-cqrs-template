using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Template.Domain.Entities.Identity;

namespace Template.Persistence.Seeders;

/// <summary>
/// Sistem rollerini seed eden sınıf
/// </summary>
public class RoleSeeder : ISeeder
{
    public int Priority => 1; // Roller önce oluşturulmalı

    public bool ShouldRunInEnvironment(string environmentName) => true; // Tüm ortamlarda çalışır

    public async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<AppRole>>();
        var logger = serviceProvider.GetRequiredService<ILogger<RoleSeeder>>();

        logger.LogInformation("🔄 Role seeding başlatılıyor...");

        var roles = new[]
        {
            new { Name = "Admin", Description = "Sistem yöneticisi - Tüm yetkilere sahip" },
            new { Name = "User", Description = "Normal kullanıcı - Temel yetkilere sahip" },
            new { Name = "Manager", Description = "Yönetici - Orta seviye yetkilere sahip" }
        };

        var createdRoles = 0;
        var existingRoles = 0;

        foreach (var roleInfo in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleInfo.Name))
            {
                var role = new AppRole
                {
                    Name = roleInfo.Name,
                    NormalizedName = roleInfo.Name.ToUpperInvariant()
                };

                var result = await roleManager.CreateAsync(role);
                
                if (result.Succeeded)
                {
                    createdRoles++;
                    logger.LogInformation("✅ Role oluşturuldu: {RoleName}", roleInfo.Name);
                }
                else
                {
                    logger.LogError("❌ Role oluşturulamadı: {RoleName}. Hatalar: {Errors}", 
                        roleInfo.Name, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                existingRoles++;
                logger.LogDebug("ℹ️ Role zaten mevcut: {RoleName}", roleInfo.Name);
            }
        }

        logger.LogInformation("✅ Role seeding tamamlandı. Oluşturulan: {Created}, Mevcut: {Existing}", 
            createdRoles, existingRoles);
    }
} 