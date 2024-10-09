namespace GNSSStatus.Utils;

public static class TimeUtils
{
    /// <summary>
    /// Gets the current time in milliseconds.
    /// </summary>
    /// <returns>The current time in milliseconds.</returns>
    public static double GetTimeMillis()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}