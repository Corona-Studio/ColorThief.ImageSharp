namespace ColorThief.ImageSharp.Shared;

public class QuantizedColor(Color color, int population)
{
    public Color Color { get; } = color;
    public int Population { get; } = population;
    public bool IsDark { get; } = CalculateYiqLuma(color) < 128;

    public static int CalculateYiqLuma(Color color)
    {
        return Convert.ToInt32(Math.Round((299 * color.R + 587 * color.G + 114 * color.B) / 1000f));
    }
}