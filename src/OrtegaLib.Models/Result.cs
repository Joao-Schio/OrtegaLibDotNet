namespace OrtegaLib.Models;

public sealed class Result<T, E>
    where T : notnull
    where E : notnull
{
    private readonly T? _value;
    private readonly E? _error;

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public T Value
    {
        get
        {
            if (IsFailure)
            {
                throw new InvalidOperationException(
                    "The value of a failed result cannot be accessed.");
            }

            return _value!;
        }
    }

    public E Error
    {
        get
        {
            if (IsSuccess)
            {
                throw new InvalidOperationException(
                    "The error of a successful result cannot be accessed.");
            }

            return _error!;
        }
    }

    private Result(
        bool isSuccess,
        T? value,
        E? error)
    {
        IsSuccess = isSuccess;
        _value = value;
        _error = error;
    }

    public static Result<T, E> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new Result<T, E>(
            isSuccess: true,
            value: value,
            error: default);
    }

    public static Result<T, E> Failure(E error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new Result<T, E>(
            isSuccess: false,
            value: default,
            error: error);
    }
}