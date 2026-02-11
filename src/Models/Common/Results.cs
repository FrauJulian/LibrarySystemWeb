namespace Library.Domain.Common;

public sealed record Results(bool IsSuccess, string? Error)
{
    public static Results Ok() => new(true, null);
    public static Results Fail(string error) => new(false, error);
}

public sealed record Results<T>(bool IsSuccess, T? Value, string? Error)
{
    public static Results<T> Ok(T value) => new(true, value, null);
    public static Results<T> Fail(string error) => new(false, default, error);
}
