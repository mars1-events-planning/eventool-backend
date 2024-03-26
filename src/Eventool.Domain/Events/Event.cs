using Eventool.Domain.Common;

namespace Eventool.Domain.Events;

public class Event(
    Guid id,
    Guid creatorId,
    DateTime createdAtUtc,
    string title)
    : Entity<Guid>(id), IAggregateRoot
{
    private DateTime _changedAtUtc = EnsureUtc(createdAtUtc);
    private string? _description;
    private string? _address;
    private DateTime? _startAtUtc;
    private List<Checklist> _checklists = [];

    public DateTime CreatedAt { get; } = EnsureUtc(createdAtUtc);

    public DateTime ChangedAtUtc
    {
        get => _changedAtUtc;
        init => _changedAtUtc = EnsureUtc(value);
    }

    public void Changed() => _changedAtUtc = DateTime.UtcNow;

    public string Title { get; private set; } = title;

    public Guid CreatorId { get; } = creatorId;

    public string? Description
    {
        get => _description;
        init => _description = value;
    }

    public string? Address
    {
        get => _address;
        init => _address = value;
    }

    public DateTime? StartAtUtc
    {
        get => _startAtUtc;
        init => _startAtUtc = value.HasValue ? EnsureUtc(value.Value) : null;
    }

    public IReadOnlyList<Checklist> Checklists => _checklists.AsReadOnly();

    public void AddChecklist(Checklist checklist) => _checklists.Add(checklist);

    public void SetTitle(string title) => Title = title;

    public void SetDescription(string? description) => _description = description;

    public void SetAddress(string? address) => _address = address;

    public void SetStartAtUtc(DateTime? startAtUtc) =>
        _startAtUtc = startAtUtc.HasValue ? EnsureUtc(startAtUtc.Value) : null;

    private static DateTime EnsureUtc(DateTime dt) => dt.Kind is DateTimeKind.Utc
        ? dt
        : throw new ArgumentException("DateTime should be UTC!");
}