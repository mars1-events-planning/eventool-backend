using Eventool.Domain.Common;
using Eventool.Domain.Organizers;
using Eventool.Infrastructure.Images;
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
            .AddTransient<IAuthenticationTokenGenerator, JwtGenerator>()
            .AddTransient<IImageStorage, S3ImageStorage>()
            .Configure<S3Options>(configuration.GetSection(S3Options.Section));

    private static IServiceCollection AddPersistence(
        this IServiceCollection serviceCollection,
        IConfiguration configuration) =>
        serviceCollection
            .AddMarten(options =>
            {
                options.Connection(GetConnectionString(configuration));
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

    private static string GetConnectionString(IConfiguration c) =>
        $"Host={c["Database:Host"]};Port={c["Database:Port"]};Database={c["Database:Database"]};Username={c["Database:Username"]};Password={c["Database:Password"]};";
}