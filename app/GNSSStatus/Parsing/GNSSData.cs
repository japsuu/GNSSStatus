using System.Reflection;
using System.Text;
using GNSSStatus.Configuration;
using GNSSStatus.Utils;

namespace GNSSStatus.Parsing;

public class GNSSData
{
    private readonly List<double> _deltaXCache = new();
    private readonly List<double> _deltaYCache = new();
    private readonly List<double> _deltaZCache = new();
    private readonly List<double> _roverXCache = new();
    private readonly List<double> _roverYCache = new();
    private readonly List<double> _roverZCache = new();
    private readonly List<GGAData.FixType> _fixTypesCache = new();
    
    private GGAData _gga;
    private GSAData _gsa;
    private GSTData _gst;
    private GSVData _gsv;
    private NTRData _ntr;
    
    public GGAData GGA
    {
        set
        {
            _deltaXCache.Add(value.DeltaX);
            _deltaYCache.Add(value.DeltaY);
            _deltaZCache.Add(value.DeltaZ);
            _roverXCache.Add(value.RoverX);
            _roverYCache.Add(value.RoverY);
            _roverZCache.Add(value.RoverZ);
            _fixTypesCache.Add(value.Quality);
            
            _gga = value;
        }
    }
    
    public GSAData GSA
    {
        set
        {
            
        }
    }
    
    public GSTData GST
    {
        set
        {
            
        }
    }
    
    public GSVData GSV
    {
        set
        {
            
        }
    }
    
    public NTRData NTR
    {
        set
        {
            
        }
    }
    
    public double IonoPercentage { get; set; }


    public string GetPayloadJson()
    {
        JsonPayloadBuilder builder = new();
        
        // Calculate medians and clear caches.
        double deltaXAverage = _deltaXCache.Count > 0 ? _deltaXCache.Average() : 0;
        double deltaYAverage = _deltaYCache.Count > 0 ? _deltaYCache.Average() : 0;
        double deltaZAverage = _deltaZCache.Count > 0 ? _deltaZCache.Average() : 0;
        double deltaXYAverage = Math.Sqrt(deltaXAverage * deltaXAverage + deltaYAverage * deltaYAverage);
        double roverXAverage = _roverXCache.Count > 0 ? _roverXCache.Average() : 0;
        double roverYAverage = _roverYCache.Count > 0 ? _roverYCache.Average() : 0;
        double roverZAverage = _roverZCache.Count > 0 ? _roverZCache.Average() : 0;
        
        _deltaXCache.Clear();
        _deltaYCache.Clear();
        _deltaZCache.Clear();
        _roverXCache.Clear();
        _roverYCache.Clear();
        _roverZCache.Clear();

        GGAData.FixType worstFixType = ReadWorstFixType();
        
        string roverIdentifier = ConfigManager.CurrentConfiguration.RoverIdentifier;
        string utcTime = _gga.UtcTime;
        int satellitesInUse = _gga.TotalSatellitesInUse;
        float pDop = _gsa.PDop;
        float hDop = _gsa.HDop;
        float vDop = _gsa.VDop;
        float latitudeError = _gst.LatitudeError;
        float longitudeError = _gst.LongitudeError;
        float altitudeError = _gst.AltitudeError;
        float differentialDataAge = _gga.AgeOfDifferentialData;
        int referenceStationID = _gga.DifferentialReferenceStationID;
        double baseDistance = _ntr.DistanceBetweenBaseAndRover;

        // Manually serialize relevant properties.
        builder.AddPayload(new
        {
            TimeUtc = utcTime,
            FixType = worstFixType,
            SatellitesInUse = satellitesInUse,
            RoverX = roverXAverage,
            RoverY = roverYAverage,
            RoverZ = roverZAverage,
        });

        builder.AddPayload(new
        {
            DeltaXY = deltaXYAverage,
            DeltaZ = deltaZAverage,
            PDop = pDop,
            HDop = hDop,
            VDop = vDop
        });

        builder.AddPayload(new
        {
            RoverId = roverIdentifier,
            ErrorLatitude = latitudeError,
            ErrorLongitude = longitudeError,
            ErrorAltitude = altitudeError
        });

        builder.AddPayload(new
        {
            DifferentialDataAge = differentialDataAge,
            ReferenceStationId = referenceStationID,
            BaseRoverDistance = baseDistance,
            IonoPercentage = IonoPercentage
        });
        
        return builder.Build(true);
    }


    private GGAData.FixType ReadWorstFixType()
    {
        // Determine the worst fix type.
        GGAData.FixType worstFixType;
        if (_fixTypesCache.Count > 0)
        {
            if (_fixTypesCache.Contains(GGAData.FixType.NoFix))
                worstFixType = GGAData.FixType.NoFix;
            else if (_fixTypesCache.Contains(GGAData.FixType.RTKFloat))
                worstFixType = GGAData.FixType.RTKFloat;
            else
                worstFixType = GGAData.FixType.RTKFixed;
        }
        else
            worstFixType = GGAData.FixType.NoFix;
        _fixTypesCache.Clear();
        
        return worstFixType;
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