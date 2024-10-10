namespace GNSSStatus.Networking;

public readonly struct Nmea0183Sentence
{
    public readonly Nmea0183SentenceType Type;
    public readonly string Data;
    public readonly string[] Parts;


    public Nmea0183Sentence(string data)
    {
        if (!data.StartsWith('$'))
            throw new ArgumentException("NMEA0183 sentences must start with a '$' character.");
        
        Type = ParseType(data.Substring(1, 5));
        Data = data;
        Parts = data.Split(',');
    }
    
    
    private static Nmea0183SentenceType ParseType(string type)
    {
        if (type.Length != 5)
            throw new ArgumentException("NMEA0183 sentence types must be 5 characters long.");
        
        return type switch
        {
            "GBS" => Nmea0183SentenceType.GBS,
            "GNGGA" => Nmea0183SentenceType.GNGGA,
            "GLL" => Nmea0183SentenceType.GLL,
            "GSA" => Nmea0183SentenceType.GSA,
            "GST" => Nmea0183SentenceType.GST,
            "GSV" => Nmea0183SentenceType.GSV,
            "RMC" => Nmea0183SentenceType.RMC,
            "VTG" => Nmea0183SentenceType.VTG,
            "ZDA" => Nmea0183SentenceType.ZDA,
            "GPNTR" => Nmea0183SentenceType.GPNTR,
            _ => Nmea0183SentenceType.UNKNOWN
        };
    }


    public override string ToString() => $"NMEA0183 Sentence: {Type} - {Data}";
}