using System;
using System.IO;

namespace PathHub.Utils
{
    public static class DebugLogger
    {
        private static readonly string LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PathHub",
            "debug.log"
        );

        static DebugLogger()
        {
            // ログディレクトリを作成
            var logDir = Path.GetDirectoryName(LogPath);
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
        }

        public static void Log(string message)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var logMessage = $"[{timestamp}] {message}\n";

                File.AppendAllText(LogPath, logMessage);

                // コンソールにも出力
                System.Diagnostics.Debug.WriteLine($"[DEBUG] {message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to write log: {ex.Message}");
            }
        }

        public static void Clear()
        {
            try
            {
                File.WriteAllText(LogPath, $"=== DEBUG LOG STARTED at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\n");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to clear log: {ex.Message}");
            }
        }

        public static string GetLogPath()
        {
            return LogPath;
        }
    }
}