using Eventool.Domain.Events;
using Marten;
using Marten.Pagination;

namespace Eventool.Infrastructure.Persistence.Events;

public class EventRepository(IDocumentSession session)
    : Repository<Guid, Event, EventDocument>(session), IEventRepository
{
    private const int PageSize = 15;

    public Event Save(Event @event)
    {
        @event.Changed();
        Session.Store(EventDocument.Create(@event));
        return @event;
    }

    public async Task<IEnumerable<Event>> GetListByCreatorIdAsync(Guid creatorId, int pageNumber, CancellationToken ct) =>
        (await Session
            .Query<EventDocument>()
            .Where(document => document.CreatorId == creatorId)
            .OrderByDescending(x => x.ChangedAtUtc)
            .ToPagedListAsync(pageNumber, PageSize, ct))
        .Select(document => document.ToDomainObject());

    public async Task<Event?> GetByIdAsync(Guid eventId, CancellationToken ct) =>
        (await Session.LoadAsync<EventDocument>(eventId, ct))?.ToDomainObject();
}