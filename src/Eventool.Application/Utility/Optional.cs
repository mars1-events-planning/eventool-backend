namespace Eventool.Application.Utility;

public readonly struct Optional<T>
{
    private readonly T? _value = default;
    private readonly bool _isSet = false;
    
    private Optional(T? value, bool isSet) => (_value, _isSet) = (value, isSet);

    public bool IsSet => _isSet;

    public T? Value => _isSet ? _value : throw new InvalidOperationException("Value not set");

    public static Optional<T> NotSet() => new(default, false);

    public static Optional<T> From(T? value) => new(value, true);

    public void IfSet(Action<T> action)
    {
        if (IsSet)
            action(Value!);
    }
}