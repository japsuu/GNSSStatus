using GNSSStatus.Configuration;
using GNSSStatus.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GNSSStatus.Parsing;

public sealed class IonoClient : IDisposable
{
    private readonly HttpClient _httpClient = new();
    private double _lastIonoUpdate = -(ConfigManager.FINPOS_IONO_PARSE_INTERVAL_MILLIS + 1);    // Force an update on the first call.
    private double _latestIonoPercentage;


    public async Task<double> GetIonoPercentage()
    {
        if (TimeUtils.GetTimeMillis() - _lastIonoUpdate >= ConfigManager.FINPOS_IONO_PARSE_INTERVAL_MILLIS)
        {
            Logger.LogDebug("Parsing the latest ionospheric percentage...");
            _latestIonoPercentage = await ReadLatestIonoPercentage();
            _lastIonoUpdate = TimeUtils.GetTimeMillis();
            Logger.LogDebug($"The latest ionospheric percentage is: {_latestIonoPercentage}%");
        }
        
        return _latestIonoPercentage;
    }


    private async Task<double> ReadLatestIonoPercentage()
    {
        const int left = 132;
        const int top = 380;
        const int right = 902;
        const int bottom = 584;
        Rgba32 targetColor = new(255, 0, 0);
        
        byte[] imageData = await _httpClient.GetByteArrayAsync(ConfigManager.FINPOS_IONO_IMAGE_URL);

        using MemoryStream ms = new(imageData);
        using Image<Rgba32> image = await Image.LoadAsync<Rgba32>(ms);
        
        // Cut the image to the graph area.
        image.Mutate(x => x.Crop(new Rectangle(left, top, right - left, bottom - top)));
        
        // Loop the image pixel columns from right to left.
        for (int x = image.Width - 1; x >= 0; x--)
        {
            // Since a single column can contain multiple pixels of the target color,
            // we need to store the Y coordinates of the lowest and highest found pixels.
            int lowestY = int.MaxValue;
            int highestY = int.MinValue;
            
            // Loop the image pixel rows from top to bottom.
            for (int y = 0; y < image.Height; y++)
            {
                Rgba32 pixel = image[x, y];

                if (!pixel.Equals(targetColor))
                    continue;
                
                lowestY = Math.Min(lowestY, y);
                highestY = Math.Max(highestY, y);
            }
            
            // If no pixels of the target color were found in the column, continue to the next column.
            if (lowestY == int.MaxValue && highestY == int.MinValue)
                continue;
            
            // Get the middle Y coordinate of the found pixels.
            double middleY;
            if (lowestY == int.MaxValue)
                middleY = highestY;
            else if (highestY == int.MinValue)
                middleY = lowestY;
            else
                middleY = (lowestY + highestY) / 2.0;
            
            // Get the percentage of the middle Y coordinate in relation to the graph area.
            // The percentage is calculated from the bottom of the graph area (highest Y coordinate).
            double height = image.Height;
            double percentageFp = (1.0 - middleY / height) * 100.0;
            int percentage = (int)Math.Round(percentageFp);
            
            // Ensure the percentage is within the range of 0-100.
            return Math.Clamp(percentage, 0.0, 100.0);
        }
        
        // The graph image did not contain any pixels of the target color.
        // This means that the graph line is above the graph area.
        Logger.LogWarning("The FINPOS iono graph not detected. Assuming 100% ionospheric percentage.");
        return 100.0;
    }

    
    public void Dispose()
    {
        _httpClient.Dispose();
    }
}