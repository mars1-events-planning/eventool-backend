using Eventool.Domain.Common;

namespace Eventool.Domain.Events;

public record ChecklistItem(string Title, bool Done);

public class Checklist(Guid id, string title) : Entity<Guid>(id)
{
    private List<ChecklistItem> _checklistItems = [];
    
    public string Title { get; private set; } = title;

    public IReadOnlyList<ChecklistItem> ChecklistItems => _checklistItems.AsReadOnly();

    public void SetItems(IEnumerable<ChecklistItem> checklistItems) => _checklistItems = checklistItems.ToList();
    
    public void SetTitle(string title) => Title = title;
}