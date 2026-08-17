using System.Text.Json.Serialization;

namespace LageFreeze.Models;

/// <summary>
/// Describes a rectangular area in physical screen pixels. Coordinates may be
/// negative when a monitor is located to the left of or above the primary one.
/// </summary>
public readonly record struct ScreenBounds
{
    [JsonConstructor]
    public ScreenBounds(int left, int top, int width, int height)
    {
        if (width < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width));
        }

        if (height < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height));
        }

        Left = left;
        Top = top;
        Width = width;
        Height = height;
    }

    public int Left { get; }

    public int Top { get; }

    public int Width { get; }

    public int Height { get; }

    [JsonIgnore]
    public int Right => checked(Left + Width);

    [JsonIgnore]
    public int Bottom => checked(Top + Height);

    [JsonIgnore]
    public bool IsEmpty => Width == 0 || Height == 0;

    public static ScreenBounds FromEdges(int left, int top, int right, int bottom)
    {
        return new ScreenBounds(left, top, checked(right - left), checked(bottom - top));
    }

    public bool Contains(int x, int y)
    {
        return x >= Left && x < Right && y >= Top && y < Bottom;
    }

    public override string ToString()
    {
        return $"{Width}x{Height} at ({Left},{Top})";
    }
}
