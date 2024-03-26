using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Eventool.Domain.Organizers;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Eventool.Infrastructure.Utility;

public class JwtGenerator(IConfiguration configuration)
    : IAuthenticationTokenGenerator
{
    public string Generate(Organizer organizer)
    {
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"]!,
            audience: configuration["Jwt:Audience"]!,
            claims: [new Claim(JwtClaims.OrganizerId, organizer.Id.ToString())],
            expires: DateTime.Now.AddHours(12),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public static class JwtClaims
{
    public const string OrganizerId = "organizerId";
}