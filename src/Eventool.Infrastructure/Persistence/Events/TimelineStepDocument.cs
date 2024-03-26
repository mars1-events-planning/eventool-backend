using Eventool.Domain.Events;
using Marten.Schema;
using Newtonsoft.Json;

namespace Eventool.Infrastructure.Persistence.Events;

public class TimelineStepDocument : IDocument<TimelineStepDocument, TimelineStep>
{
    [Identity] [JsonProperty("id")] public Guid Id { get; set; }

    [JsonProperty("title")] public string Title { get; set; } = null!;

    [JsonProperty("description")] public string? Description { get; set; }

    [JsonProperty("address")] public string Address { get; set; } = null!;
    
    [JsonProperty("happens_at")] public DateTime HappensAt { get; set; }

    [JsonProperty("checklists")] public IEnumerable<ChecklistDocument> Checklists { get; set; } = [];

    public static TimelineStepDocument Create(TimelineStep domainObject) => new()
    {
        Id = domainObject.Id,
        Title = domainObject.Title,
        Description = domainObject.Description,
        Checklists = domainObject.Checklists.Select(ChecklistDocument.Create)
    };

    public TimelineStep ToDomainObject()
    {
        var step = new TimelineStep(Id, Title) { Description = Description };
        foreach (var checklist in Checklists.Select(x => x.ToDomainObject()))
            step.AddChecklist(checklist);

        return step;
    }
}