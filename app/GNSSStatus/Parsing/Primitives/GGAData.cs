using GNSSStatus.Configuration;
using GNSSStatus.Coordinates;
using GNSSStatus.Networking;

namespace GNSSStatus.Parsing;

public readonly struct GGAData
{
    public enum FixType : byte
    {
        NoFix = 0,
        RTKFixed = 1,
        RTKFloat = 2
    }
    
    public const int LENGTH = 14;

    public readonly GKCoordinate GKCoordinate;
    public readonly double RoverX;
    public readonly double RoverY;
    public readonly double DeltaX;
    public readonly double DeltaY;
    public readonly double RoverZ;
    public readonly double DeltaZ;
    public readonly string UtcTime;
    public readonly string Latitude;
    public readonly string DirectionLatitude;
    public readonly string Longitude;
    public readonly string DirectionLongitude;
    public readonly FixType Quality;
    public readonly string TotalSatellitesInUse;
    public readonly string HDOP;
    public readonly string Altitude;
    public readonly string AltitudeUnit;
    public readonly string GeoidSeparation;
    public readonly string GeoidSeparationUnit;
    public readonly string AgeOfDifferentialData;
    public readonly string DifferentialReferenceStationID;


    public GGAData(Nmea0183Sentence sentence)
    {
        // UTC time of position fix - hhmmss.ss(ss)
        string utcTime = sentence.Parts[1];

        // Latitude - ddmm.mmmmm(mm)
        string latitude = sentence.Parts[2];

        // Direction of latitude (N/S) - Character
        string directionLatitude = sentence.Parts[3];

        // Longitude - dddmm.mmmmm(mm)
        string longitude = sentence.Parts[4];

        // Direction of longitude (E/W) - Character
        string directionLongitude = sentence.Parts[5];

        // GPS Quality indicator (0-6) - Digit
        //   0: Fix not valid
        //   1: GPS fix
        //   2: Differential GPS fix (DGNSS), SBAS, OmniSTAR VBS, Beacon, RTX in GVBS mode
        //   3: Not applicable
        //   4: RTK Fixed, xFill
        //   5: RTK Float, OmniSTAR XP/HP, Location RTK, RTX
        //   6: INS Dead reckoning
        string quality = sentence.Parts[6];

        // Number of satellites in use. Range 00-12 in strict NMEA mode, 00-99 in high-precision NMEA mode - Digit
        string satellites = sentence.Parts[7];

        // Horizontal dilution of precision - Float
        string hdop = sentence.Parts[8];

        // Altitude above ellipsoid - Float
        string altitude = sentence.Parts[9];

        // Altitude unit - Character
        string altitudeUnit = sentence.Parts[10];

        // Geoid separation - Float
        string geoidSeparation = sentence.Parts[11];

        // Geoid separation unit - Character
        string geoidSeparationUnit = sentence.Parts[12];

        // Age of differential data - Float
        string ageOfDifferentialData = sentence.Parts[13];

        // Differential reference station ID - Integer
        string differentialReferenceStationID = sentence.Parts[14];
        // Prune the checksum from the end
        differentialReferenceStationID = differentialReferenceStationID[..differentialReferenceStationID.IndexOf('*')];
        
        // Modify the FixType so that anything other than RTKFixed or RTKFloat is considered NoFix.
        FixType fixType = quality switch
        {
            "4" => FixType.RTKFixed,
            "5" => FixType.RTKFloat,
            _ => FixType.NoFix
        };

        UtcTime = utcTime;
        Latitude = latitude;
        DirectionLatitude = directionLatitude;
        Longitude = longitude;
        DirectionLongitude = directionLongitude;
        Quality = fixType;
        TotalSatellitesInUse = satellites;
        HDOP = hdop;
        Altitude = altitude;
        AltitudeUnit = altitudeUnit;
        GeoidSeparation = geoidSeparation;
        GeoidSeparationUnit = geoidSeparationUnit;
        AgeOfDifferentialData = ageOfDifferentialData;
        DifferentialReferenceStationID = differentialReferenceStationID;

        GKCoordinate = CoordinateConverter.ConvertToGk(latitude, longitude, directionLatitude, directionLongitude, ConfigManager.CurrentConfiguration.GkSystemNumber, altitude);

        RoverX = GKCoordinate.N;
        RoverY = GKCoordinate.E;
        RoverZ = GKCoordinate.Z;
        //DeltaXY = MathUtils.Distance(RoverX, RoverY, ConfigManager.CurrentConfiguration.RoverLocationX, ConfigManager.CurrentConfiguration.RoverLocationY);
        DeltaX = RoverX - ConfigManager.CurrentConfiguration.RoverLocationX;
        DeltaY = RoverY - ConfigManager.CurrentConfiguration.RoverLocationY;
        DeltaZ = RoverZ - ConfigManager.CurrentConfiguration.RoverLocationZ;
    }
}