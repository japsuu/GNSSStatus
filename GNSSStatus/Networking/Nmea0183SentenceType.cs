namespace GNSSStatus.Networking;

public enum Nmea0183SentenceType
{
    /// <summary>
    /// Unknown sentence type.
    /// </summary>
    UNKNOWN,
    
    /// <summary>
    /// Satellite Fault Detection.
    /// </summary>
    GBS,
    
    /// <summary>
    /// Fix Data.
    /// </summary>
    GGA,
    
    /// <summary>
    /// Geographic Position: Latitude/Longitude.
    /// </summary>
    GLL,
    
    /// <summary>
    /// DOP and active satellites.
    /// </summary>
    GSA,
    
    /// <summary>
    /// Pseudorange Noise Statistics.
    /// </summary>
    GST,
    
    /// <summary>
    /// Satellites in view.
    /// </summary>
    GSV,
    
    /// <summary>
    /// Recommended Minimum: position, velocity, time.
    /// </summary>
    RMC,
    
    /// <summary>
    /// Track made good and Ground speed.
    /// </summary>
    VTG,
    
    /// <summary>
    /// Time & Date - UTC, day, month, year and local time zone.
    /// </summary>
    ZDA,
}