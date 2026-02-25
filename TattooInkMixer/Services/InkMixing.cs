using System.Globalization;

namespace TattooInkMixer.Services;

public readonly record struct Rgb(byte R, byte G, byte B)
{
    public static Rgb FromHex(string hex)
    {
        hex = (hex ?? "").Trim().TrimStart('#');

        if (hex.Length == 3)
            hex = string.Concat(hex.Select(ch => $"{ch}{ch}"));

        if (hex.Length != 6)
            throw new FormatException("Hex must be #RRGGBB or #RGB.");

        byte r = byte.Parse(hex[..2], NumberStyles.HexNumber);
        byte g = byte.Parse(hex[2..4], NumberStyles.HexNumber);
        byte b = byte.Parse(hex[4..6], NumberStyles.HexNumber);
        return new Rgb(r, g, b);
    }

    public string ToHex() => $"#{R:X2}{G:X2}{B:X2}";
}

public readonly record struct Vec3(double X, double Y, double Z)
{
    public static Vec3 operator +(Vec3 a, Vec3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vec3 operator -(Vec3 a, Vec3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vec3 operator *(double k, Vec3 v) => new(k * v.X, k * v.Y, k * v.Z);
}

public static class Srgb
{
    public static double ToLinear01(byte srgb)
    {
        double c = srgb / 255.0;
        return (c <= 0.04045) ? (c / 12.92) : Math.Pow((c + 0.055) / 1.055, 2.4);
    }

    public static byte ToSrgbByte(double linear01)
    {
        linear01 = Math.Clamp(linear01, 0.0, 1.0);

        double c = (linear01 <= 0.0031308)
            ? 12.92 * linear01
            : 1.055 * Math.Pow(linear01, 1.0 / 2.4) - 0.055;

        int v = (int)Math.Round(255.0 * Math.Clamp(c, 0.0, 1.0));
        return (byte)Math.Clamp(v, 0, 255);
    }

    public static Vec3 ToLinear(Rgb c) => new(ToLinear01(c.R), ToLinear01(c.G), ToLinear01(c.B));
    public static Rgb FromLinear(Vec3 v) => new(ToSrgbByte(v.X), ToSrgbByte(v.Y), ToSrgbByte(v.Z));
}

public static class InkModel
{
    private const double Eps = 1e-8;

    // Treat linear RGB as reflectance proxy R in [0..1]
    public static Vec3 ReflectanceFromHex(string hex)
    {
        var rgb = Rgb.FromHex(hex);
        var lin = Srgb.ToLinear(rgb);
        return new Vec3(
            Math.Clamp(lin.X, Eps, 1.0),
            Math.Clamp(lin.Y, Eps, 1.0),
            Math.Clamp(lin.Z, Eps, 1.0)
        );
    }

    // Optical density D = -ln(R)
    public static Vec3 ReflectanceToDensity(Vec3 R)
        => new(-Math.Log(Math.Max(R.X, Eps)),
               -Math.Log(Math.Max(R.Y, Eps)),
               -Math.Log(Math.Max(R.Z, Eps)));

    // R = exp(-D)
    public static Vec3 DensityToReflectance(Vec3 D)
        => new(Math.Exp(-D.X), Math.Exp(-D.Y), Math.Exp(-D.Z));

    // Mix densities: Dmix = sum(wi * Di), then Rmix = exp(-Dmix)
    public static Vec3 MixReflectanceFromDensities(IReadOnlyList<Vec3> densities, double[] w)
    {
        Vec3 dMix = new(0, 0, 0);
        for (int i = 0; i < w.Length; i++)
            dMix = dMix + (w[i] * densities[i]);

        return DensityToReflectance(dMix);
    }
}

public static class Simplex
{
    // Project v onto the probability simplex { w>=0, sum(w)=1 }
    public static void ProjectToSimplexInPlace(double[] v)
    {
        int n = v.Length;

        var u = (double[])v.Clone();
        Array.Sort(u);
        Array.Reverse(u);

        double cssv = 0.0;
        int rho = -1;

        for (int i = 0; i < n; i++)
        {
            cssv += u[i];
            double t = (cssv - 1.0) / (i + 1);
            if (u[i] - t > 0) rho = i;
        }

        double sum = 0.0;
        for (int i = 0; i <= rho; i++) sum += u[i];
        double theta = (sum - 1.0) / (rho + 1);

        for (int i = 0; i < n; i++)
            v[i] = Math.Max(v[i] - theta, 0.0);
    }
}

public sealed class InkMixResult
{
    public required double[] Weights { get; init; }          // sum=1
    public required string PredictedHex { get; init; }        // predicted mix display
    public required double ErrorRmse { get; init; }           // RMSE in reflectance proxy space
}

public static class InkMixSolver
{
    // Loss in reflectance proxy space (linear RGB)
    private static double Loss(Vec3 predR, Vec3 targetR)
    {
        var d = predR - targetR;
        return d.X * d.X + d.Y * d.Y + d.Z * d.Z;
    }

    public static InkMixResult Solve(IReadOnlyList<string> inkHexes, string targetHex, int iterations = 450)
    {
        if (inkHexes.Count == 0) throw new ArgumentException("No inks provided.");

        // Convert base inks to densities
        var densities = inkHexes
            .Select(h => InkModel.ReflectanceToDensity(InkModel.ReflectanceFromHex(h)))
            .ToList();

        var targetR = InkModel.ReflectanceFromHex(targetHex);

        int n = densities.Count;
        var w = Enumerable.Repeat(1.0 / n, n).ToArray();

        double Eval(double[] x)
        {
            var predR = InkModel.MixReflectanceFromDensities(densities, x);
            return Loss(predR, targetR);
        }

        // finite-difference gradient + projection to simplex
        var g = new double[n];
        for (int it = 0; it < iterations; it++)
        {
            double fx = Eval(w);
            double eps = 1e-4;

            // gradient
            for (int i = 0; i < n; i++)
            {
                double old = w[i];

                w[i] = old + eps;
                Simplex.ProjectToSimplexInPlace(w);
                double fp = Eval(w);

                w[i] = old;
                Simplex.ProjectToSimplexInPlace(w);

                g[i] = (fp - fx) / eps;
            }

            double step = 0.18 * Math.Pow(0.985, it);
            for (int i = 0; i < n; i++)
                w[i] -= step * g[i];

            Simplex.ProjectToSimplexInPlace(w);
        }

        var finalR = InkModel.MixReflectanceFromDensities(densities, w);
        var predicted = Srgb.FromLinear(finalR).ToHex();

        var rmse = Math.Sqrt(Loss(finalR, targetR) / 3.0);

        return new InkMixResult
        {
            Weights = w,
            PredictedHex = predicted,
            ErrorRmse = rmse
        };
    }

    // Greedy “use at most K inks” wrapper
    public static InkMixResult SolveWithLimit(IReadOnlyList<string> inkHexes, string targetHex, int maxInks)
    {
        if (maxInks <= 0) throw new ArgumentOutOfRangeException(nameof(maxInks));
        maxInks = Math.Min(maxInks, inkHexes.Count);

        var chosen = new List<int>();
        var remaining = new HashSet<int>(Enumerable.Range(0, inkHexes.Count));

        InkMixResult? bestSoFar = null;

        for (int k = 0; k < maxInks; k++)
        {
            double bestErr = double.PositiveInfinity;
            int bestIdx = -1;
            InkMixResult? bestSol = null;

            foreach (var idx in remaining)
            {
                var subsetIdxs = chosen.Concat(new[] { idx }).ToArray();
                var subsetHexes = subsetIdxs.Select(i => inkHexes[i]).ToList();

                var sol = Solve(subsetHexes, targetHex);
                if (sol.ErrorRmse < bestErr)
                {
                    bestErr = sol.ErrorRmse;
                    bestIdx = idx;
                    bestSol = sol;
                }
            }

            chosen.Add(bestIdx);
            remaining.Remove(bestIdx);
            bestSoFar = bestSol;
        }

        // Expand weights back to full set
        var fullW = new double[inkHexes.Count];
        for (int i = 0; i < chosen.Count; i++)
            fullW[chosen[i]] = bestSoFar!.Weights[i];

        return new InkMixResult
        {
            Weights = fullW,
            PredictedHex = bestSoFar!.PredictedHex,
            ErrorRmse = bestSoFar!.ErrorRmse
        };
    }
}