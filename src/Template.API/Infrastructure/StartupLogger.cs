using Serilog;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace Template.API.Infrastructure;

/// <summary>
/// Uygulama başlangıç bilgilerini loglayan statik yardımcı sınıf
/// </summary>
public static class StartupLogger
{
    /// <summary>
    /// Uygulama başlangıç detaylarını loglar
    /// </summary>
    /// <param name="app">WebApplication instance'ı</param>
    /// <returns>Async task</returns>
    public static async Task LogDetailsAsync(WebApplication app)
    {
        try
        {
            var startupInfo = await BuildStartupInfoAsync(app);
            
            // Structured logging için JSON formatında log
            Log.Information("Application Startup Details: {@StartupInfo}", startupInfo);
            
            // Konsol için okunabilir format
            Log.Information("🚀 {ProjectName} v{Version} started successfully!", 
                startupInfo.ProjectName, startupInfo.Version);
            Log.Information("📅 Build Date: {BuildDate}", startupInfo.BuildDate);
            Log.Information("🌍 Environment: {Environment}", startupInfo.Environment);
            Log.Information("🖥️  Operating System: {OS}", startupInfo.OS);
            Log.Information("⚡ Runtime: {Runtime}", startupInfo.Runtime);
            Log.Information("🔧 Process ID: {ProcessId}", startupInfo.ProcessId);
            Log.Information("👤 User: {UserName}@{DomainName}", startupInfo.UserName, startupInfo.DomainName);
            Log.Information("🖥️  Machine: {MachineName}", startupInfo.MachineName);
            Log.Information("📂 Application Path: {ApplicationPath}", startupInfo.ApplicationPath);
            
            if (startupInfo.ServerIPAddresses.Any())
            {
                Log.Information("🌐 Server IP Addresses: {IPAddresses}", 
                    string.Join(", ", startupInfo.ServerIPAddresses));
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to log startup details");
        }
    }

    /// <summary>
    /// Başlangıç bilgilerini toplar
    /// </summary>
    /// <param name="app">WebApplication instance'ı</param>
    /// <returns>StartupInfo nesnesi</returns>
    private static async Task<StartupInfo> BuildStartupInfoAsync(WebApplication app)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyName = assembly.GetName();
        
        return new StartupInfo
        {
            ProjectName = assemblyName.Name ?? "Template.API",
            Version = assemblyName.Version?.ToString() ?? "1.0.0.0",
            BuildDate = GetBuildDate(assembly),
            StartTime = DateTime.UtcNow,
            Environment = app.Environment.EnvironmentName,
            OS = Environment.OSVersion.ToString(),
            Runtime = Environment.Version.ToString(),
            ProcessId = Environment.ProcessId,
            ApplicationPath = AppContext.BaseDirectory,
            UserName = Environment.UserName,
            DomainName = Environment.UserDomainName,
            MachineName = Environment.MachineName,
            ServerIPAddresses = await GetServerIPAddressesAsync()
        };
    }

    /// <summary>
    /// Assembly build tarihini alır
    /// </summary>
    /// <param name="assembly">Assembly</param>
    /// <returns>Build tarihi</returns>
    private static DateTime GetBuildDate(Assembly assembly)
    {
        try
        {
            var attribute = assembly.GetCustomAttribute<System.Reflection.AssemblyMetadataAttribute>();
            if (attribute?.Key == "BuildDate" && DateTime.TryParse(attribute.Value, out var buildDate))
            {
                return buildDate;
            }

            // Fallback: assembly dosya tarihini kullan
            var assemblyLocation = assembly.Location;
            if (!string.IsNullOrEmpty(assemblyLocation) && File.Exists(assemblyLocation))
            {
                return File.GetCreationTime(assemblyLocation);
            }
        }
        catch
        {
            // Ignore errors
        }

        return DateTime.UtcNow;
    }

    /// <summary>
    /// Sunucu IP adreslerini alır
    /// </summary>
    /// <returns>IP adresleri listesi</returns>
    private static async Task<List<string>> GetServerIPAddressesAsync()
    {
        var ipAddresses = new List<string>();
        
        try
        {
            var hostName = Dns.GetHostName();
            var addresses = await Dns.GetHostAddressesAsync(hostName);
            
            ipAddresses.AddRange(
                addresses
                    .Where(addr => addr.AddressFamily is AddressFamily.InterNetwork or AddressFamily.InterNetworkV6)
                    .Where(addr => !IPAddress.IsLoopback(addr))
                    .Select(addr => addr.ToString())
            );
        }
        catch
        {
            // Ignore DNS resolution errors
        }

        return ipAddresses;
    }

    /// <summary>
    /// Başlangıç bilgileri modeli
    /// </summary>
    private record StartupInfo
    {
        public string ProjectName { get; init; } = string.Empty;
        public string Version { get; init; } = string.Empty;
        public DateTime BuildDate { get; init; }
        public DateTime StartTime { get; init; }
        public string Environment { get; init; } = string.Empty;
        public string OS { get; init; } = string.Empty;
        public string Runtime { get; init; } = string.Empty;
        public int ProcessId { get; init; }
        public string ApplicationPath { get; init; } = string.Empty;
        public string UserName { get; init; } = string.Empty;
        public string DomainName { get; init; } = string.Empty;
        public string MachineName { get; init; } = string.Empty;
        public List<string> ServerIPAddresses { get; init; } = new();
    }
} 