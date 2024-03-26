using Eventool.Domain.Organizers;
using Eventool.Infrastructure.Persistence;
using Eventool.Infrastructure.Utility;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Weasel.Core;

namespace Eventool.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureLayer(
        this IServiceCollection serviceCollection,
        IConfiguration configuration) =>
        serviceCollection
            .AddPersistence(configuration)
            .AddTransient<IAuthenticationTokenGenerator, JwtGenerator>();

    private static IServiceCollection AddPersistence(
        this IServiceCollection serviceCollection,
        IConfiguration configuration) =>
        serviceCollection
            .AddMarten(options =>
            {
                options.Connection(configuration.GetConnectionString("Database")!);
                options.AutoCreateSchemaObjects = AutoCreate.All;
                options.DatabaseSchemaName = "eventool";
                options.UseDefaultSerialization(
                    casing: Casing.SnakeCase, 
                    enumStorage: EnumStorage.AsString,
                    collectionStorage: CollectionStorage.AsArray);
            })
            .OptimizeArtifactWorkflow()
            .Services
            .AddTransient<IUnitOfWork, UnitOfWork>();
}