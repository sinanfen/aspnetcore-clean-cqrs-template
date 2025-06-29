using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;

namespace Template.Persistence.Seeders;

/// <summary>
/// Veritabanı seed işlemlerini koordine eden merkezi sınıf
/// </summary>
public static class DbSeeder
{
    /// <summary>
    /// Tüm seed işlemlerini sırayla çalıştırır
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    /// <param name="environment">Host environment</param>
    /// <returns>Async task</returns>
    public static async Task SeedAsync(IServiceProvider serviceProvider, IHostEnvironment environment)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<ISeeder>>();
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("🌱 Veritabanı seeding başlatılıyor - Ortam: {Environment}", environment.EnvironmentName);

        try
        {
            // Tüm seeder sınıflarını bulur ve önceliğe göre sıralar
            var seeders = GetSeeders(environment.EnvironmentName);
            
            if (!seeders.Any())
            {
                logger.LogWarning("⚠️ Hiç seeder bulunamadı");
                return;
            }

            logger.LogInformation("📦 {Count} seeder bulundu", seeders.Count);

            var successCount = 0;
            var failureCount = 0;

            // Seeder'ları sırayla çalıştır
            foreach (var seeder in seeders)
            {
                try
                {
                    var seederStopwatch = Stopwatch.StartNew();
                    
                    logger.LogInformation("🔄 Seeder çalıştırılıyor: {SeederName} (Öncelik: {Priority})", 
                        seeder.GetType().Name, seeder.Priority);

                    await seeder.SeedAsync(serviceProvider);
                    
                    seederStopwatch.Stop();
                    successCount++;
                    
                    logger.LogInformation("✅ Seeder tamamlandı: {SeederName} ({ElapsedMs}ms)", 
                        seeder.GetType().Name, seederStopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    failureCount++;
                    logger.LogError(ex, "❌ Seeder başarısız: {SeederName}", seeder.GetType().Name);
                    
                    // Kritik seeder'lar için işlemi durdur
                    if (IsCriticalSeeder(seeder))
                    {
                        logger.LogError("💥 Kritik seeder başarısız oldu, seeding durduruldu");
                        throw;
                    }
                }
            }

            stopwatch.Stop();
            
            logger.LogInformation("🎉 Veritabanı seeding tamamlandı - Başarılı: {Success}, Başarısız: {Failed}, Süre: {ElapsedMs}ms",
                successCount, failureCount, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "💥 Veritabanı seeding başarısız oldu - Süre: {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Assembly'den tüm seeder sınıflarını bulur ve sıralar
    /// </summary>
    /// <param name="environmentName">Ortam adı</param>
    /// <returns>Sıralı seeder listesi</returns>
    private static List<ISeeder> GetSeeders(string environmentName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        return assembly.GetTypes()
            .Where(type => typeof(ISeeder).IsAssignableFrom(type) && 
                          !type.IsInterface && 
                          !type.IsAbstract)
            .Select(type => Activator.CreateInstance(type) as ISeeder)
            .Where(seeder => seeder != null && seeder.ShouldRunInEnvironment(environmentName))
            .OrderBy(seeder => seeder!.Priority)
            .ToList()!;
    }

    /// <summary>
    /// Seeder'ın kritik olup olmadığını kontrol eder
    /// </summary>
    /// <param name="seeder">Kontrol edilecek seeder</param>
    /// <returns>Kritik ise true</returns>
    private static bool IsCriticalSeeder(ISeeder seeder)
    {
        // RoleSeeder kritiktir - roller olmadan kullanıcılar oluşturulamaz
        return seeder is RoleSeeder;
    }
}

/// <summary>
/// Geliştirme ortamına özel seeder base sınıfı
/// </summary>
public abstract class DevelopmentSeeder : ISeeder
{
    public abstract int Priority { get; }
    public abstract Task SeedAsync(IServiceProvider serviceProvider);

    public virtual bool ShouldRunInEnvironment(string environmentName)
    {
        return environmentName.Equals("Development", StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Üretim ortamına özel seeder base sınıfı
/// </summary>
public abstract class ProductionSeeder : ISeeder
{
    public abstract int Priority { get; }
    public abstract Task SeedAsync(IServiceProvider serviceProvider);

    public virtual bool ShouldRunInEnvironment(string environmentName)
    {
        return environmentName.Equals("Production", StringComparison.OrdinalIgnoreCase);
    }
} 