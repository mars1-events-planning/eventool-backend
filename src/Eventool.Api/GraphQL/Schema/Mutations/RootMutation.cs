using System.Security.Claims;
using Eventool.Api.GraphQL.Schema.Events.OutputTypes;
using Eventool.Api.GraphQL.Schema.Utility;
using Eventool.Application.UseCases;
using Eventool.Domain.Events;
using Eventool.Infrastructure.Utility;
using HotChocolate.Authorization;
using MediatR;
using UnauthorizedAccessException = Eventool.Application.UseCases.UnauthorizedAccessException;
using ValidationException = FluentValidation.ValidationException;

namespace Eventool.Api.GraphQL.Schema;

[MutationType]
[GraphQLName(nameof(RootMutation))]
public class RootMutation
{
    [GraphQLName("registerOrganizer")]
    [Error<ValidationException>]
    public async Task<MutationResult<GqlOrganizer>> RegisterAsync(
        string username,
        string fullName,
        string password,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var organizer = await mediator.Send(
            new RegisterOrganizerRequest(username, fullName, password),
            cancellationToken);

        return new GqlOrganizer(organizer);
    }

    [GraphQLName("login")]
    [Error<UserNotFoundByUsernameException>]
    [Error<WrongPasswordException>]
    [Error<UserNameShouldBeFilledException>]
    public async Task<GqlToken> LoginAsync(
        string username,
        string password,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var token = await mediator.Send(
            new LoginOrganizerRequest(username, password),
            cancellationToken);

        return new GqlToken(token);
    }
    
    [Authorize]
    [GraphQLName("changePassword")]
    [Error<ValidationException>]
    [Error<WrongPasswordException>]
    public async Task<GqlOrganizer> ChangePasswordAsync(
        string oldPassword,
        string newPassword,
        [Service] IMediator mediator,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var organizerId = claimsPrincipal.Claims
            .Single(x => x.Type == JwtClaims.OrganizerId).Value;

        var guid = Guid.Parse(organizerId);

        var organizer = await mediator.Send(
            new ChangePasswordRequest(oldPassword, newPassword, guid),
            cancellationToken);

        return new GqlOrganizer(organizer);
    }
    
    [Authorize]
    [GraphQLName("editOrganizer")]
    [Error<ValidationException>]
    public async Task<GqlOrganizer> EditOrganizerAsync(
        [Service] IMediator mediator,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken,
        string? username = null,
        string? fullName = null)
    {
        var organizerId = claimsPrincipal.Claims
            .Single(x => x.Type == JwtClaims.OrganizerId).Value;
        var guid = Guid.Parse(organizerId);

        var organizer = await mediator.Send(
            new EditOrganizerRequest(guid, fullName, username),
            cancellationToken);

        return new GqlOrganizer(organizer);
    }
    
    [Authorize]
    [GraphQLName("createEvent")]
    [Error<ValidationException>]
    public async Task<GqlEvent> CreateEventAsync(
        string title,
        [Service] IMediator mediator,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var organizerId = claimsPrincipal.Claims
            .Single(x => x.Type == JwtClaims.OrganizerId).Value;
        var guid = Guid.Parse(organizerId);

        var @event = await mediator.Send(
            new CreateEventCommand(title, guid),
            cancellationToken);

        return new GqlEvent(@event);
    }
    
    [Authorize]
    [GraphQLName("editEvent")]
    [Error<ValidationException>]
    [Error<UnauthorizedAccessException>]
    public async Task<GqlEvent> EditEventAsync(
        [Service] IMediator mediator,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken,
        Guid eventId,
        string title,
        string? address = null,
        string? description = null,
        DateTime? startDateTimeUtc = null)
    {
        var organizerId = claimsPrincipal
            .Claims
            .Single(x => x.Type == JwtClaims.OrganizerId)
            .Value;

        var guid = Guid.Parse(organizerId);
        
        var @event = await mediator.Send(
            new EditEventRequest(eventId, guid, title, address, description, startDateTimeUtc),
            cancellationToken);

        return new GqlEvent(@event);
    }
    
    [Authorize]
    [GraphQLName("saveChecklist")]
    [Error<ValidationException>]
    [Error<UnauthorizedAccessException>]
    public async Task<GqlEvent> SaveChecklistAsync(
        [Service] IMediator mediator,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken,
        Guid eventId,
        string title,
        IEnumerable<ChecklistItem> items,
        Guid? checklistId = null)
    {
        var organizerId = claimsPrincipal
            .Claims
            .Single(x => x.Type == JwtClaims.OrganizerId)
            .Value;

        var guid = Guid.Parse(organizerId);
        
        var @event = await mediator.Send(
            new SaveChecklistRequest(eventId, guid, title, items, checklistId),
            cancellationToken);

        return new GqlEvent(@event);
    }
}