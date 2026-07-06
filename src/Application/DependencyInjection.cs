using Application.Auth;
using Application.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

/// <summary>רישום שירותי ה-use-cases של שכבת Application.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<EventIngestionService>();
        services.AddScoped<AuthService>();
        return services;
    }
}
