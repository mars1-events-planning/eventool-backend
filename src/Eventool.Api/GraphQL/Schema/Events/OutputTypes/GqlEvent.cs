using Eventool.Domain.Events;
using Eventool.Infrastructure.Persistence;

namespace Eventool.Api.GraphQL.Schema.Events.OutputTypes;

public class GqlEvent(Event @event)
{
    public Guid Id { get; } = @event.Id;
    
    public string Title { get; } = @event.Title;

    public string? Address { get; } = @event.Address;

    public string? Description { get; } = @event.Description;
    
    public DateTime CreatedAtUtc { get; } = @event.CreatedAt;
    
    public DateTime ChangedAtUtc { get; } = @event.ChangedAtUtc;

    public DateTime? StartAtUtc { get; } = @event.StartAtUtc;

    public IEnumerable<string> Photos { get; } = @event.ImagesUrls;
    
    public IEnumerable<GqlChecklist> Checklists { get; } = @event.Checklists.Select(x => new GqlChecklist(x));

    public IEnumerable<GqlGuest> Guests { get; } = @event.Guests.Select(x => new GqlGuest(x));

    [GraphQLName("creator")]
    public async Task<GqlOrganizer> GetCreator(
        [Service] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken) => await unitOfWork.ExecuteReadOnlyAsync(
        action: async repositories =>
        {
            var organizer = await repositories.OrganizersRepository.TryGetByIdAsync(@event.CreatorId, cancellationToken);
            return organizer is null ? throw new NotFoundByUsernameException() : new GqlOrganizer(organizer);
        }, cancellationToken);
}

public class GqlGuest(Guest guest)
{
    public Guid Id { get; } = guest.Id;

    public string Name { get; } = guest.Name;

    public string Contact { get; } = guest.Contact;

    public string? PhotoUrl { get; } = guest.PhotoUrl;

    public IEnumerable<string> Tags { get; } = guest.Tags;
}

public class GqlChecklist(Checklist checklist)
{
    public Guid Id { get; } = checklist.Id;
    
    public string Title { get; } = checklist.Title;

    public IEnumerable<GqlChecklistItem> Items { get; } = checklist.ChecklistItems.Select(x => new GqlChecklistItem(x));
}

public class GqlChecklistItem(ChecklistItem item)
{
    public string Title { get; } = item.Title;

    public bool Done { get; } = item.Done;
}