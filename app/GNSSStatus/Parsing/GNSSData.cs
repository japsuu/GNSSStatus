using System.Reflection;
using System.Text;
using GNSSStatus.Configuration;
using GNSSStatus.Utils;
using YamlDotNet.Serialization;

namespace GNSSStatus.Parsing;

public class GNSSData
{
    public readonly List<double> DeltaZCache = new();
    public readonly List<double> DeltaXCache = new();
    public readonly List<double> DeltaYCache = new();
    public readonly List<double> RoverXCache = new();
    public readonly List<double> RoverYCache = new();
    public readonly List<double> RoverZCache = new();
    public readonly List<string> RoverUtcTimeCache = new();
    public readonly List<GGAData.FixType> RoverFixTypeCache = new();
    public readonly List<int> RoverSatInUseCache = new();
    public readonly List<float> RoverPDopCache = new();
    public readonly List<float> RoverVDopCache = new();
    public readonly List<float> RoverHDopCache = new();
    public readonly List<float> RoverErrorLatitudeCache = new();
    public readonly List<float> RoverErrorLongitudeCache = new();
    public readonly List<float> RoverErrorAltitudeCache = new();
    public readonly List<double> RoverBaselineCache = new();
    
    //public readonly List<string> RoverFixCache = new();
    //public readonly List<string> RoverSatInUseCache = new();
    //public readonly List<GGAData.FixType> FixTypesCache = new();
    public double IonoPercentage { get; set; }
    
    public GGAData GGA { get; set; }
    public GSAData GSA { get; set; }
    public GSTData GST { get; set; }
    public GSVData GSV { get; set; }
    public NTRData NTR { get; set; }


    public string GetPayloadJson()
    {
        JsonPayloadBuilder builder = new();
        /*
        // Calculate averages and clear caches.
        double deltaXAverage = DeltaXCache.Count > 0 ? DeltaXCache.Average() : 0;
        double deltaYAverage = DeltaYCache.Count > 0 ? DeltaYCache.Average() : 0;
        double deltaZAverage = DeltaZCache.Count > 0 ? DeltaZCache.Average() : 0;
        double deltaXYAverage = Math.Sqrt(deltaXAverage * deltaXAverage + deltaYAverage * deltaYAverage);
        double roverXAverage = RoverXCache.Count > 0 ? RoverXCache.Average() : 0;
        double roverYAverage = RoverYCache.Count > 0 ? RoverYCache.Average() : 0;
        double roverZAverage = RoverZCache.Count > 0 ? RoverZCache.Average() : 0;
        */
        
        // Calculate median and clear caches.
        int medianIndex = CalculateMedianIndex(RoverZCache).Item1;
        int medianPdop = CalculateMedianIndex(RoverPDopCache).Item1;
        int medianGstAltitudeError = CalculateMedianIndex(RoverErrorAltitudeCache).Item1;
        int medianBaseline = CalculateMedianIndex(RoverBaselineCache).Item1;
        double roverXMedian = RoverXCache[medianIndex];
        double roverYMedian = RoverYCache[medianIndex];
        double roverZMedian = RoverZCache[medianIndex];
        double deltaXMedian = DeltaXCache[medianIndex];
        double deltaYMedian = DeltaYCache[medianIndex];
        double deltaXy = Math.Sqrt(deltaXMedian * deltaXMedian + deltaYMedian * deltaYMedian);
        string roverUtCTime = RoverUtcTimeCache[medianIndex];
        GGAData.FixType roverFixType = RoverFixTypeCache[medianIndex];
        int roverSatInUse = RoverSatInUseCache[medianIndex];
        double deltaZ = DeltaZCache[medianIndex];
        float pDop = RoverPDopCache[medianPdop];
        float hDop = RoverHDopCache[medianPdop];
        float vDop = RoverVDopCache[medianPdop];
        float errLat = RoverErrorLatitudeCache[medianGstAltitudeError];
        float errLon = RoverErrorLongitudeCache[medianGstAltitudeError];
        float errAlt = RoverErrorAltitudeCache[medianGstAltitudeError];
        double baseline = RoverBaselineCache[medianBaseline];
        
        DeltaXCache.Clear();
        DeltaYCache.Clear();
        DeltaZCache.Clear();
        RoverXCache.Clear();
        RoverYCache.Clear();
        RoverZCache.Clear();
        RoverUtcTimeCache.Clear();
        RoverFixTypeCache.Clear();
        RoverSatInUseCache.Clear();
        RoverPDopCache.Clear();
        RoverHDopCache.Clear();
        RoverVDopCache.Clear();
        RoverErrorAltitudeCache.Clear();
        RoverErrorLatitudeCache.Clear();
        RoverErrorLongitudeCache.Clear();
        RoverBaselineCache.Clear();

        /*
        // Determine the worst fix type.
        GGAData.FixType worstFixType;
        if (FixTypesCache.Count > 0)
        {
            if (FixTypesCache.Contains(GGAData.FixType.NoFix))
                worstFixType = GGAData.FixType.NoFix;
            else if (FixTypesCache.Contains(GGAData.FixType.RTKFloat))
                worstFixType = GGAData.FixType.RTKFloat;
            else
                worstFixType = GGAData.FixType.RTKFixed;
            
        }
        else
            worstFixType = GGAData.FixType.NoFix;
        FixTypesCache.Clear();
        */
        
        // Manually serialize relevant properties.
        builder.AddPayload(new
        {
            TimeUtc = roverUtCTime,
            //FixType = worstFixType,
            FixType = roverFixType,
            //SatellitesInUse = GGA.TotalSatellitesInUse,
            SatellitesInUse = roverSatInUse,
            //RoverX = roverXAverage,
            RoverX = roverXMedian,
            //RoverY = roverYAverage,
            RoverY = roverYMedian,
            //RoverZ = roverZAverage,
            RoverZ = roverZMedian,
        });
        
        builder.AddPayload(new
        {
            //DeltaXY = deltaXYAverage,
            DeltaXY = deltaXy,
            //DeltaZ = deltaZAverage,
            DeltaZ = deltaZ,
            //PDop = GSA.PDop,
            PDop = pDop,
            //HDop = GSA.HDop,
            HDop = hDop,
            //VDop = GSA.VDop
            VDop = vDop,
        });
        
        builder.AddPayload(new
        {
            RoverId = ConfigManager.CurrentConfiguration.RoverIdentifier,
            //ErrorLatitude = GST.LatitudeError,
            ErrorLatitude = errLat,
            //ErrorLongitude = GST.LongitudeError,
            ErrorLongitude = errLon,
            //ErrorAltitude = GST.AltitudeError
            ErrorAltitude = errAlt
        });
        
        builder.AddPayload(new
        {
            DifferentialDataAge = GGA.AgeOfDifferentialData,
            ReferenceStationId = GGA.DifferentialReferenceStationID,
            //BaseRoverDistance = NTR.DistanceBetweenBaseAndRover,
            BaseRoverDistance = baseline,
            IonoPercentage = IonoPercentage
        });
        
        return builder.Build(true);
    }
    
    public (int, int) CalculateMedianIndex<T>(List<T> list)
    {
        // Luo kopio alkuperäisestä listasta ja liitä siihen alkuperäiset indeksit
        var indexedNumbers = list
            .Select((value, index) => new { Value = value, Index = index })
            .OrderBy(x => x.Value)
            .ToArray();

        int count = indexedNumbers.Length;

        if (count % 2 == 0)
        {
            // Jos määrä on parillinen, laske kahden keskimmäisen luvun indeksit
            int mid1Index = indexedNumbers[count / 2 - 1].Index;
            int mid2Index = indexedNumbers[count / 2].Index;
            return (mid1Index, mid2Index);
        }
        else
        {
            // Jos määrä on pariton, valitse keskimmäisen luvun indeksi
            int medianIndex = indexedNumbers[count / 2].Index;
            return (medianIndex, -1); // -1 tarkoittaa, että vain yksi mediaani löytyy
        }
    }
    
    /// <summary>
    /// NOTE: Should only be used for debugging purposes.
    /// Allocates crazy amounts of memory.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        StringBuilder sb = new();
        
        sb.AppendLine("GNSS Data:");
        AppendPropertiesRecursive(sb, this, 1);
        
        return sb.ToString();
    }
    
    private static void AppendPropertiesRecursive(StringBuilder sb, object? obj, int depth = 0)
    {
        if (obj == null)
            return;
        
        if (depth > 10)
        {
            sb.Append(' ', depth * 2);
            sb.AppendLine("...");
            return;
        }
        
        PropertyInfo[] properties = obj.GetType().GetProperties();
        foreach (PropertyInfo property in properties)
        {
            object? value = property.GetValue(obj);
            if (value == null)
                continue;
            
            sb.Append(' ', depth * 2);
            sb.Append(property.Name);
            sb.Append(": ");
            
            // If array or collection, print count.
            if (value is System.Collections.ICollection collection)
            {
                sb.AppendLine($"Count: {collection.Count}");
                continue;
            }
            
            if (value is string || value.GetType().IsPrimitive)
                sb.AppendLine(value.ToString());
            else
            {
                sb.AppendLine();
                AppendPropertiesRecursive(sb, value, depth + 1);
            }
        }
        
        FieldInfo[] fields = obj.GetType().GetFields();
        foreach (FieldInfo field in fields)
        {
            object? value = field.GetValue(obj);
            if (value == null)
                continue;
            
            sb.Append(' ', depth * 2);
            sb.Append(field.Name);
            sb.Append(": ");
            
            // If array or collection, print count.
            if (value is System.Collections.ICollection collection)
            {
                sb.AppendLine($"Count: {collection.Count}");
                continue;
            }
            
            if (value is string || value.GetType().IsPrimitive)
                sb.AppendLine(value.ToString());
            else
            {
                sb.AppendLine();
                AppendPropertiesRecursive(sb, value, depth + 1);
            }
        }
    }
}