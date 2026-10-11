using Microsoft.Extensions.DependencyInjection;

namespace SkillForge.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // EF Core DbContext, JWT, and other infrastructure registrations will be added here.
        return services;
    }
}
