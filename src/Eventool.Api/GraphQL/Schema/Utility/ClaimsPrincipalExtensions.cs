using System.Security.Claims;
using Eventool.Infrastructure.Utility;

namespace Eventool.Api.GraphQL.Schema.Utility;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetOrganizerId(this ClaimsPrincipal claims) =>
        Guid.Parse(claims.GetOrganizerIdString());
        
    public static string GetOrganizerIdString(this ClaimsPrincipal claims) => claims
        .Claims
        .Single(x => x.Type == JwtClaims.OrganizerId)
        .Value;
}