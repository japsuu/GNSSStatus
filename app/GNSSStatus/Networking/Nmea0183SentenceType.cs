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
    GNGGA,
    
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
    /// 1 $GPNTR Log header $GPNTR
    /// 2 utc time
    /// 3 quality 0 = fix not available or invalid, 1 = single point position, 2 = DGPS or SBAS, 4 = RTK fix, 5 =RTK float, 6 = INS, 7 = manual input mode(FixedPosition)
    /// 4 Distance Distance between base and rover, in meters !!!
    /// 5 N Distance in North, +: North, -: South
    /// 6 E Distance in East, +: East, -: West
    /// 7 U Distance in vertical direction +: Up, -: Down
    /// 8 stn ID Base station ID，0000-4096
    /// 9 *xx Check sum
    /// 10 [CR][LF] Sentence terminator
    /// </summary>
    GPNTR,
}