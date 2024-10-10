using GNSSStatus.Configuration;
using GNSSStatus.Coordinates;
using GNSSStatus.Networking;
using GNSSStatus.Utils;

namespace GNSSStatus.MathCalc;

public class Calculators
{
    
    public static double deltaXYCalc(double x1, double y1, double x2, double y2)
    {
        return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
    }

    public static double deltaZCalc(double z1, double z2)
    {
        return z2 - z1;
    }
}