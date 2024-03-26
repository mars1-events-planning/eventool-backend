namespace Eventool.Domain.Organizers;

public interface IOrganizersRepository
{
    public Task<Organizer?> TryGetByIdAsync(Guid id, CancellationToken cancellationToken);
    
    public Task<Organizer> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    
    public Task<Organizer?> GetByUsernameAsync(string username, CancellationToken cancellationToken);
    
    public void Save(Organizer organizer);

    public Task<bool> UsernameTakenAsync(string username, CancellationToken cancellationToken);
    
    public Task EnsureOrganizerExistsAsync(Guid organizerId, CancellationToken cancellationToken);
}