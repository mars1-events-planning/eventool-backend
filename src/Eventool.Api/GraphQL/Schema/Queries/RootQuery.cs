using System.Security.Claims;
using Eventool.Api.GraphQL.Schema.Events.OutputTypes;
using Eventool.Application.UseCases;
using Eventool.Infrastructure.Persistence;
using Eventool.Infrastructure.Utility;
using HotChocolate.Authorization;
using MediatR;

namespace Eventool.Api.GraphQL.Schema;

public class NotFoundByUsernameException()
    : Exception("Пользователь с таким логином не найден!");

[QueryType]
public class RootQuery
{
    [Authorize]
    [GraphQLName("authorized")]
    public bool Authorized => true;
    
    [Authorize]
    [GraphQLName("organizer")]
    public async Task<GqlOrganizer?> GetCurrentOrganizerAsync(
        [Service] IUnitOfWork uow,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken) =>
        await uow.ExecuteReadOnlyAsync(async repositories =>
        {
            var organizerId = claimsPrincipal.Claims
                .Single(x => x.Type == JwtClaims.OrganizerId).Value;
            var guid = Guid.Parse(organizerId);
            var organizer =
                await repositories.OrganizersRepository.TryGetByIdAsync(guid, cancellationToken);

            return organizer is null
                ? null
                : new GqlOrganizer(organizer);
        }, cancellationToken);

    [Authorize]
    [GraphQLName("organizerByUsername")]
    public async Task<GqlOrganizer?> GetOrganizerByUsernameAsync(
        [Service] IUnitOfWork uow,
        string username,
        CancellationToken cancellationToken) =>
        await uow.ExecuteReadOnlyAsync(async repositories =>
        {
            var organizer =
                await repositories.OrganizersRepository.GetByUsernameAsync(username, cancellationToken);

            return organizer is null
                ? null
                : new GqlOrganizer(organizer);
        }, cancellationToken);

    [Authorize]
    [GraphQLName("events")]
    public async Task<IEnumerable<GqlEvent>> GetEventsAsync(
        [Service] IMediator mediator,
        int page,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var organizerEvents = await mediator.Send(new GetEventsByOrganizerRequest(
            Guid.Parse(claimsPrincipal.Claims.Single(x => x.Type == JwtClaims.OrganizerId).Value), page), 
            cancellationToken);

        return organizerEvents.Select(x => new GqlEvent(x));
    }
    
    [Authorize]
    [GraphQLName("event")]
    public async Task<GqlEvent?> GetEventByIdAsync(
        [Service] IMediator mediator,
        string eventId,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var @event = await mediator.Send(new GetByIdRequest(
            Guid.Parse(eventId), Guid.Parse(claimsPrincipal.Claims.Single(x => x.Type == JwtClaims.OrganizerId).Value)), 
            cancellationToken);

        return @event is null ? null : new GqlEvent(@event);
    }
}