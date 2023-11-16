namespace ColorThief.ImageSharp.Shared;

/// <summary>
///     Color map
/// </summary>
public class ColorMap
{
    private readonly List<VBox> _vBoxes = [];
    private List<QuantizedColor>? _palette;

    public void Push(VBox box)
    {
        _palette = null;
        _vBoxes.Add(box);
    }

    public List<QuantizedColor> GeneratePalette()
    {
        return _palette ??= (from vBox in _vBoxes
            let rgb = vBox.Avg(false)
            let color = FromRgb(rgb[0], rgb[1], rgb[2])
            select new QuantizedColor(color, vBox.Count(false))).ToList();
    }

    public int Size()
    {
        return _vBoxes.Count;
    }

    public int[]? Map(int[] color)
    {
        foreach (var vbox in _vBoxes.Where(vbox => vbox.Contains(color))) return vbox.Avg(false);
        return Nearest(color);
    }

    public int[]? Nearest(int[] color)
    {
        var d1 = double.MaxValue;
        int[]? pColor = null;

        foreach (var t in _vBoxes)
        {
            var vbColor = t.Avg(false);
            var d2 = Math.Sqrt(Math.Pow(color[0] - vbColor[0], 2)
                               + Math.Pow(color[1] - vbColor[1], 2)
                               + Math.Pow(color[2] - vbColor[2], 2));
            if (d2 < d1)
            {
                d1 = d2;
                pColor = vbColor;
            }
        }

        return pColor;
    }

    public VBox? FindColor(double targetLuma, double minLuma, double maxLuma, double targetSaturation,
        double minSaturation, double maxSaturation)
    {
        VBox? max = null;
        double maxValue = 0;
        var highestPopulation = _vBoxes.Select(p => p.Count(false)).Max();

        foreach (var swatch in _vBoxes)
        {
            var avg = swatch.Avg(false);
            var hsl = FromRgb(avg[0], avg[1], avg[2]).ToHsl();
            var sat = hsl.S;
            var luma = hsl.L;

            if (sat >= minSaturation && sat <= maxSaturation &&
                luma >= minLuma && luma <= maxLuma)
            {
                var thisValue = Mmcq.CreateComparisonValue(sat, targetSaturation, luma, targetLuma,
                    swatch.Count(false), highestPopulation);

                if (max == null || thisValue > maxValue)
                {
                    max = swatch;
                    maxValue = thisValue;
                }
            }
        }

        return max;
    }

    public static Color FromRgb(int red, int green, int blue)
    {
        var color = new Color
        {
            A = 255,
            R = (byte)red,
            G = (byte)green,
            B = (byte)blue
        };

        return color;
    }
}