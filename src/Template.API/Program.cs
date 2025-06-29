using Serilog;
using Serilog.Events;
using Template.API.Extensions;
using Template.API.Infrastructure;
using Template.Application.Extensions;
using Template.Infrastructure.Extensions;
using Template.Persistence.Extensions;

// Serilog konfigürasyonu - Erken kurulum
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog'u appsettings.json'dan yapılandır
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithEnvironmentUserName();

        // PostgreSQL connection string'i environment variable'dan al
        var postgresConnectionString = context.Configuration.GetConnectionString("Logging:PostgreSql:ConnectionString");
        if (!string.IsNullOrEmpty(postgresConnectionString))
        {
            // PostgreSQL sink ayarlarını manuel olarak ekle
            configuration.WriteTo.PostgreSQL(
                connectionString: postgresConnectionString,
                tableName: "Logs",
                needAutoCreateTable: true,
                batchSizeLimit: 50,
                period: TimeSpan.FromSeconds(5)
            );
        }
    });

    // Layer Services - Clean Architecture DI Orchestration
    builder.Services
        .AddApplicationServices()
        .AddPersistenceServices(builder.Configuration)
        .AddInfrastructureServices(builder.Configuration)
        .AddApiServices(builder.Configuration);

    var app = builder.Build();

    // Initialize database (migrations and seeding)
    await app.InitializeDatabaseAsync();

    // Configure HTTP Pipeline
    await ConfigurePipelineAsync(app);

    // Database health check before starting
    await app.Services.CheckDatabaseHealthAsync(app.Logger);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// HTTP Pipeline'ını yapılandırır
/// </summary>
/// <param name="app">WebApplication instance'ı</param>
static async Task ConfigurePipelineAsync(WebApplication app)
{
    // Serilog HTTP request logging
    app.UseSerilogRequestLogging();

    // Development pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Template API v1");
            c.RoutePrefix = "swagger";
            c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        });
        
        // Development CORS (daha esnek)
        app.UseCors("DevelopmentCors");
    }
    else
    {
        // Production pipeline
        app.UseExceptionHandler("/Error");
        app.UseHsts();
        
        // Production CORS (kısıtlı)
        app.UseCors("AllowSpecificOrigins");
    }

    // Common pipeline
    app.UseHttpsRedirection();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // Health checks
    app.MapHealthChecks("/health");

    // Startup bilgilerini logla
    await StartupLogger.LogDetailsAsync(app);
    
    Log.Information("✅ Application pipeline configured and started successfully");
}
