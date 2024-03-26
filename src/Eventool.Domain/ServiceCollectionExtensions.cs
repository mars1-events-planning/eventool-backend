using Eventool.Domain.Organizers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eventool.Domain;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainLayer(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        return serviceCollection
            .AddOrganizerModule();
    }
}