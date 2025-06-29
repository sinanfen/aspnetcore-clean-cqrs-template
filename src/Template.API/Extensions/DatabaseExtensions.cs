using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Template.Persistence.Data;
using Template.Persistence.Seeders;

namespace Template.API.Extensions;

/// <summary>
/// Veritabanı migration ve seed işlemleri için extension methods
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Veritabanı migration ve seed işlemlerini gerçekleştirir
    /// </summary>
    /// <param name="app">WebApplication instance</param>
    /// <returns>Async task</returns>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<WebApplication>>();
        var environment = services.GetRequiredService<IHostEnvironment>();

        try
        {
            var stopwatch = Stopwatch.StartNew();
            logger.LogInformation("🚀 Veritabanı başlatma işlemi başlıyor...");

            // Migration işlemi
            await MigrateDatabaseAsync(services, environment, logger);

            // Seed işlemi
            await DbSeeder.SeedAsync(services, environment);

            stopwatch.Stop();
            logger.LogInformation("✅ Veritabanı başlatma tamamlandı - Toplam süre: {ElapsedMs}ms", 
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "💥 Veritabanı başlatma başarısız oldu");
            
            // Geliştirme ortamında exception fırlat, production'da sadece log
            if (environment.IsDevelopment())
            {
                throw;
            }
            
            logger.LogWarning("⚠️ Production ortamında veritabanı hatasını yoksayıp devam ediliyor");
        }
    }

    /// <summary>
    /// Veritabanı migration işlemini gerçekleştirir
    /// </summary>
    /// <param name="scopedServices">Scoped service provider (scope içinden gelir)</param>
    /// <param name="environment">Host environment</param>
    /// <param name="logger">Logger instance</param>
    /// <returns>Async task</returns>
    private static async Task MigrateDatabaseAsync(IServiceProvider scopedServices, IHostEnvironment environment, ILogger logger)
    {
        var dbContext = scopedServices.GetRequiredService<ApplicationDbContext>();
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("🔄 Veritabanı migration işlemi başlatılıyor...");

        try
        {
            if (environment.IsDevelopment())
            {
                logger.LogInformation("🛠️ Development ortamı: EnsureCreated kullanılıyor");
                
                // Development'ta EnsureCreated kullan (hızlı prototype için)
                var created = await dbContext.Database.EnsureCreatedAsync();
                
                if (created)
                {
                    logger.LogInformation("✅ Veritabanı oluşturuldu");
                }
                else
                {
                    logger.LogInformation("ℹ️ Veritabanı zaten mevcuttu");
                }
            }
            else
            {
                logger.LogInformation("🏭 Production ortamı: Migration kullanılıyor");
                
                // Production'da Migration kullan (güvenli şema güncellemesi)
                var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
                var pendingCount = pendingMigrations.Count();
                
                if (pendingCount > 0)
                {
                    logger.LogInformation("📊 {Count} bekleyen migration bulundu", pendingCount);
                    logger.LogDebug("Bekleyen migration'lar: {Migrations}", string.Join(", ", pendingMigrations));
                    
                    await dbContext.Database.MigrateAsync();
                    logger.LogInformation("✅ Migration'lar başarıyla uygulandı");
                }
                else
                {
                    logger.LogInformation("ℹ️ Hiç bekleyen migration yok");
                }
            }

            // Veritabanı bağlantısını test et
            var canConnect = await dbContext.Database.CanConnectAsync();
            if (!canConnect)
            {
                throw new InvalidOperationException("Veritabanına bağlanılamıyor!");
            }

            stopwatch.Stop();
            logger.LogInformation("✅ Veritabanı migration tamamlandı - Süre: {ElapsedMs}ms", 
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "💥 Veritabanı migration başarısız oldu - Süre: {ElapsedMs}ms", 
                stopwatch.ElapsedMilliseconds);
            throw; // Re-throw to be handled by caller
        }
    }

    /// <summary>
    /// Veritabanı durumunu kontrol eder ve raporlar
    /// </summary>
    /// <param name="services">Service provider</param>
    /// <param name="logger">Logger instance</param>
    /// <returns>Async task</returns>
    public static async Task CheckDatabaseHealthAsync(this IServiceProvider services, ILogger logger)
    {
        try
        {
            // Scoped service için scope oluştur
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            logger.LogInformation("🔍 Veritabanı sağlık kontrolü yapılıyor...");
            
            var canConnect = await dbContext.Database.CanConnectAsync();
            if (!canConnect)
            {
                logger.LogError("❌ Veritabanına bağlanılamıyor");
                return;
            }

            // Temel tablo kontrolü
            var userCount = await dbContext.Users.CountAsync();
            var roleCount = await dbContext.Roles.CountAsync();
            
            logger.LogInformation("✅ Veritabanı sağlık kontrolü başarılı - Kullanıcı: {UserCount}, Rol: {RoleCount}", 
                userCount, roleCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Veritabanı sağlık kontrolü başarısız");
        }
    }
} 