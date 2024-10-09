using System.Globalization;
using GNSSStatus.Networking;
using GNSSStatus.Nmea;

namespace GNSSStatus;

internal static class Program
{
    private static string serverAddress = "192.168.1.42";
    private static int port = 2999;


    private static void Main(string[] args)
    {
        CultureInfo culture = new("en-US");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        // If args contains a server address and port, use those instead.
        ParseArgs(args);

        // Create a new NMEA client.
        using NmeaClient client = new(serverAddress, port);

        // Read the latest received NMEA sentence from the server.
        foreach (Nmea0183Sentence sentence in client.ReadSentence())
            HandleSentence(sentence);
    }


    /*
        Message ID $GPGGA
        1 UTC of position fix
        2	Latitude
        3	Direction of latitude:
        N: North
        S: South
        
        4	Longitude
        5	Direction of longitude:
        E: East
        W: West
        
        6	GPS Quality indicator:
        0: Fix not valid
        1: GPS fix
        2: Differential GPS fix (DGNSS), SBAS, OmniSTAR VBS, Beacon, RTX in GVBS mode
        3: Not applicable
        4: RTK Fixed, xFill
        5: RTK Float, OmniSTAR XP/HP, Location RTK, RTX
        6: INS Dead reckoning
        
        7	Number of SVs in use, range from 00 through to 24+
        8	HDOP
        9	Orthometric height (MSL reference)
        10	M: unit of measure for orthometric height is meters
        11	Geoid separation
        12	M: geoid separation measured in meters
        13	Age of differential GPS data record, Type 1 or Type 9. Null field when DGPS is not used.
        14	Reference station ID, range 0000 to 4095. A null field when any reference station ID is selected and no corrections are received. See table below for a description of the field values.
        15	The checksum data, always begins with *
    */


    private static void HandleSentence(Nmea0183Sentence sentence)
    {
        if (sentence.Type == Nmea0183SentenceType.GGA)
        {
            string[] parts = sentence.Data.Split(',');

            if (parts.Length < 10)
                return;

            string altitude = parts[9];
            string altitudeUnit = parts[10];
            string utcTime = parts[1];
            string latitudi = parts[2];
            string directionLatitudi = parts[3];
            string longitudi = parts[4];
            string directionLongitudi = parts[5];
            string quality = parts[6];

            CoordinateConverter.create_dem();
            ConvertedCoordinate GK = CoordinateConverter.ConvertToGk(latitudi, longitudi, directionLatitudi, directionLongitudi, 21, altitude);

            Logger.LogInfo($"GK21 X: {GK.N.ToString("#.000")} Y: {GK.E.ToString("#.000")} N2000 Korkeus: {GK.Z.ToString("#.000")}");
        }
    }


    private static void ParseArgs(string[] args)
    {
        if (args.Length == 2)
        {
            serverAddress = args[0];
            port = int.Parse(args[1]);
        }
    }
}