using System.Text;

namespace GNSSStatus.Parsing;

public class GNSSData
{
    public GGAData GGA { get; set; }
    public GSAData GSA { get; set; }
    public GSTData GST { get; set; }
    
    
    public override string ToString()
    {
        StringBuilder sb = new();
        
        sb.AppendLine("GNSS Data:");
        sb.AppendLine($"  {GGA}");
        sb.AppendLine($"  {GSA}");
        sb.AppendLine($"  {GST}");
        
        return sb.ToString();
    }
}