namespace GNSSStatus.Utils;

public static class MathUtils
{
    public static int Median(this IEnumerable<int> source) => MedianInPlace(source.ToArray());
    public static float Median(this IEnumerable<float> source) => MedianInPlace(source.ToArray());
    public static double Median(this IEnumerable<double> source) => MedianInPlace(source.ToArray());


    private static int MedianInPlace(int[] source)
    {
        Array.Sort(source);
        int n = source.Length;
        
        // If the length is even, the median is calculated as the average of the two middle elements.
        if (n % 2 == 0)
            return (source[n / 2 - 1] + source[n / 2]) / 2;
        
        // If the length is odd, the median is the middle element of the sorted array.
        return source[n / 2];
    }
    
    
    private static float MedianInPlace(float[] source)
    {
        Array.Sort(source);
        int n = source.Length;
        
        // If the length is even, the median is calculated as the average of the two middle elements.
        if (n % 2 == 0)
            return (source[n / 2 - 1] + source[n / 2]) / 2.0f;
        
        // If the length is odd, the median is the middle element of the sorted array.
        return source[n / 2];
    }
    
    
    private static double MedianInPlace(double[] source)
    {
        Array.Sort(source);
        int n = source.Length;
        
        // If the length is even, the median is calculated as the average of the two middle elements.
        if (n % 2 == 0)
            return (source[n / 2 - 1] + source[n / 2]) / 2.0;
        
        // If the length is odd, the median is the middle element of the sorted array.
        return source[n / 2];
    }
}