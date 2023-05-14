using ColorThief.ImageSharp.Shared;
using Color = ColorThief.ImageSharp.Shared.Color;

namespace ColorThief.ImageSharp;

public static class ColorThief
{
    private const int DefaultColorCount = 5;
    private const int DefaultQuality = 10;
    private const bool DefaultIgnoreWhite = true;
    private const int ColorDepth = 4;

    /// <summary>
    ///     Use the median cut algorithm to cluster similar colors.
    /// </summary>
    /// <param name="pixelArray">Pixel array.</param>
    /// <param name="colorCount">The color count.</param>
    /// <returns></returns>
    private static ColorMap? GetColorMap(byte[][] pixelArray, int colorCount)
    {
        // Send array to quantize function which clusters values using median
        // cut algorithm

        if (colorCount == 0) return null;

        var cmap = Mmcq.Quantize(pixelArray, colorCount);
        return cmap;
    }

    private static byte[][] ConvertPixels(IReadOnlyList<byte> pixels, int pixelCount, int quality, bool ignoreWhite)
    {
        var expectedDataLength = pixelCount * ColorDepth;
        if (expectedDataLength != pixels.Count)
            throw new ArgumentException("(expectedDataLength = "
                                        + expectedDataLength + ") != (pixels.length = "
                                        + pixels.Count + ")");

        // Store the RGB values in an array format suitable for quantize
        // function

        // numRegardedPixels must be rounded up to avoid an
        // ArrayIndexOutOfBoundsException if all pixels are good.

        var numRegardedPixels = (pixelCount + quality - 1) / quality;

        var numUsedPixels = 0;
        var pixelArray = new byte[numRegardedPixels][];

        for (var i = 0; i < pixelCount; i += quality)
        {
            var offset = i * ColorDepth;
            var b = pixels[offset];
            var g = pixels[offset + 1];
            var r = pixels[offset + 2];
            var a = pixels[offset + 3];

            // If pixel is mostly opaque and not white
            if (a >= 125 && !(ignoreWhite && r > 250 && g > 250 && b > 250))
            {
                pixelArray[numUsedPixels] = new[] { r, g, b };
                numUsedPixels++;
            }
        }

        // Remove unused pixels from the array
        var copy = new byte[numUsedPixels][];
        Array.Copy(pixelArray, copy, numUsedPixels);
        return copy;
    }

    /// <summary>
    ///     Use the median cut algorithm to cluster similar colors and return the base color from the largest cluster.
    /// </summary>
    /// <param name="sourceImage">The source image.</param>
    /// <param name="quality">
    ///     1 is the highest quality settings. 10 is the default. There is
    ///     a trade-off between quality and speed. The bigger the number,
    ///     the faster a color will be returned but the greater the
    ///     likelihood that it will not be the visually most dominant color.
    /// </param>
    /// <param name="ignoreWhite">if set to <c>true</c> [ignore white].</param>
    /// <returns></returns>
    public static QuantizedColor GetColor<T>(Image<T> sourceImage, int quality = DefaultQuality,
        bool ignoreWhite = DefaultIgnoreWhite) where T : unmanaged, IPixel<T>
    {
        var palette = GetPalette(sourceImage, 3, quality, ignoreWhite);

        var dominantColor = new QuantizedColor(new Color
        {
            A = Convert.ToByte(palette.Average(a => a.Color.A)),
            R = Convert.ToByte(palette.Average(a => a.Color.R)),
            G = Convert.ToByte(palette.Average(a => a.Color.G)),
            B = Convert.ToByte(palette.Average(a => a.Color.B))
        }, Convert.ToInt32(palette.Average(a => a.Population)));

        return dominantColor;
    }

    /// <summary>
    ///     Use the median cut algorithm to cluster similar colors.
    /// </summary>
    /// <param name="sourceImage">The source image.</param>
    /// <param name="colorCount">The color count.</param>
    /// <param name="quality">
    ///     1 is the highest quality settings. 10 is the default. There is
    ///     a trade-off between quality and speed. The bigger the number,
    ///     the faster a color will be returned but the greater the
    ///     likelihood that it will not be the visually most dominant color.
    /// </param>
    /// <param name="ignoreWhite">if set to <c>true</c> [ignore white].</param>
    /// <returns></returns>
    /// <code>true</code>
    public static List<QuantizedColor> GetPalette<T>(Image<T> sourceImage, int colorCount = DefaultColorCount,
        int quality = DefaultQuality, bool ignoreWhite = DefaultIgnoreWhite) where T : unmanaged, IPixel<T>
    {
        var pixelArray = GetPixelsFast(sourceImage, quality, ignoreWhite);
        var cmap = GetColorMap(pixelArray, colorCount);
        if (cmap != null)
        {
            var colors = cmap.GeneratePalette();
            return colors;
        }

        return new List<QuantizedColor>();
    }

    private static byte[][] GetPixelsFast<T>(Image<T> sourceImage, int quality, bool ignoreWhite)
        where T : unmanaged, IPixel<T>
    {
        if (quality < 1) quality = DefaultQuality;

        var pixels = GetIntFromPixel(sourceImage);
        var pixelCount = sourceImage.Width * sourceImage.Height;

        return ConvertPixels(pixels, pixelCount, quality, ignoreWhite);
    }

    private static byte[] GetIntFromPixel<T>(Image<T> bmp) where T : unmanaged, IPixel<T>
    {
        var pixelList = new byte[bmp.Width * bmp.Height * 4];
        var count = 0;

        for (var x = 0; x < bmp.Width; x++)
        for (var y = 0; y < bmp.Height; y++)
        {
            var clr = new Rgba32();
            bmp[x, y].ToRgba32(ref clr);

            pixelList[count] = clr.B;
            count++;

            pixelList[count] = clr.G;
            count++;

            pixelList[count] = clr.R;
            count++;

            pixelList[count] = clr.A;
            count++;
        }

        return pixelList;
    }
}