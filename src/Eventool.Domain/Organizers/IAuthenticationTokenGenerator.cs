namespace Eventool.Domain.Organizers;

public interface IAuthenticationTokenGenerator
{
    public string Generate(Organizer organizer);
}