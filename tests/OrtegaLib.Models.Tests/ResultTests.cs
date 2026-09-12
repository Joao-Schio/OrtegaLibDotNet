namespace OrtegaLib.Models.Tests;

public class ResultTests
{
    [Fact]
    public void Success_ShouldContainValue()
    {
        Result<int, string> result =
            Result<int, string>.Success(42);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Failure_ShouldContainError()
    {
        Result<int, string> result =
            Result<int, string>.Failure("error");

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal("error", result.Error);
    }

    [Fact]
    public void Success_ErrorAccess_ShouldThrow()
    {
        Result<int, string> result =
            Result<int, string>.Success(42);

        Assert.Throws<InvalidOperationException>(
            () => result.Error);
    }

    [Fact]
    public void Failure_ValueAccess_ShouldThrow()
    {
        Result<int, string> result =
            Result<int, string>.Failure("error");

        Assert.Throws<InvalidOperationException>(
            () => result.Value);
    }

    [Fact]
    public void Success_NullValue_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(
            () => Result<string, string>.Success(null!));
    }

    [Fact]
    public void Failure_NullError_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(
            () => Result<string, string>.Failure(null!));
    }
}