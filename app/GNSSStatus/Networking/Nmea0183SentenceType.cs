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
    
    /// <summary>
    /// This message includes distance between base and rover, distance in east, north and vertical dimension respectively
    /// $GPNTR,212805.00,4,13417.470,+12167.763,-5654.401,-42.319,117*71
    /// 0 $GPNTR Log header $GPNTR
    /// 1 utc time
    /// 2 quality 0 = fix not available or invalid, 1 = single point position, 2 = DGPS or SBAS, 4 = RTK fix, 5 =RTK float, 6 = INS, 7 = manual input mode(FixedPosition)
    /// 3 Distance Distance between base and rover, in meters !!!
    /// 4 N Distance in North, +: North, -: South
    /// 5 E Distance in East, +: East, -: West
    /// 6 U Distance in vertical direction +: Up, -: Down
    /// 7 stn ID Base station ID，0000-4096
    /// 8 *xx Check sum
    /// 9 [CR][LF] Sentence terminator
    /// </summary>
    NTR,
}