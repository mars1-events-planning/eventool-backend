using System.Security.Claims;
using Eventool.Api.GraphQL.Schema.Events.OutputTypes;
using Eventool.Api.GraphQL.Schema.Utility;
using Eventool.Application.UseCases;
using Eventool.Domain.Events;
using HotChocolate.Authorization;
using MediatR;
using ValidationException = FluentValidation.ValidationException;
using UnauthorizedAccessException = Eventool.Application.Utility.UnauthorizedAccessException;

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
        var organizer = await mediator.Send(
            new ChangePasswordRequest(oldPassword, newPassword, claimsPrincipal.GetOrganizerId()),
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
        var organizer = await mediator.Send(
            new EditOrganizerRequest(claimsPrincipal.GetOrganizerId(), fullName, username),
            cancellationToken);

        return new GqlOrganizer(organizer);
    }

    [Authorize]
    [GraphQLName("saveEvent")]
    [Error<ValidationException>]
    [Error<UnauthorizedAccessException>]
    public async Task<GqlEvent> SaveEventAsync(
        [Service] IMediator mediator,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken,
        EventInput input)
    {
        var request = new SaveEventRequest(
            OrganizerId: claimsPrincipal.GetOrganizerId(),
            EventChanges: input.MapToRequest());

        var @event = await mediator.Send(request, cancellationToken);

        return new GqlEvent(@event);
    }
}

public record EventInput(
    Optional<Guid?> EventId,
    Optional<string?> Title,
    Optional<string?> Description,
    Optional<string?> Address,
    Optional<DateTime?> StartDateTimeUtc,
    Optional<IEnumerable<ChecklistInput>?> Checklists,
    Optional<IEnumerable<GuestInput>?> Guests
);

public record ChecklistInput(
    Optional<Guid?> Id,
    string Title,
    IEnumerable<ChecklistItem> ChecklistItems
);

public record GuestInput(
    Optional<Guid?> Id,
    string Name,
    string Contact,
    IEnumerable<string> Tags
);
