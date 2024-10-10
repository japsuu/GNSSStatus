using System.Text;
using GNSSStatus.Networking;

namespace GNSSStatus.Parsing;

public readonly struct GSAData
{
    public const int LENGTH = 17;
    
    public readonly string OperationMode;
    public readonly string NavigationMode;
    public readonly string[] PRNs;
    public readonly string PDOP;
    public readonly string HDOP;
    public readonly string VDOP;


    public GSAData(Nmea0183Sentence sentence)
    {
        // M = Manual
        // A = Automatic
        string mode = sentence.Parts[1];
            
        // 1 = Fix not available
        // 2 = 2D
        // 3 = 3D
        string fix = sentence.Parts[2];
            
        // PRNs of satellites used for fix (max 12)
        string[] prns = sentence.Parts[3..15];
            
        // Dilution of Precision (DOP) values
        string pDop = sentence.Parts[15];
        string hDop = sentence.Parts[16];
        string vDop = sentence.Parts[17];
        
        OperationMode = mode;
        NavigationMode = fix;
        PRNs = prns;
        PDOP = pDop;
        HDOP = hDop;
        VDOP = vDop;
    }
}