using System.Globalization;

namespace GNSSStatus;

public static class CoordinateConverter
{
    private static readonly DemNode[,] NodeBuffer = new DemNode[586, 389];

    private readonly struct DemNode
    {
        public readonly double Lat;
        public readonly double Lon;
        public readonly double Height;


        public DemNode(double lat, double lon, double height)
        {
            Lat = lat;
            Lon = lon;
            Height = height;
        }
    }
    
    
    /// <summary>
    /// Funktion tarkoituksena ottaa sisään Decimal Degrees koordinaatit GGA viestistä ja muuntaa ne haluttuun GK järjestelmään.
    /// Funktio siis ottaa sisään N ja E koordinaatit.
    /// </summary>
    /// <param name="lat"></param>
    /// <param name="lon"></param>
    /// <param name="hemisphereLatitude"></param>
    /// <param name="hemisphereLongitude"></param>
    /// <param name="GK">GK järjestelmän numero.</param>
    /// <param name="altitude"></param>
    /// <returns>X,Y koordinaatit GK järjestelmässä.</returns>
    public static ConvertedCoordinate ConvertToGk(string lat, string lon, string hemisphereLatitude, string hemisphereLongitude, int GK, string altitude)
    {
        double leveysasteDd = NmeaToDecimal(lat, hemisphereLatitude);
        double pituusasteDd = NmeaToDecimal(lon, hemisphereLongitude);

        // 2013-04-29/JeH, loukko (at) loukko (dot) net
        // http://www.loukko.net/koord_proj/
        // Vapaasti käytettävissä ilman toimintatakuuta.

        // Vakiot
        const double f = 1 / 298.257222101; // Ellipsoidin litistyssuhde
        const double a = 6378137; // Isoakselin puolikas
        double lambdaNolla = Math.PI / 180 * GK;
        const int kNolla = 1; // Mittakaavakerroin
        double eNolla = 1000000 * GK + 500000;
        // Itäkoordinaatti

        // Kaavat

        // Muunnetaan astemuotoisesta radiaaneiksi
        double fii = leveysasteDd * Math.PI / 180;
        double lambda = pituusasteDd * Math.PI / 180;

        const double n = f / (2.0 - f);
        double a1 = a / (1.0 + n) * (1.0 + Math.Pow(n, 2) / 4 + Math.Pow(n, 4) / 64.0);
        double eToiseen = 2 * f - Math.Pow(f, 2);
        double h1Pilkku = 1.0 / 2.0 * n -
                          2.0 / 3.0 * Math.Pow(n, 2) +
                          5.0 / 16.0 * Math.Pow(n, 3) +
                          41.0 / 180.0 * Math.Pow(n, 4);
        double h2Pilkku = 13.0 / 48.0 * Math.Pow(n, 2) -
                           3.0 / 5.0 * Math.Pow(n, 3) +
                           557.0 / 1440.0 * Math.Pow(n, 4);
        double h3Pilkku = 61.0 / 240.0 * Math.Pow(n, 3) - 103.0 / 140.0 * Math.Pow(n, 4);
        double h4Pilkku = 49561.0 / 161280.0 * Math.Pow(n, 4);
        double qPilkku = Math.Asinh(Math.Tan(fii));
        double q2Pilkku = Math.Atanh(Math.Sqrt(eToiseen) * Math.Sin(fii));
        double q = qPilkku - Math.Sqrt(eToiseen) * q2Pilkku;
        double l = lambda - lambdaNolla;
        double beeta = Math.Atan(Math.Sinh(q));
        double eetaPilkku = Math.Atanh(Math.Cos(beeta) * Math.Sin(l));
        double zeetaPilkku = Math.Asin(Math.Sin(beeta) / (1.0 / Math.Cosh(eetaPilkku)));
        double zeeta1 = h1Pilkku * Math.Sin(2 * zeetaPilkku) * Math.Cosh(2 * eetaPilkku);
        double zeeta2 = h2Pilkku * Math.Sin(4 * zeetaPilkku) * Math.Cosh(4 * eetaPilkku);
        double zeeta3 = h3Pilkku * Math.Sin(6 * zeetaPilkku) * Math.Cosh(6 * eetaPilkku);
        double zeeta4 = h4Pilkku * Math.Sin(8 * zeetaPilkku) * Math.Cosh(8 * eetaPilkku);
        double eeta1 = h1Pilkku * Math.Cos(2 * zeetaPilkku) * Math.Sinh(2 * eetaPilkku);
        double eeta2 = h2Pilkku * Math.Cos(4 * zeetaPilkku) * Math.Sinh(4 * eetaPilkku);
        double eeta3 = h3Pilkku * Math.Cos(6 * zeetaPilkku) * Math.Sinh(6 * eetaPilkku);
        double eeta4 = h4Pilkku * Math.Cos(8 * zeetaPilkku) * Math.Sinh(8 * eetaPilkku);
        double zeeta = zeetaPilkku + zeeta1 + zeeta2 + zeeta3 + zeeta4;
        double eeta = eetaPilkku + eeta1 + eeta2 + eeta3 + eeta4;

        double z = Convert.ToDouble(altitude) - CreateDem(leveysasteDd, pituusasteDd);

        // Tulos tasokoordinaatteina
        return new ConvertedCoordinate(a1 * zeeta * kNolla, a1 * eeta * kNolla + eNolla, z);
    }


    public static double NmeaToDecimal(string ll, string hemisphere)
    {
        // ll on koordinaatti NMEA-muodossa (esim. 6304.21725318).
        // hemisph määrittelee, onko kyseessä pohjoinen/itäinen (1) vai eteläinen/läntinen (-1) pallonpuolisko.
        int hemisph;
        if (hemisphere == "N" || hemisphere == "E")
            hemisph = 1;
        else
            hemisph = -1;
        double coordinate = double.Parse(ll, CultureInfo.InvariantCulture);
        return Math.Round((Convert.ToInt32(coordinate / 100) + (coordinate - Convert.ToInt32(coordinate / 100) * 100) / 60) * hemisph, 10);
    }


    private static double CreateDem(double leveysasteDd, double pituusasteDd)
    {
        //double LeveysasteDD = NmeaToDecimal(lat, hemisphereLatitude);
        //double PituusasteDD = NmeaToDecimal(lon, hemisphereLongitude);

        int row = (int)((leveysasteDd - 59.0) / 0.02);
        int col = (int)((pituusasteDd - 17.48) / 0.04);

        double q12 = NodeBuffer[row + 1, col].Height;
        double q22 = NodeBuffer[row + 1, col + 1].Height;
        double q11 = NodeBuffer[row, col].Height;
        double q21 = NodeBuffer[row, col + 1].Height;

        double x = pituusasteDd;
        double x1 = NodeBuffer[row, col].Lon;
        double x2 = NodeBuffer[row, col + 1].Lon;

        double y = leveysasteDd;
        double y1 = NodeBuffer[row, col].Lat;
        double y2 = NodeBuffer[row + 1, col].Lat;

        double r1 = (x2 - x) / (x2 - x1) * q11 + (x - x1) / (x2 - x1) * q21;
        double r2 = (x2 - x) / (x2 - x1) * q12 + (x - x1) / (x2 - x1) * q22;
        double p = (y2 - y) / (y2 - y1) * r1 + (y - y1) / (y2 - y1) * r2;

        return p;
    }


    public static void create_dem()
    {
        int row = 586;
        int col = 1;
        string[] input = File.ReadAllText("FIN2005N00.lst").Split('\n');

        // 70.700000  17.480000  34.415
        // 70,00000000  10,04000000  41,5820
        // lat = Double.Parse(input[i].Substring(0,11));
        // lon = Double.Parse(input[i].Substring(13,11));
        // height = Double.Parse(input[i].Substring(26));
        // 60.280000  24.160000  19.184 tama on FIN2005N00.lst formaatti

        for (int i = 0; i < 227954; i++)
        {
            double lat = double.Parse(input[i].Substring(0, 9));
            double lon = double.Parse(input[i].Substring(11, 9));
            double height = double.Parse(input[i].Substring(22));
            DemNode node = new DemNode(lat, lon, height);
            NodeBuffer[row - 1, col - 1] = node;
            col++;

            if (col > 389)
            {
                col = 1;
                row--;
            }
        }
    }
}