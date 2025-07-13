using ModernLauncher.Interfaces;
using ModernLauncher.Models;
using System;
using System.Diagnostics;
using System.IO;

namespace ModernLauncher.Services
{
    public class LauncherService : ILauncherService
    {
        public void LaunchItem(LauncherItem item)
        {
            try
            {
                string path = item.Path;

                if (path.StartsWith("http://") || path.StartsWith("https://") || path.StartsWith("www."))
                {
                    // URL
                    if (path.StartsWith("www."))
                        path = "http://" + path;
                    Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                }
                else if (Directory.Exists(path))
                {
                    // フォルダ
                    Process.Start("explorer.exe", $"\"{path}\"");
                }
                else if (File.Exists(path))
                {
                    // ファイル
                    Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                }
                else
                {
                    // コマンドとして実行
                    Process.Start(new ProcessStartInfo("cmd.exe", $"/c {path}")
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"起動に失敗しました: {ex.Message}", ex);
            }
        }

        public string GetIconForPath(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    return "?";

                // URLの場合
                if (path.StartsWith("http://") || path.StartsWith("https://") || path.StartsWith("www."))
                {
                    var uri = new Uri(path.StartsWith("www.") ? "http://" + path : path);
                    var host = uri.Host.ToLower();
                    
                    if (host.Contains("github.com"))
                        return "??";
                    else if (host.Contains("gitlab.com"))
                        return "??";
                    else if (host.Contains("drive.google.com") || host.Contains("docs.google.com"))
                        return "??";
                    else if (host.Contains("teams.microsoft.com") || host.Contains("teams.live.com"))
                        return "??";
                    else if (host.Contains("sharepoint.com") || host.Contains(".sharepoint.com") || 
                             host.EndsWith("sharepoint.com") || host.Contains("office365.sharepoint.com"))
                        return "???";
                    else if (host.Contains("outlook.office365.com") || host.Contains("outlook.office.com") ||
                             host.Contains("onedrive.live.com") || host.Contains("1drv.ms"))
                        return "??";
                    else
                        return "??";
                }

                // ディレクトリの場合
                if (Directory.Exists(path))
                {
                    // G:ドライブの場合はGoogleドライブアイコン
                    if (path.StartsWith("G:", StringComparison.OrdinalIgnoreCase) || 
                        path.StartsWith("G\\", StringComparison.OrdinalIgnoreCase))
                    {
                        return "??";
                    }
                    return "??";
                }

                // ファイルの場合
                if (File.Exists(path))
                {
                    // G:ドライブのファイルもGoogleドライブアイコン
                    if (path.StartsWith("G:", StringComparison.OrdinalIgnoreCase) || 
                        path.StartsWith("G\\", StringComparison.OrdinalIgnoreCase))
                    {
                        return "??";
                    }

                    var ext = Path.GetExtension(path).ToLower();
                    return ext switch
                    {
                        ".exe" or ".msi" or ".bat" or ".cmd" => "??",
                        ".txt" or ".rtf" => "??",
                        ".doc" or ".docx" => "??",
                        ".xls" or ".xlsx" => "??",
                        ".ppt" or ".pptx" => "??",
                        ".pdf" => "??",
                        ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".svg" or ".webp" => "???",
                        ".mp3" or ".wav" or ".wma" or ".flac" or ".aac" or ".ogg" => "??",
                        ".mp4" or ".avi" or ".mkv" or ".wmv" or ".mov" or ".flv" or ".webm" => "??",
                        ".zip" or ".rar" or ".7z" or ".tar" or ".gz" or ".bz2" => "??",
                        ".lnk" => "??",
                        ".py" or ".js" or ".html" or ".css" or ".cpp" or ".c" or ".cs" or ".java" or ".php" => "??",
                        _ => "??"
                    };
                }

                // G:で始まる存在しないパスもGoogleドライブアイコン
                if (path.StartsWith("G:", StringComparison.OrdinalIgnoreCase) || 
                    path.StartsWith("G\\", StringComparison.OrdinalIgnoreCase))
                {
                    return "??";
                }

                // コマンドの場合
                return "?";
            }
            catch (Exception)
            {
                return "?";
            }
        }

        public string GetItemType(string path)
        {
            if (path.StartsWith("http://") || path.StartsWith("https://") || path.StartsWith("www."))
                return "Web";
            else if (Directory.Exists(path))
                return "Folder";
            else if (File.Exists(path))
                return "File";
            else
                return "Command";
        }
    }
}