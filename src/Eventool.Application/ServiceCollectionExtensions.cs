using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eventool.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayer(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        return serviceCollection
            .AddMediatR(c => c.RegisterServicesFromAssemblyContaining<Marker>())
            .AddValidatorsFromAssemblyContaining<Marker>();
    }
}

public abstract record Marker();