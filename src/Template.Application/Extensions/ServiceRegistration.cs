using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Template.Application.Extensions;

/// <summary>
/// Application katmanı servislerini DI container'a ekleyen extension metotları
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Application katmanı servislerini IServiceCollection'a ekler
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // MediatR - CQRS Command/Query pattern için
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        // AutoMapper - Object mapping için
        services.AddAutoMapper(assembly);

        // FluentValidation - Input validation için
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
} 