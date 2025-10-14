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
                // �ŏI�A�N�Z�X�������X�V
                item.LastAccessed = DateTime.Now;
                
                // VSCode�ŊJ���I�v�V�������I���̏ꍇ��VSCode�ŊJ��
                if (item.OpenWithVSCode)
                {
                    LaunchItemWithVSCode(item);
                    return;
                }
                
                // Office�A�v���ŊJ���I�v�V�������I���̏ꍇ��Office�A�v���ŊJ��
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
                throw new InvalidOperationException("�N���Ɏ��s���܂���: " + ex.Message, ex);
            }
        }

        public void LaunchItemWithVSCode(LauncherItem item)
        {
            try
            {
                // �ŏI�A�N�Z�X�������X�V
                item.LastAccessed = DateTime.Now;
                
                string path = item.Path;

                // VSCode�̃p�X������
                string vsCodePath = FindVSCodePath();
                if (string.IsNullOrEmpty(vsCodePath))
                {
                    throw new InvalidOperationException("VS Code ��������܂���BVS Code ���C���X�g�[������Ă��邱�Ƃ��m�F���Ă��������B");
                }

                // VSCode�ŊJ��
                if (Directory.Exists(path))
                {
                    // �t�H���_�̏ꍇ
                    Process.Start(new ProcessStartInfo(vsCodePath, $"\"{path}\"") { UseShellExecute = true });
                }
                else if (File.Exists(path))
                {
                    // �t�@�C���̏ꍇ
                    Process.Start(new ProcessStartInfo(vsCodePath, $"\"{path}\"") { UseShellExecute = true });
                }
                else
                {
                    // �p�X�����݂��Ȃ��ꍇ�͒ʏ�̋N�����@������
                    LaunchItem(new LauncherItem { Path = path, OpenWithVSCode = false });
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("VS Code �ł̋N���Ɏ��s���܂���: " + ex.Message, ex);
            }
        }

        public void LaunchItemWithOffice(LauncherItem item)
        {
            try
            {
                // �ŏI�A�N�Z�X�������X�V
                item.LastAccessed = DateTime.Now;
                
                string path = item.Path;

                // URL�܂���SharePoint�̏ꍇ
                if (path.StartsWith("http://") || path.StartsWith("https://"))
                {
                    LaunchOfficeUrl(path);
                    return;
                }

                // ���[�J���t�@�C���̏ꍇ
                if (File.Exists(path))
                {
                    LaunchOfficeFile(path);
                    return;
                }

                // �t�@�C�������݂��Ȃ��ꍇ�͒ʏ�̋N�����@������
                LaunchItem(new LauncherItem { Path = path, OpenWithOffice = false });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Office �A�v���ł̋N���Ɏ��s���܂���: " + ex.Message, ex);
            }
        }

        private void LaunchOfficeUrl(string url)
        {
            // OneNote links - launch directly with onenote: protocol
            if (url.StartsWith("onenote:", StringComparison.OrdinalIgnoreCase))
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                return;
            }

            // SharePoint�� Office Online ��URL��Office�A�v���ŊJ��
            string officeUri = "";

            // URL�̎�ނɉ�����Office URI�X�L�[�����g�p
            if (url.Contains("sharepoint.com") || url.Contains("office365.sharepoint.com") ||
                url.Contains("-my.sharepoint.com"))
            {
                // SharePoint�̃t�@�C���̏ꍇ�i�g�D�T�C�g�ƌl�pOneDrive�̗����ɑΉ��j
                if (url.Contains(".one") || url.Contains("notebook") || url.Contains(":o:") ||
                    url.Contains("onenote.aspx") || url.Contains("_layouts/OneNote.aspx"))
                {
                    // OneNote uses onenote:https://... format to open in desktop app
                    officeUri = $"onenote:{url}";
                }
                else if (url.Contains(".xlsx") || url.Contains("workbook") || url.Contains(":x:"))
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
                    // �t�@�C����ނ��s���ȏꍇ�̓u���E�U�ŊJ��
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                    return;
                }
            }
            else if (url.Contains("docs.google.com"))
            {
                // Google Docs�̏ꍇ�͓K�؂�Office�A�v���ŊJ��
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
                // ���̑���URL�̓u���E�U�ŊJ��
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                return;
            }

            // Office URI�X�L�[���ŋN��
            Process.Start(new ProcessStartInfo(officeUri) { UseShellExecute = true });
        }

        private void LaunchOfficeFile(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLower();
            string officeUri = "";

            // �t�@�C���g���q�ɉ�����Office URI�X�L�[�����g�p
            switch (ext)
            {
                case ".one":
                    // OneNote uses onenote:file:/// format
                    officeUri = $"onenote:file:///{filePath.Replace('\\', '/')}";
                    break;
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
                    // Office�t�@�C���łȂ��ꍇ�͒ʏ�̕��@�ŊJ��
                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                    return;
            }

            // Office URI�X�L�[���ŋN��
            Process.Start(new ProcessStartInfo(officeUri) { UseShellExecute = true });
        }

        private string FindVSCodePath()
        {
            // ��ʓI��VS Code�̃C���X�g�[���p�X
            string[] possiblePaths = {
                @"C:\Users\" + Environment.UserName + @"\AppData\Local\Programs\Microsoft VS Code\Code.exe",
                @"C:\Program Files\Microsoft VS Code\Code.exe",
                @"C:\Program Files (x86)\Microsoft VS Code\Code.exe",
                "code" // PATH���ϐ��ɂ���ꍇ
            };

            foreach (string path in possiblePaths)
            {
                if (path == "code")
                {
                    // PATH���ϐ���"code"�R�}���h���g�p�\���`�F�b�N
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
                        // PATH���ϐ���code�R�}���h���Ȃ��ꍇ�͖���
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
                return "�s��";

            if (path.StartsWith("http://") || path.StartsWith("https://") || path.StartsWith("www."))
                return "Web";

            if (Directory.Exists(path))
                return "�t�H���_";

            if (File.Exists(path))
            {
                var ext = Path.GetExtension(path).ToLower();
                return ext switch
                {
                    ".exe" or ".msi" or ".bat" or ".cmd" => "���s�t�@�C��",
                    ".txt" or ".rtf" => "�e�L�X�g",
                    ".doc" or ".docx" => "Word����",
                    ".xls" or ".xlsx" => "Excel����",
                    ".ppt" or ".pptx" => "PowerPoint",
                    ".pdf" => "PDF",
                    ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".svg" or ".webp" => "�摜",
                    ".mp3" or ".wav" or ".wma" or ".flac" or ".aac" or ".ogg" => "���y",
                    ".mp4" or ".avi" or ".mkv" or ".wmv" or ".mov" or ".flv" or ".webm" => "����",
                    ".zip" or ".rar" or ".7z" or ".tar" or ".gz" or ".bz2" => "���k�t�@�C��",
                    ".lnk" => "�V���[�g�J�b�g",
                    ".py" or ".js" or ".html" or ".css" or ".cpp" or ".c" or ".cs" or ".java" or ".php" => "�v���O����",
                    _ => "�t�@�C��"
                };
            }

            return "�R�}���h";
        }

        public string DetectCategory(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    return "���̑�";

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
                        return "Google�h���C�u";
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
                        return "Web�T�C�g";
                }

                if (Directory.Exists(path))
                {
                    return "�t�H���_";
                }

                if (File.Exists(path))
                {
                    var ext = Path.GetExtension(path).ToLower();
                    return ext switch
                    {
                        ".exe" or ".msi" or ".bat" or ".cmd" => "�A�v���P�[�V����",
                        ".txt" or ".rtf" => "�h�L�������g",
                        ".doc" or ".docx" => "Word",
                        ".xls" or ".xlsx" => "Excel",
                        ".ppt" or ".pptx" => "PowerPoint",
                        ".pdf" => "PDF",
                        ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".svg" or ".webp" => "�摜",
                        ".mp3" or ".wav" or ".wma" or ".flac" or ".aac" or ".ogg" => "���y",
                        ".mp4" or ".avi" or ".mkv" or ".wmv" or ".mov" or ".flv" or ".webm" => "����",
                        ".zip" or ".rar" or ".7z" or ".tar" or ".gz" or ".bz2" => "�A�[�J�C�u",
                        ".lnk" => "�V���[�g�J�b�g",
                        ".py" or ".js" or ".html" or ".css" or ".cpp" or ".c" or ".cs" or ".java" or ".php" => "�v���O����",
                        _ => "�t�@�C��"
                    };
                }

                return "�R�}���h";
            }
            catch (Exception)
            {
                return "���̑�";
            }
        }
    }
}