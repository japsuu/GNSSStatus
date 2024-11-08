using GNSSStatus.Networking;
using GNSSStatus.Utils;

namespace GNSSStatus.Parsing;

public static class SentenceParser
{
    public static readonly GNSSData ParsedData = new();
    
    /// <summary>
    /// Parses the given NMEA sentence and writes relevant data to <see cref="ParsedData"/>.
    /// </summary>
    /// <param name="sentence">The sentence to parse.</param>
    public static void Parse(Nmea0183Sentence sentence)
    {
        switch (sentence.Type)
        {
            case Nmea0183SentenceType.GGA:
            {
                if (sentence.Parts.Length < GGAData.LENGTH)
                {
                    Logger.LogWarning($"Invalid {sentence.Type.ToString()} sentence received.");
                    return;
                }
                
                ParsedData.GGA = new GGAData(sentence);
                ParsedData.DeltaXCache.Add(ParsedData.GGA.DeltaX);
                ParsedData.DeltaYCache.Add(ParsedData.GGA.DeltaY);
                ParsedData.DeltaZCache.Add(ParsedData.GGA.DeltaZ);
                ParsedData.RoverXCache.Add(ParsedData.GGA.RoverX);
                ParsedData.RoverYCache.Add(ParsedData.GGA.RoverY);
                ParsedData.RoverZCache.Add(ParsedData.GGA.RoverZ);
                ParsedData.RoverUtcTimeCache.Add(ParsedData.GGA.UtcTime);
                ParsedData.RoverFixTypeCache.Add(ParsedData.GGA.Quality);
                ParsedData.RoverSatInUseCache.Add(ParsedData.GGA.TotalSatellitesInUse);
                break;
            }
            case Nmea0183SentenceType.GSA:
            {
                if (sentence.Parts.Length < GSAData.LENGTH)
                {
                    Logger.LogWarning($"Invalid {sentence.Type.ToString()} sentence received.");
                    return;
                }
            
                ParsedData.GSA = new GSAData(sentence);
                ParsedData.RoverPDopCache.Add(ParsedData.GSA.PDop);
                ParsedData.RoverVDopCache.Add(ParsedData.GSA.VDop);
                ParsedData.RoverHDopCache.Add(ParsedData.GSA.HDop);
                break;
            }
            case Nmea0183SentenceType.GST:
            {
                if (sentence.Parts.Length < GSTData.LENGTH)
                {
                    Logger.LogWarning($"Invalid {sentence.Type.ToString()} sentence received.");
                    return;
                }
            
                ParsedData.GST = new GSTData(sentence);
                ParsedData.RoverErrorAltitudeCache.Add(ParsedData.GST.AltitudeError);
                ParsedData.RoverErrorLongitudeCache.Add(ParsedData.GST.LongitudeError);
                ParsedData.RoverErrorLatitudeCache.Add(ParsedData.GST.LatitudeError);
                break;
            }
            case Nmea0183SentenceType.GSV:
            {
                if (sentence.Parts.Length < GSVData.LENGTH)
                {
                    Logger.LogWarning($"Invalid {sentence.Type.ToString()} sentence received.");
                    return;
                }
            
                ParsedData.GSV = new GSVData(sentence);
                break;
            }
            case Nmea0183SentenceType.NTR:
            {
                if (sentence.Parts.Length < NTRData.LENGTH)
                {
                    Logger.LogWarning($"Invalid {sentence.Type.ToString()} sentence received.");
                    return;
                }
            
                ParsedData.NTR = new NTRData(sentence);
                ParsedData.RoverBaselineCache.Add(ParsedData.NTR.DistanceBetweenBaseAndRover);
                break;
            }
            case Nmea0183SentenceType.GBS:
                break;
            case Nmea0183SentenceType.GLL:
                break;
            case Nmea0183SentenceType.RMC:
                break;
            case Nmea0183SentenceType.VTG:
                break;
            case Nmea0183SentenceType.ZDA:
                break;
            case Nmea0183SentenceType.UNKNOWN:
            default:
            {
                break;
            }
        }
    }
}