namespace Eventool.Domain.Events;

public interface IEventRepository
{
    public Event Save(Event @event);

    public Task<IEnumerable<Event>> GetListByCreatorIdAsync(Guid creatorId, int pageNumber, CancellationToken ct);

    public Task<Event?> GetByIdAsync(Guid eventId, CancellationToken ct);
}