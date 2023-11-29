namespace ColorThief.ImageSharp.Shared;

/// <summary>
///     3D color space box.
/// </summary>
public class VBox(int r1, int r2, int g1, int g2, int b1, int b2, IReadOnlyList<int> histo)
{
    private int[]? _avg;
    private int? _count;
    private int? _volume;
    public int B1 = b1;
    public int B2 = b2;
    public int G1 = g1;
    public int G2 = g2;
    public int R1 = r1;
    public int R2 = r2;

    public int Volume(bool force)
    {
        if (_volume == null || force) _volume = (R2 - R1 + 1) * (G2 - G1 + 1) * (B2 - B1 + 1);

        return _volume.Value;
    }

    public int Count(bool force)
    {
        if (_count == null || force)
        {
            var npix = 0;
            int i;

            for (i = R1; i <= R2; i++)
            {
                int j;
                for (j = G1; j <= G2; j++)
                {
                    int k;
                    for (k = B1; k <= B2; k++)
                    {
                        var index = Mmcq.GetColorIndex(i, j, k);
                        npix += histo[index];
                    }
                }
            }

            _count = npix;
        }

        return _count.Value;
    }

    public VBox Clone()
    {
        return new VBox(R1, R2, G1, G2, B1, B2, histo);
    }

    public int[] Avg(bool force)
    {
        if (_avg == null || force)
        {
            var ntot = 0;

            var rsum = 0;
            var gsum = 0;
            var bsum = 0;

            int i;

            for (i = R1; i <= R2; i++)
            {
                int j;
                for (j = G1; j <= G2; j++)
                {
                    int k;
                    for (k = B1; k <= B2; k++)
                    {
                        var histoindex = Mmcq.GetColorIndex(i, j, k);
                        var hval = histo[histoindex];
                        ntot += hval;
                        rsum += Convert.ToInt32(hval * (i + 0.5) * Mmcq.Mult);
                        gsum += Convert.ToInt32(hval * (j + 0.5) * Mmcq.Mult);
                        bsum += Convert.ToInt32(hval * (k + 0.5) * Mmcq.Mult);
                    }
                }
            }

            if (ntot > 0)
                _avg =
                [
                    Math.Abs(rsum / ntot), Math.Abs(gsum / ntot),
                    Math.Abs(bsum / ntot)
                ];
            else
                _avg =
                [
                    Math.Abs(Mmcq.Mult * (R1 + R2 + 1) / 2),
                    Math.Abs(Mmcq.Mult * (G1 + G2 + 1) / 2),
                    Math.Abs(Mmcq.Mult * (B1 + B2 + 1) / 2)
                ];
        }

        return _avg;
    }

    public bool Contains(int[] pixel)
    {
        var rval = pixel[0] >> Mmcq.Rshift;
        var gval = pixel[1] >> Mmcq.Rshift;
        var bval = pixel[2] >> Mmcq.Rshift;

        return rval >= R1 && rval <= R2 && gval >= G1 && gval <= G2 && bval >= B1 && bval <= B2;
    }
}

internal class VBoxCountComparer : IComparer<VBox>
{
    public int Compare(VBox? x, VBox? y)
    {
        if (x == y) return 0;
        if (x == null || y == null) return -1;

        var a = x.Count(false);
        var b = y.Count(false);
        return a < b ? -1 : a > b ? 1 : 0;
    }
}

internal class VBoxComparer : IComparer<VBox>
{
    public int Compare(VBox? x, VBox? y)
    {
        if (x == y) return 0;
        if (x == null || y == null) return -1;

        var aCount = x.Count(false);
        var bCount = y.Count(false);
        var aVolume = x.Volume(false);
        var bVolume = y.Volume(false);

        // Otherwise sort by products
        var a = aCount * aVolume;
        var b = bCount * bVolume;
        return a < b ? -1 : a > b ? 1 : 0;
    }
}