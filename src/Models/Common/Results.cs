namespace Library.Models.Common;

public sealed record Results(bool IsSuccess, string? Error)
{
    public static Results Ok()
    {
        return new Results(true, null);
    }

    public static Results Fail(string error)
    {
        return new Results(false, error);
    }
}

public sealed record Results<T>(bool IsSuccess, T? Value, string? Error)
{
    public static Results<T> Ok(T value)
    {
        return new Results<T>(true, value, null);
    }

    public static Results<T> Fail(string error)
    {
        return new Results<T>(false, default, error);
    }
}