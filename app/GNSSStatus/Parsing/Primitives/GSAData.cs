using GNSSStatus.Networking;

namespace GNSSStatus.Parsing;

public readonly struct GSAData
{
    public const int LENGTH = 17;
    
    public readonly char OperationMode;
    public readonly int NavigationMode;
    public readonly int[] PRNs;
    public readonly float PDop;
    public readonly float HDop;
    public readonly float VDop;


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
        
        OperationMode = mode[0];
        NavigationMode = int.Parse(fix);
        PRNs = new int[12];
        for (int i = 0; i < 12; i++)
        {
            if (!int.TryParse(prns[i], out int res))
                continue;
            PRNs[i] = res;
        }
        PDop = float.Parse(pDop);
        HDop = float.Parse(hDop);
        VDop = float.Parse(vDop);
    }
}