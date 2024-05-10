using System.Text;
using Eventool.Api.GraphQL.Infrastructure;
using Eventool.Application;
using Eventool.Domain;
using Eventool.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    builder.Configuration
        .AddJsonFile("appsettings.Production.json")
        .AddKeyPerFile("/run/secrets");
}

builder.Services
    .AddApplicationLayer(builder.Configuration)
    .AddInfrastructureLayer(builder.Configuration)
    .AddDomainLayer(builder.Configuration);

var policyName = "allow-specific-origin";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: policyName,
        policy =>
        {
            policy
                .SetIsOriginAllowed(origin =>
                {
                    if (origin == "https://eventool.online")
                        return true;

                    var host = new Uri(origin).Host;

                    return host == "localhost";
                })
                .AllowAnyHeader()
                .AllowCredentials()
                .AllowAnyMethod();
        });
});

builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddApiTypes()
    .AddMutationConventions()
    .AddErrorFilter<ValidationErrorsFilter>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

var app = builder.Build();

app.UseCors(policyName);

app.UseAuthentication();

app.MapImageEndpoints();

app.MapGraphQL();

app.Run();