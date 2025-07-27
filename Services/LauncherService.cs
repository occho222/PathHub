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
                // 最終アクセス日時を更新
                item.LastAccessed = DateTime.Now;
                
                // VSCodeで開くオプションがオンの場合はVSCodeで開く
                if (item.OpenWithVSCode)
                {
                    LaunchItemWithVSCode(item);
                    return;
                }
                
                // Officeアプリで開くオプションがオンの場合はOfficeアプリで開く
                if (item.OpenWithOffice)
                {
                    LaunchItemWithOffice(item);
                    return;
                }
                
                string path = item.Path;

                if (path.StartsWith("http://") || path.StartsWith("https://") || path.StartsWith("www."))
                {
                    if (path.StartsWith("www."))
                        path = "http://" + path;
                    Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                }
                else if (Directory.Exists(path))
                {
                    Process.Start("explorer.exe", "\"" + path + "\"");
                }
                else if (File.Exists(path))
                {
                    Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                }
                else
                {
                    Process.Start(new ProcessStartInfo("cmd.exe", "/c " + path)
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("起動に失敗しました: " + ex.Message, ex);
            }
        }

        public void LaunchItemWithVSCode(LauncherItem item)
        {
            try
            {
                // 最終アクセス日時を更新
                item.LastAccessed = DateTime.Now;
                
                string path = item.Path;

                // VSCodeのパスを検索
                string vsCodePath = FindVSCodePath();
                if (string.IsNullOrEmpty(vsCodePath))
                {
                    throw new InvalidOperationException("VS Code が見つかりません。VS Code がインストールされていることを確認してください。");
                }

                // VSCodeで開く
                if (Directory.Exists(path))
                {
                    // フォルダの場合
                    Process.Start(new ProcessStartInfo(vsCodePath, $"\"{path}\"") { UseShellExecute = true });
                }
                else if (File.Exists(path))
                {
                    // ファイルの場合
                    Process.Start(new ProcessStartInfo(vsCodePath, $"\"{path}\"") { UseShellExecute = true });
                }
                else
                {
                    // パスが存在しない場合は通常の起動方法を試す
                    LaunchItem(new LauncherItem { Path = path, OpenWithVSCode = false });
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("VS Code での起動に失敗しました: " + ex.Message, ex);
            }
        }

        public void LaunchItemWithOffice(LauncherItem item)
        {
            try
            {
                // 最終アクセス日時を更新
                item.LastAccessed = DateTime.Now;
                
                string path = item.Path;

                // URLまたはSharePointの場合
                if (path.StartsWith("http://") || path.StartsWith("https://"))
                {
                    LaunchOfficeUrl(path);
                    return;
                }

                // ローカルファイルの場合
                if (File.Exists(path))
                {
                    LaunchOfficeFile(path);
                    return;
                }

                // ファイルが存在しない場合は通常の起動方法を試す
                LaunchItem(new LauncherItem { Path = path, OpenWithOffice = false });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Office アプリでの起動に失敗しました: " + ex.Message, ex);
            }
        }

        private void LaunchOfficeUrl(string url)
        {
            // SharePointや Office Online のURLをOfficeアプリで開く
            string officeUri = "";
            
            // URLの種類に応じてOffice URIスキームを使用
            if (url.Contains("sharepoint.com") || url.Contains("office365.sharepoint.com") || 
                url.Contains("-my.sharepoint.com"))
            {
                // SharePointのファイルの場合（組織サイトと個人用OneDriveの両方に対応）
                if (url.Contains(".xlsx") || url.Contains("workbook") || url.Contains(":x:"))
                {
                    officeUri = $"ms-excel:ofe|u|{url}";
                }
                else if (url.Contains(".docx") || url.Contains("document") || url.Contains(":w:"))
                {
                    officeUri = $"ms-word:ofe|u|{url}";
                }
                else if (url.Contains(".pptx") || url.Contains("presentation") || url.Contains(":p:"))
                {
                    officeUri = $"ms-powerpoint:ofe|u|{url}";
                }
                else
                {
                    // ファイル種類が不明な場合はブラウザで開く
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                    return;
                }
            }
            else if (url.Contains("docs.google.com"))
            {
                // Google Docsの場合は適切なOfficeアプリで開く
                if (url.Contains("/spreadsheets/"))
                {
                    officeUri = $"ms-excel:ofe|u|{url}";
                }
                else if (url.Contains("/document/"))
                {
                    officeUri = $"ms-word:ofe|u|{url}";
                }
                else if (url.Contains("/presentation/"))
                {
                    officeUri = $"ms-powerpoint:ofe|u|{url}";
                }
                else
                {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                    return;
                }
            }
            else
            {
                // その他のURLはブラウザで開く
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                return;
            }

            // Office URIスキームで起動
            Process.Start(new ProcessStartInfo(officeUri) { UseShellExecute = true });
        }

        private void LaunchOfficeFile(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLower();
            string officeUri = "";

            // ファイル拡張子に応じてOffice URIスキームを使用
            switch (ext)
            {
                case ".xlsx":
                case ".xls":
                case ".xlsm":
                case ".xlsb":
                    officeUri = $"ms-excel:ofe|u|file:///{filePath.Replace('\\', '/')}";
                    break;
                case ".docx":
                case ".doc":
                case ".docm":
                    officeUri = $"ms-word:ofe|u|file:///{filePath.Replace('\\', '/')}";
                    break;
                case ".pptx":
                case ".ppt":
                case ".pptm":
                    officeUri = $"ms-powerpoint:ofe|u|file:///{filePath.Replace('\\', '/')}";
                    break;
                default:
                    // Officeファイルでない場合は通常の方法で開く
                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                    return;
            }

            // Office URIスキームで起動
            Process.Start(new ProcessStartInfo(officeUri) { UseShellExecute = true });
        }

        private string FindVSCodePath()
        {
            // 一般的なVS Codeのインストールパス
            string[] possiblePaths = {
                @"C:\Users\" + Environment.UserName + @"\AppData\Local\Programs\Microsoft VS Code\Code.exe",
                @"C:\Program Files\Microsoft VS Code\Code.exe",
                @"C:\Program Files (x86)\Microsoft VS Code\Code.exe",
                "code" // PATH環境変数にある場合
            };

            foreach (string path in possiblePaths)
            {
                if (path == "code")
                {
                    // PATH環境変数で"code"コマンドが使用可能かチェック
                    try
                    {
                        var process = Process.Start(new ProcessStartInfo("code", "--version")
                        {
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        });
                        process?.WaitForExit();
                        if (process?.ExitCode == 0)
                        {
                            return "code";
                        }
                    }
                    catch
                    {
                        // PATH環境変数にcodeコマンドがない場合は無視
                    }
                }
                else if (File.Exists(path))
                {
                    return path;
                }
            }

            return string.Empty;
        }

        public string GetIconForPath(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    return "?";

                if (path.StartsWith("http://") || path.StartsWith("https://") || path.StartsWith("www."))
                {
                    var uri = new Uri(path.StartsWith("www.") ? "http://" + path : path);
                    var host = uri.Host.ToLower();
                    var lowerPath = path.ToLower();
                    
                    if (host.Contains("github.com"))
                        return "??";
                    else if (host.Contains("gitlab.com") || lowerPath.Contains("gitlab"))
                        return "??";
                    else if (lowerPath.Contains("redmine"))
                        return "??";
                    else if (host.Contains("drive.google.com") || host.Contains("docs.google.com"))
                        return "??";
                    else if (host.Contains("teams.microsoft.com") || host.Contains("teams.live.com"))
                        return "??";
                    else if (host.Contains("sharepoint.com") || host.Contains(".sharepoint.com") || 
                             host.EndsWith("sharepoint.com") || host.Contains("office365.sharepoint.com") ||
                             host.Contains("-my.sharepoint.com"))
                        return "??";
                    else if (host.Contains("outlook.office365.com") || host.Contains("outlook.office.com") ||
                             host.Contains("onedrive.live.com") || host.Contains("1drv.ms"))
                        return "??";
                    else
                        return "??";
                }

                if (Directory.Exists(path))
                {
                    return "??";
                }

                if (File.Exists(path))
                {
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

                return "?";
            }
            catch (Exception)
            {
                return "?";
            }
        }

        public string GetItemType(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "不明";

            if (path.StartsWith("http://") || path.StartsWith("https://") || path.StartsWith("www."))
                return "Web";

            if (Directory.Exists(path))
                return "フォルダ";

            if (File.Exists(path))
            {
                var ext = Path.GetExtension(path).ToLower();
                return ext switch
                {
                    ".exe" or ".msi" or ".bat" or ".cmd" => "実行ファイル",
                    ".txt" or ".rtf" => "テキスト",
                    ".doc" or ".docx" => "Word文書",
                    ".xls" or ".xlsx" => "Excel文書",
                    ".ppt" or ".pptx" => "PowerPoint",
                    ".pdf" => "PDF",
                    ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".svg" or ".webp" => "画像",
                    ".mp3" or ".wav" or ".wma" or ".flac" or ".aac" or ".ogg" => "音楽",
                    ".mp4" or ".avi" or ".mkv" or ".wmv" or ".mov" or ".flv" or ".webm" => "動画",
                    ".zip" or ".rar" or ".7z" or ".tar" or ".gz" or ".bz2" => "圧縮ファイル",
                    ".lnk" => "ショートカット",
                    ".py" or ".js" or ".html" or ".css" or ".cpp" or ".c" or ".cs" or ".java" or ".php" => "プログラム",
                    _ => "ファイル"
                };
            }

            return "コマンド";
        }

        public string DetectCategory(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    return "その他";

                if (path.StartsWith("http://") || path.StartsWith("https://") || path.StartsWith("www."))
                {
                    var uri = new Uri(path.StartsWith("www.") ? "http://" + path : path);
                    var host = uri.Host.ToLower();
                    var lowerPath = path.ToLower();
                    
                    if (host.Contains("github.com"))
                        return "GitHubURL";
                    else if (host.Contains("gitlab.com") || lowerPath.Contains("gitlab"))
                        return "GitLabURL";
                    else if (lowerPath.Contains("redmine"))
                        return "RedmineURL";
                    else if (host.Contains("drive.google.com") || host.Contains("docs.google.com"))
                        return "Googleドライブ";
                    else if (host.Contains("teams.microsoft.com") || host.Contains("teams.live.com"))
                        return "MicrosoftTeams";
                    else if (host.Contains("sharepoint.com") || host.Contains(".sharepoint.com") || 
                             host.EndsWith("sharepoint.com") || host.Contains("office365.sharepoint.com") ||
                             host.Contains("-my.sharepoint.com"))
                        return "SharePoint";
                    else if (host.Contains("outlook.office365.com") || host.Contains("outlook.office.com") ||
                             host.Contains("onedrive.live.com") || host.Contains("1drv.ms"))
                        return "OneDrive";
                    else
                        return "Webサイト";
                }

                if (Directory.Exists(path))
                {
                    return "フォルダ";
                }

                if (File.Exists(path))
                {
                    var ext = Path.GetExtension(path).ToLower();
                    return ext switch
                    {
                        ".exe" or ".msi" or ".bat" or ".cmd" => "アプリケーション",
                        ".txt" or ".rtf" => "ドキュメント",
                        ".doc" or ".docx" => "Word",
                        ".xls" or ".xlsx" => "Excel",
                        ".ppt" or ".pptx" => "PowerPoint",
                        ".pdf" => "PDF",
                        ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".svg" or ".webp" => "画像",
                        ".mp3" or ".wav" or ".wma" or ".flac" or ".aac" or ".ogg" => "音楽",
                        ".mp4" or ".avi" or ".mkv" or ".wmv" or ".mov" or ".flv" or ".webm" => "動画",
                        ".zip" or ".rar" or ".7z" or ".tar" or ".gz" or ".bz2" => "アーカイブ",
                        ".lnk" => "ショートカット",
                        ".py" or ".js" or ".html" or ".css" or ".cpp" or ".c" or ".cs" or ".java" or ".php" => "プログラム",
                        _ => "ファイル"
                    };
                }

                return "コマンド";
            }
            catch (Exception)
            {
                return "その他";
            }
        }
    }
}