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
                    // �t�H���_
                    Process.Start("explorer.exe", $"\"{path}\"");
                }
                else if (File.Exists(path))
                {
                    // �t�@�C��
                    Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                }
                else
                {
                    // �R�}���h�Ƃ��Ď��s
                    Process.Start(new ProcessStartInfo("cmd.exe", $"/c {path}")
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"�N���Ɏ��s���܂���: {ex.Message}", ex);
            }
        }

        public string GetIconForPath(string path)
        {
            if (path.StartsWith("http://") || path.StartsWith("https://") || path.StartsWith("www."))
                return "Web";
            else if (Directory.Exists(path))
                return "DIR";
            else if (File.Exists(path))
            {
                var ext = Path.GetExtension(path).ToLower();
                switch (ext)
                {
                    case ".exe": return "EXE";
                    case ".txt": return "TXT";
                    case ".doc":
                    case ".docx": return "DOC";
                    case ".xls":
                    case ".xlsx": return "XLS";
                    case ".pdf": return "PDF";
                    case ".zip":
                    case ".rar": return "ZIP";
                    case ".jpg":
                    case ".png":
                    case ".gif": return "IMG";
                    case ".mp3":
                    case ".wav": return "SND";
                    case ".mp4":
                    case ".avi": return "VID";
                    default: return "FILE";
                }
            }
            else
                return "CMD"; // �R�}���h
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