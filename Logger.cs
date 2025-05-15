using System;
using System.IO;

public static class Logger
{
    private static readonly string LogFileName = "WetworksErrLog.txt";
    private static readonly string LogFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WetWorks_Networks", LogFileName);
    private static readonly object _lock = new object(); // For thread-safe logging

    public static void LogError(string message, Exception ex = null)
    {
        try
        {
            // Ensure the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(LogFilePath));

            lock (_lock)
            {
                using (StreamWriter sw = new StreamWriter(LogFilePath, true)) // true to append
                {
                    sw.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message}");
                    if (ex != null)
                    {
                        sw.WriteLine($"Exception Type: {ex.GetType().FullName}");
                        sw.WriteLine($"Exception Message: {ex.Message}");
                        sw.WriteLine($"Stack Trace: {ex.StackTrace}");
                        if (ex.InnerException != null)
                        {
                            sw.WriteLine($"Inner Exception Message: {ex.InnerException.Message}");
                        }
                    }
                    sw.WriteLine(new string('-', 50)); // Separator
                }
            }
        }
        catch (Exception logEx)
        {
            // If logging fails, we can't really log it,
            // but could potentially use Debug.WriteLine or Console.WriteLine
            System.Diagnostics.Debug.WriteLine($"Failed to write to log file: {logEx.Message}");
        }
    }

    // Optional: Add a method to log information messages if needed
    // public static void LogInfo(string message)
    // {
    //     try
    //     {
    //         Directory.CreateDirectory(Path.GetDirectoryName(LogFilePath));
    //         lock (_lock)
    //         {
    //             using (StreamWriter sw = new StreamWriter(LogFilePath, true))
    //             {
    //                 sw.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INFO: {message}");
    //             }
    //         }
    //     }
    //     catch (Exception logEx)
    //     {
    //         System.Diagnostics.Debug.WriteLine($"Failed to write to log file: {logEx.Message}");
    //     }
    // }
}