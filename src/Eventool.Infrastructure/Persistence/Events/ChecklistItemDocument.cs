using Eventool.Domain.Events;
using Newtonsoft.Json;

namespace Eventool.Infrastructure.Persistence.Events;

public class ChecklistItemDocument : IDocument<ChecklistItemDocument, ChecklistItem>
{
    [JsonProperty("title")] public string Title { get; set; } = null!;
    [JsonProperty("done")] public bool Done { get; set; }

    public ChecklistItem ToDomainObject() => new(Title, Done);

    public static ChecklistItemDocument Create(ChecklistItem domainObject) =>
        new() { Title = domainObject.Title, Done = domainObject.Done };
}