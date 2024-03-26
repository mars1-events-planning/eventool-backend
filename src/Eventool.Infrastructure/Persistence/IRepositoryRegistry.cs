using Eventool.Domain.Events;
using Eventool.Domain.Organizers;
using Eventool.Infrastructure.Persistence.Events;
using Marten;

namespace Eventool.Infrastructure.Persistence;

public interface IRepositoryRegistry
{
    IOrganizersRepository OrganizersRepository { get; }
    
    IEventRepository EventRepository { get; }
}

public class RepositoryRegistry(IDocumentSession session) : IRepositoryRegistry
{
    public IOrganizersRepository OrganizersRepository { get; } = new OrganizersRepository(session);

    public IEventRepository EventRepository { get; } = new EventRepository(session);
}