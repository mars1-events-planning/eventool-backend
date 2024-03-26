using Microsoft.Extensions.DependencyInjection;

namespace Eventool.Domain.Organizers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrganizerModule(this IServiceCollection services) =>
        services
            .AddScoped<IPasswordHasher, PasswordHasher>();
}