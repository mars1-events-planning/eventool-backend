using Eventool.Domain.Organizers;

namespace Eventool.Api.GraphQL.Schema;

[GraphQLName("Organizer")]
public class GqlOrganizer(Organizer organizer)
{
    [GraphQLName("id")]
    public string Id { get; } = organizer.Id.ToString();
    
    [GraphQLName("fullname")]
    public string FullName { get; } = organizer.Fullname;

    [GraphQLName("username")] 
    public string Username { get; } = organizer.Username;
}