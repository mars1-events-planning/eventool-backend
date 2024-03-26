using Eventool.Domain.Common;

namespace Eventool.Domain.Events;

public class TimelineStep(Guid id, string title) : Entity<Guid>(id)
{
    private string? _description = null!;
    private readonly List<Checklist> _checklists = [];
    
    public string Title { get; private set; } = title;
    public string? Description { get => _description; init => _description = value; }

    public IReadOnlyList<Checklist> Checklists => _checklists.AsReadOnly();
    
    public void AddChecklist(Checklist checklist) => _checklists.Add(checklist);

    public void RemoveChecklist(Checklist checklist) => _checklists.Remove(checklist);


    public void SetTitle(string title) => Title = title;

    public void SetDescription(string? description) => _description = description;
}