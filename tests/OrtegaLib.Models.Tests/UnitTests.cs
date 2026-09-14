namespace OrtegaLib.Models.Tests;

public class UnitTests
{
    [Fact]
    public void Value_ShouldEqualDefault()
    {
        Assert.Equal(default, Unit.Value);
    }

    [Fact]
    public void Instances_ShouldAlwaysBeEqual()
    {
        Unit first = new();
        Unit second = new();

        Assert.Equal(first, second);
    }

    [Fact]
    public void ToString_ShouldRepresentUnit()
    {
        Assert.Equal("()", Unit.Value.ToString());
    }

    [Fact]
    public void ResultSuccess_ShouldSupportUnit()
    {
        Result<Unit, string> result =
            Result<Unit, string>.Success(Unit.Value);

        Assert.True(result.IsSuccess);
        Assert.Equal(Unit.Value, result.Value);
    }
}