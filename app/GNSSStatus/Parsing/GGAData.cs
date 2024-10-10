using System.Text;
using GNSSStatus.Configuration;
using GNSSStatus.Coordinates;
using GNSSStatus.Networking;

namespace GNSSStatus.Parsing;

public readonly struct GGAData
{
    public const int LENGTH = 14;

    public readonly GKCoordinate GKCoordinate;
    public readonly double DeltaX;
    public readonly double DeltaY;
    public readonly double DeltaZ;
    public readonly string UtcTime;
    public readonly string Latitude;
    public readonly string DirectionLatitude;
    public readonly string Longitude;
    public readonly string DirectionLongitude;
    public readonly string Quality;
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

        UtcTime = utcTime;
        Latitude = latitude;
        DirectionLatitude = directionLatitude;
        Longitude = longitude;
        DirectionLongitude = directionLongitude;
        Quality = quality;
        TotalSatellitesInUse = satellites;
        HDOP = hdop;
        Altitude = altitude;
        AltitudeUnit = altitudeUnit;
        GeoidSeparation = geoidSeparation;
        GeoidSeparationUnit = geoidSeparationUnit;
        AgeOfDifferentialData = ageOfDifferentialData;
        DifferentialReferenceStationID = differentialReferenceStationID;

        GKCoordinate = CoordinateConverter.ConvertToGk(latitude, longitude, directionLatitude, directionLongitude, ConfigManager.CurrentConfiguration.GkSystemNumber, altitude);

        DeltaX = GKCoordinate.N - ConfigManager.CurrentConfiguration.RoverLocationX;
        DeltaY = GKCoordinate.E - ConfigManager.CurrentConfiguration.RoverLocationY;
        DeltaZ = GKCoordinate.Z - ConfigManager.CurrentConfiguration.RoverLocationZ;
    }


    public override string ToString()
    {
        StringBuilder sb = new();

        sb.AppendLine("GGA Data:");
        sb.AppendLine($"  UTC Time: {UtcTime}");
        sb.AppendLine($"  Latitude: {Latitude} {DirectionLatitude}");
        sb.AppendLine($"  Longitude: {Longitude} {DirectionLongitude}");
        sb.AppendLine($"  Quality: {Quality}");
        sb.AppendLine($"  Satellites: {TotalSatellitesInUse}");
        sb.AppendLine($"  HDOP: {HDOP}");
        sb.AppendLine($"  Altitude: {Altitude} {AltitudeUnit}");
        sb.AppendLine($"  Geoid Separation: {GeoidSeparation} {GeoidSeparationUnit}");
        sb.AppendLine($"  Age of Differential Data: {AgeOfDifferentialData}");
        sb.AppendLine($"  Differential Reference Station ID: {DifferentialReferenceStationID}");
        sb.AppendLine($"  GK Coordinate: {GKCoordinate}");
        sb.AppendLine($"  Deltas: X={DeltaX}, Y={DeltaY}, Z={DeltaZ}");

        return sb.ToString();
    }
}