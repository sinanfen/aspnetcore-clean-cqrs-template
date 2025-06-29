namespace Template.Persistence.Seeders;

/// <summary>
/// Veritabanı seed işlemleri için temel arayüz
/// </summary>
public interface ISeeder
{
    /// <summary>
    /// Seed işlemini asenkron olarak gerçekleştirir
    /// </summary>
    /// <param name="serviceProvider">Dependency injection service provider</param>
    /// <returns>Async task</returns>
    Task SeedAsync(IServiceProvider serviceProvider);

    /// <summary>
    /// Seed işleminin çalıştırılma önceliği (düşük değerler önce çalışır)
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Bu seeder'ın hangi ortamlarda çalışacağını belirtir
    /// </summary>
    bool ShouldRunInEnvironment(string environmentName);
} 