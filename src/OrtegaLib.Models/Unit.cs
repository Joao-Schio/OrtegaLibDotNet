namespace OrtegaLib.Models;

public record struct Unit
{
    public static Unit Value => default;
    public override readonly string ToString() => "()";
}