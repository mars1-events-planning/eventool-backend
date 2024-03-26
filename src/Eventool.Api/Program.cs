using System.Text;
using Eventool.Api.GraphQL.Infrastructure;
using Eventool.Application;
using Eventool.Domain;
using Eventool.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplicationLayer(builder.Configuration)
    .AddInfrastructureLayer(builder.Configuration)
    .AddDomainLayer(builder.Configuration);

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

app.UseAuthentication();

app.MapGraphQL();

app.Run();