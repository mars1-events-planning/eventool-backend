using Eventool.Domain.Common;
using Eventool.Domain.Organizers;
using Marten;

namespace Eventool.Infrastructure.Persistence;

public class OrganizersRepository(IDocumentSession session) :
    Repository<Guid, Organizer, OrganizerDocument>(session),
    IOrganizersRepository
{
    public async Task<Organizer?> TryGetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await Session.Query<OrganizerDocument>()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return document?.ToDomainObject();
    }

    public async Task<Organizer> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await TryGetByIdAsync(id, cancellationToken)
        ?? throw new DomainException("Организатор не найден!")
        {
            Data = { ["organizerId"] = id }
        };

    public async Task<Organizer?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        var document = await Session.Query<OrganizerDocument>()
            .SingleOrDefaultAsync(x => x.Username == username, cancellationToken);

        return document?.ToDomainObject();
    }

    public void Save(Organizer organizer)
    {
        Session.Store(OrganizerDocument.Create(organizer));
    }

    public async Task<bool> UsernameTakenAsync(string username, CancellationToken cancellationToken) =>
        await Session
            .Query<OrganizerDocument>()
            .AnyAsync(x => x.Username == username, token: cancellationToken);

    public async Task EnsureOrganizerExistsAsync(Guid organizerId, CancellationToken cancellationToken)
    {
        var organizerExists = await TryGetByIdAsync(organizerId, cancellationToken) is not null;
        if (organizerExists)
            return;

        throw new DomainException("Организатор не найден!")
        {
            Data = { ["organizerId"] = organizerId }
        };
    }
}