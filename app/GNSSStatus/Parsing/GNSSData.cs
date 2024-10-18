using System.Reflection;
using System.Text;
using GNSSStatus.Configuration;
using GNSSStatus.Utils;

namespace GNSSStatus.Parsing;

public class GNSSData
{
    public readonly List<double> DeltaXCache = new();
    public readonly List<double> DeltaYCache = new();
    public readonly List<double> DeltaZCache = new();
    public readonly List<double> RoverXCache = new();
    public readonly List<double> RoverYCache = new();
    public readonly List<double> RoverZCache = new();
    public readonly List<GGAData.FixType> FixTypesCache = new();
    public double IonoPercentage { get; set; }
    
    public GGAData GGA { get; set; }
    public GSAData GSA { get; set; }
    public GSTData GST { get; set; }
    public GSVData GSV { get; set; }
    public NTRData NTR { get; set; }


    public string GetPayloadJson()
    {
        JsonPayloadBuilder builder = new();
        
        // Calculate averages and clear caches.
        double deltaXAverage = DeltaXCache.Count > 0 ? DeltaXCache.Average() : 0;
        double deltaYAverage = DeltaYCache.Count > 0 ? DeltaYCache.Average() : 0;
        double deltaZAverage = DeltaZCache.Count > 0 ? DeltaZCache.Average() : 0;
        double deltaXYAverage = Math.Sqrt(deltaXAverage * deltaXAverage + deltaYAverage * deltaYAverage);
        double roverXAverage = RoverXCache.Count > 0 ? RoverXCache.Average() : 0;
        double roverYAverage = RoverYCache.Count > 0 ? RoverYCache.Average() : 0;
        double roverZAverage = RoverZCache.Count > 0 ? RoverZCache.Average() : 0;
        DeltaXCache.Clear();
        DeltaYCache.Clear();
        DeltaZCache.Clear();
        RoverXCache.Clear();
        RoverYCache.Clear();
        RoverZCache.Clear();

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
        
        // Manually serialize relevant properties.
        builder.AddPayload(new
        {
            TimeUtc = GGA.UtcTime,
            FixType = worstFixType,
            SatellitesInUse = GGA.TotalSatellitesInUse,
            RoverX = roverXAverage,
            RoverY = roverYAverage,
            RoverZ = roverZAverage,
        });
        
        builder.AddPayload(new
        {
            DeltaXY = deltaXYAverage,
            DeltaZ = deltaZAverage,
            PDop = GSA.PDOP,
            HDop = GSA.HDOP,
            VDop = GSA.VDOP
        });
        
        builder.AddPayload(new
        {
            RoverId = ConfigManager.CurrentConfiguration.RoverIdentifier,
            ErrorLatitude = GST.LatitudeError,
            ErrorLongitude = GST.LongitudeError,
            ErrorAltitude = GST.AltitudeError
        });
        
        builder.AddPayload(new
        {
            DifferentialDataAge = GGA.AgeOfDifferentialData,
            ReferenceStationId = GGA.DifferentialReferenceStationID,
            BaseRoverDistance = NTR.DistanceBetweenBaseAndRover,
            IonoPercentage = IonoPercentage
        });
        
        return builder.Build(true);
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