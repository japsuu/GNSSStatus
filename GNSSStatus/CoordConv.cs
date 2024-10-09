namespace GNSSStatus;


public readonly struct ConvertedCoordinate
{
    public readonly double N;
    public readonly double E;
    public readonly double Z;


    public ConvertedCoordinate(double n, double e, double z)
    {
        N = n;
        E = e;
        Z = z;
    }
}