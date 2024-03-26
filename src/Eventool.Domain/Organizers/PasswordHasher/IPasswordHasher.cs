namespace Eventool.Domain.Organizers;

public interface IPasswordHasher
{
    HashedPassword Hash(string password);

    bool IsSame(string password, HashedPassword hashedPassword);
}