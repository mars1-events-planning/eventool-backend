namespace Eventool.Domain.Common;

public class Entity<TId>(TId id)
    where TId : struct
{
    public TId Id { get; } = id;

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other || GetType() != other.GetType())
            return false;

        return ReferenceEquals(this, other) || Id.Equals(other.Id);
    }
    
    public static bool operator ==(Entity<TId>? a, Entity<TId>? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }
    
    public static bool operator !=(Entity<TId>? a, Entity<TId>? b) => !(a == b);

    public override int GetHashCode() => Id.GetHashCode();

    public override string ToString() => $"{GetType().Name} [Id: {Id}]";
}