using System.Diagnostics;

namespace OrtegaLib.Diagnostics;


public static class DebugFunctions
{
    [Conditional("DEBUG")]
    public static void Print(object? value)
    {
        Console.WriteLine(value?.ToString() ?? "Null");
    }

    [Conditional("DEBUG")]
    public static void PrintEnumerable<T>(IEnumerable<T> values)
    {
        var vals = values.ToArray();
        Console.WriteLine($"Length = {vals.Length}");
        foreach (T value in values)
        {
            Console.WriteLine(value?.ToString() ?? "Null");
        }
    }
}