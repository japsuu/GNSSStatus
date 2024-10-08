﻿namespace GNSSStatus;

public static class Logger
{
    private enum LogLevel
    {
        DEBUG,
        INFO,
        WARNING,
        ERROR,
        FATAL
    }
    
    
    public static void LogDebug(string message)
    {
#if DEBUG
        WriteColored(LogLevel.DEBUG, message, ConsoleColor.Gray, ConsoleColor.Black);
#endif
    }
    
    
    public static void LogInfo(string message)
    {
        WriteColored(LogLevel.INFO, message, ConsoleColor.White, ConsoleColor.Black);
    }
    
    
    public static void LogWarning(string message)
    {
        WriteColored(LogLevel.WARNING, message, ConsoleColor.Yellow, ConsoleColor.Black);
    }


    public static void LogError(string message)
    {
        WriteColored(LogLevel.ERROR, message, ConsoleColor.Red, ConsoleColor.Black);
    }


    public static void LogException(string message, Exception ex)
    {
        WriteColored(LogLevel.FATAL, message, ConsoleColor.Red, ConsoleColor.Black);
        WriteColored(LogLevel.FATAL, ex.ToString(), ConsoleColor.Red, ConsoleColor.Black);
        
        throw ex;
    }


    private static void WriteColored(LogLevel level, string message, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
    {
        ConsoleColor fgCache = Console.ForegroundColor;
        ConsoleColor bgCache = Console.BackgroundColor;
        Console.ForegroundColor = foregroundColor;
        Console.BackgroundColor = backgroundColor;
        
        string levelString = level.ToString();
        Console.WriteLine($"[{levelString}] {message}");
        
        Console.ForegroundColor = fgCache;
        Console.BackgroundColor = bgCache;
    }
}