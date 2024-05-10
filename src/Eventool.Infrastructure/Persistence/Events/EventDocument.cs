using Eventool.Domain.Events;
using Marten.Schema;
using Newtonsoft.Json;

namespace Eventool.Infrastructure.Persistence.Events;

[DocumentAlias("events")]
public class EventDocument : IDocument<EventDocument, Event>
{
    [Identity] [JsonProperty("id")] public Guid Id { get; set; }

    [JsonProperty("title")] public string Title { get; set; } = null!;

    [JsonProperty("address")] public string? Address { get; set; }

    [JsonProperty("description")] public string? Description { get; set; }

    [JsonProperty("created_at")] public DateTime CreatedAt { get; set; }

    [JsonProperty("changed_at_utc")] public DateTime ChangedAtUtc { get; set; }

    [JsonProperty("start_at_utc")] public DateTime? StartAtUtc { get; set; }

    [JsonProperty("creator_id")] public Guid CreatorId { get; set; }

    [JsonProperty("photos")] public IEnumerable<string> Photos { get; set; } = [];

    [JsonProperty("checklists")] public IEnumerable<ChecklistDocument> Checklists { get; set; } = [];

    [JsonProperty("guests")] public IEnumerable<GuestDocument> Guests { get; set; } = [];
    
    public Event ToDomainObject()
    {
        var e = new Event(Id, CreatorId, createdAtUtc: CreatedAt, Title)
        {
            Address = Address,
            Description = Description,
            ChangedAtUtc = ChangedAtUtc,
            StartAtUtc = StartAtUtc
        };
        foreach (var photo in Photos)
            e.AddImageUrl(photo);
        
        foreach (var checklist in Checklists)
            e.AddChecklist(checklist.ToDomainObject());
        
        foreach (var guest in Guests)
            e.AddGuest(guest.ToDomainObject());

        return e;
    }

    public static EventDocument Create(Event domainObject) => new()
    {
        Id = domainObject.Id,
        Title = domainObject.Title,
        Address = domainObject.Address,
        Description = domainObject.Description,
        CreatorId = domainObject.CreatorId,
        CreatedAt = domainObject.CreatedAt,
        ChangedAtUtc = domainObject.ChangedAtUtc,
        StartAtUtc = domainObject.StartAtUtc,
        Checklists = domainObject.Checklists.Select(ChecklistDocument.Create),
        Guests = domainObject.Guests.Select(GuestDocument.Create),
        Photos = domainObject.ImagesUrls
    };
}

[DocumentAlias("guests")]
public class GuestDocument : IDocument<GuestDocument, Guest>
{
    [Identity] [JsonProperty("id")] public Guid Id { get; set; }

    [JsonProperty("name")] public string Name { get; set; }

    [JsonProperty("photo_url")] public string? PhotoUrl { get; set; }

    [JsonProperty("contact")] public string Contact { get; set; }

    [JsonProperty("tags")] public IEnumerable<string> Tags { get; set; } = [];

    public Guest ToDomainObject() => new Guest(Id, Name, Contact)
    {
        Tags = Tags,
        PhotoUrl = PhotoUrl
    };

    public static GuestDocument Create(Guest domainObject) => new()
    {
        Id = domainObject.Id,
        Contact = domainObject.Contact,
        Name = domainObject.Name,
        PhotoUrl = domainObject.PhotoUrl,
        Tags = domainObject.Tags
    };
}