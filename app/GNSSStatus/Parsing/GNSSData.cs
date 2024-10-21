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
    private readonly List<int> _satellitesInUseCache = new();
    private readonly List<float> _pDopCache = new();
    private readonly List<float> _hDopCache = new();
    private readonly List<float> _vDopCache = new();
    private readonly List<float> _latitudeErrorCache = new();
    private readonly List<float> _longitudeErrorCache = new();
    private readonly List<float> _altitudeErrorCache = new();
    private readonly List<double> _baseDistanceCache = new();

    private string? _latestUtcTime;
    private int _latestReferenceStationID;
    private float _latestDifferentialDataAge;
    private double _latestIonoPercentage;


    public void SetGGA(GGAData data)
    {
        _deltaXCache.Add(data.DeltaX);
        _deltaYCache.Add(data.DeltaY);
        _deltaZCache.Add(data.DeltaZ);
        _roverXCache.Add(data.RoverX);
        _roverYCache.Add(data.RoverY);
        _roverZCache.Add(data.RoverZ);
        _fixTypesCache.Add(data.Quality);
        _satellitesInUseCache.Add(data.TotalSatellitesInUse);

        _latestUtcTime = data.UtcTime;
        _latestReferenceStationID = data.DifferentialReferenceStationID;
        _latestDifferentialDataAge = data.AgeOfDifferentialData;
    }


    public void SetGSA(GSAData data)
    {
        _pDopCache.Add(data.PDop);
        _hDopCache.Add(data.HDop);
        _vDopCache.Add(data.VDop);
    }


    public void SetGST(GSTData data)
    {
        _latitudeErrorCache.Add(data.LatitudeError);
        _longitudeErrorCache.Add(data.LongitudeError);
        _altitudeErrorCache.Add(data.AltitudeError);
    }


    public void SetNTR(NTRData data)
    {
        _baseDistanceCache.Add(data.DistanceBetweenBaseAndRover);
    }


    public void SetIonoPercentage(double value)
    {
        _latestIonoPercentage = value;
    }


    public string GetPayloadJson()
    {
        JsonPayloadBuilder builder = new();

        // Median properties
        double deltaX = _deltaXCache.Count > 0 ? _deltaXCache.Median() : 0;
        _deltaXCache.Clear();

        double deltaY = _deltaYCache.Count > 0 ? _deltaYCache.Median() : 0;
        _deltaYCache.Clear();

        double deltaZAverage = _deltaZCache.Count > 0 ? _deltaZCache.Median() : 0;
        _deltaZCache.Clear();

        double roverXAverage = _roverXCache.Count > 0 ? _roverXCache.Median() : 0;
        _roverXCache.Clear();

        double roverYAverage = _roverYCache.Count > 0 ? _roverYCache.Median() : 0;
        _roverYCache.Clear();

        double roverZAverage = _roverZCache.Count > 0 ? _roverZCache.Median() : 0;
        _roverZCache.Clear();

        int satellitesInUse = _satellitesInUseCache.Count > 0 ? _satellitesInUseCache.Median() : 0;
        _satellitesInUseCache.Clear();

        float pDop = _pDopCache.Count > 0 ? _pDopCache.Median() : 0;
        _pDopCache.Clear();

        float hDop = _hDopCache.Count > 0 ? _hDopCache.Median() : 0;
        _hDopCache.Clear();

        float vDop = _vDopCache.Count > 0 ? _vDopCache.Median() : 0;
        _vDopCache.Clear();

        float latitudeError = _latitudeErrorCache.Count > 0 ? _latitudeErrorCache.Median() : 0;
        _latitudeErrorCache.Clear();

        float longitudeError = _longitudeErrorCache.Count > 0 ? _longitudeErrorCache.Median() : 0;
        _longitudeErrorCache.Clear();

        float altitudeError = _altitudeErrorCache.Count > 0 ? _altitudeErrorCache.Median() : 0;
        _altitudeErrorCache.Clear();

        double baseDistance = _baseDistanceCache.Count > 0 ? _baseDistanceCache.Median() : 0;
        _baseDistanceCache.Clear();

        // Static properties
        string roverIdentifier = ConfigManager.CurrentConfiguration.RoverIdentifier;

        // Latest properties
        string? utcTime = _latestUtcTime;
        int referenceStationID = _latestReferenceStationID;
        float differentialDataAge = _latestDifferentialDataAge;
        double ionoPercentage = _latestIonoPercentage;

        // Calculated properties
        double deltaXY = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

        GGAData.FixType worstFixType = GetWorstFixType();
        _fixTypesCache.Clear();

        // Manually serialize relevant properties.
        builder.AddPayload(
            new
            {
                TimeUtc = utcTime,
                FixType = worstFixType,
                SatellitesInUse = satellitesInUse,
                RoverX = roverXAverage,
                RoverY = roverYAverage,
                RoverZ = roverZAverage
            });

        builder.AddPayload(
            new
            {
                DeltaXY = deltaXY,
                DeltaZ = deltaZAverage,
                PDop = pDop,
                HDop = hDop,
                VDop = vDop
            });

        builder.AddPayload(
            new
            {
                RoverId = roverIdentifier,
                ErrorLatitude = latitudeError,
                ErrorLongitude = longitudeError,
                ErrorAltitude = altitudeError
            });

        builder.AddPayload(
            new
            {
                DifferentialDataAge = differentialDataAge,
                ReferenceStationId = referenceStationID,
                BaseRoverDistance = baseDistance,
                IonoPercentage = ionoPercentage
            });

        return builder.Build(true);
    }


    private GGAData.FixType GetWorstFixType()
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