namespace VSATemplate.Common;

public struct Maybe<T>
{
    private readonly T? _value;
    public bool HasValue { get; }
    public bool HasNoValue => !HasValue;

    private Maybe(T value)
    {
        _value = value;
        HasValue = value != null;
    }

    public T Value
    {
        get
        {
            if (!HasValue)
                throw new InvalidOperationException("Maybe has no value.");
            return _value!;
        }
    }

    public static Maybe<T> None => new();
    public static Maybe<T> From(T? value) => value == null ? None : new Maybe<T>(value);

    public static implicit operator Maybe<T>(T? value) => From(value);

    public TResult Match<TResult>(Func<T, TResult> some, Func<TResult> none)
    {
        return HasValue ? some(_value!) : none();
    }

    public void Match(Action<T> some, Action none)
    {
        if (HasValue)
            some(_value!);
        else
            none();
    }
}
