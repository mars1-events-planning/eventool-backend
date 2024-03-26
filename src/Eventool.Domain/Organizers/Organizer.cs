using Eventool.Domain.Common;

namespace Eventool.Domain.Organizers;

public class Organizer(
    Guid id,
    string fullname,
    string username,
    HashedPassword hashedPassword)
    : Entity<Guid>(id), IAggregateRoot
{
    public string Fullname { get; private set; } = fullname;
    public string Username { get; private set; } = username;
    public HashedPassword HashedPassword { get; private set; } = hashedPassword;
    
    public void ChangePassword(HashedPassword newPassword) => HashedPassword = newPassword;
    
    public void ChangeFullname(string newFullname) => Fullname = newFullname;
    
    public void ChangeUsername(string newUsername) => Username = newUsername;
}