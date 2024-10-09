namespace GNSSStatus.Coordinates;


public readonly struct GKCoordinate
{
    public readonly double N;
    public readonly double E;
    public readonly double Z;


    public GKCoordinate(double n, double e, double z)
    {
        N = n;
        E = e;
        Z = z;
    }


    public override string ToString()
    {
        return $"N: {N}, E: {E}, Z: {Z}";
    }
}