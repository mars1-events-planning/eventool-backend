using Eventool.Domain.Organizers;
using Marten.Schema;
using Newtonsoft.Json;

namespace Eventool.Infrastructure.Persistence;

[DocumentAlias("organizers")]
public class OrganizerDocument : IDocument<OrganizerDocument, Organizer>
{
    [Identity] 
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("full_name")]
    public string FullName { get; set; } = null!;

    [JsonProperty("username")]
    public string Username { get; set; } = null!;

    [JsonProperty("password_hash")]
    public string PasswordHash { get; set; } = null!;

    [JsonProperty("password_salt")]
    public string PasswordSalt { get; set; } = null!;

    public Organizer ToDomainObject() => new(
        Id,
        FullName,
        Username, 
        new HashedPassword(PasswordHash, PasswordSalt)
    );

    public static OrganizerDocument Create(Organizer domainObject) => new()
    {
        Id = domainObject.Id,
        FullName = domainObject.Fullname,
        Username = domainObject.Username,
        PasswordHash = domainObject.HashedPassword.Value,
        PasswordSalt = domainObject.HashedPassword.Salt
    };
}