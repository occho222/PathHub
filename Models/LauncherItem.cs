using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ModernLauncher.Services;

namespace ModernLauncher.Models
{
    public class LauncherItem : INotifyPropertyChanged
    {
        private string id = string.Empty;
        private string name = string.Empty;
        private string path = string.Empty;
        private string description = string.Empty;
        private string category = "Other";
        private string groupId = string.Empty;
        private List<string> groupIds = new List<string>();
        private string icon = string.Empty;
        private string itemType = string.Empty;
        private int orderIndex;
        private string groupNames = string.Empty;
        private DateTime lastAccessed = DateTime.MinValue;
        private bool openWithVSCode = false;
        private bool openWithOffice = false;

        // 新しいフィールド - フォルダパス表示用
        private string projectName = string.Empty;
        private string folderPath = string.Empty;

        public string Id
        {
            get => id;
            set => SetProperty(ref id, value);
        }

        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        public string Path
        {
            get => path;
            set
            {
                if (SetProperty(ref path, value))
                {
                    // パスが変更された時にアイコンとアイテムタイプを更新
                    UpdateIconAndType();
                }
            }
        }

        public string Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }

        public string Category
        {
            get => category;
            set => SetProperty(ref category, value);
        }

        [Obsolete("Use GroupIds instead")]
        public string GroupId
        {
            get => groupId;
            set => SetProperty(ref groupId, value);
        }

        public List<string> GroupIds
        {
            get => groupIds;
            set => SetProperty(ref groupIds, value);
        }

        public string Icon
        {
            get => icon;
            set => SetProperty(ref icon, value);
        }

        public string ItemType
        {
            get => itemType;
            set => SetProperty(ref itemType, value);
        }

        public int OrderIndex
        {
            get => orderIndex;
            set => SetProperty(ref orderIndex, value);
        }

        public string GroupNames
        {
            get => groupNames;
            set => SetProperty(ref groupNames, value);
        }

        // 新しいプロパティ - 最終アクセス日時
        public DateTime LastAccessed
        {
            get => lastAccessed;
            set => SetProperty(ref lastAccessed, value);
        }

        // 新しいプロパティ - 所属プロジェクト名
        public string ProjectName
        {
            get => projectName;
            set => SetProperty(ref projectName, value);
        }

        // 新しいプロパティ - フォルダパス
        public string FolderPath
        {
            get => folderPath;
            set => SetProperty(ref folderPath, value);
        }

        // 新しいプロパティ - VSCodeで開くオプション
        public bool OpenWithVSCode
        {
            get => openWithVSCode;
            set => SetProperty(ref openWithVSCode, value);
        }

        // 新しいプロパティ - Officeアプリで開くオプション
        public bool OpenWithOffice
        {
            get => openWithOffice;
            set => SetProperty(ref openWithOffice, value);
        }

        private void UpdateIconAndType()
        {
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    var launcherService = ServiceLocator.Instance.GetService<Interfaces.ILauncherService>();
                    if (launcherService != null)
                    {
                        Icon = launcherService.GetIconForPath(path);
                        ItemType = launcherService.GetItemType(path);
                    }
                    else
                    {
                        // フォールバック: 基本的なアイコン判定
                        Icon = GetFallbackIcon(path);
                        ItemType = GetFallbackItemType(path);
                    }
                }
                catch
                {
                    // エラーが発生した場合のフォールバック
                    Icon = GetFallbackIcon(path);
                    ItemType = GetFallbackItemType(path);
                }
            }
            else
            {
                Icon = "?";
                ItemType = "不明";
            }
        }

        private string GetFallbackIcon(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "❓";

            // URLの場合
            if (path.StartsWith("http://") || path.StartsWith("https://") || path.StartsWith("www."))
                return "🌐";

            // ディレクトリの場合
            if (System.IO.Directory.Exists(path))
                return "📁";

            // ファイルの場合
            if (System.IO.File.Exists(path))
            {
                var ext = System.IO.Path.GetExtension(path).ToLower();
                return ext switch
                {
                    ".exe" or ".msi" or ".bat" or ".cmd" => "⚙️",
                    ".txt" or ".rtf" => "📝",
                    ".doc" or ".docx" => "📄",
                    ".xls" or ".xlsx" => "📊",
                    ".ppt" or ".pptx" => "📊",
                    ".pdf" => "📄",
                    ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".svg" or ".webp" => "🖼️",
                    _ => "📄"
                };
            }

            // コマンドの場合
            return "⚡";
        }

        private string GetFallbackItemType(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "不明";

            // URLの場合
            if (path.StartsWith("http://") || path.StartsWith("https://") || path.StartsWith("www."))
                return "Web";

            // ディレクトリの場合
            if (System.IO.Directory.Exists(path))
            {
                // G:ドライブの場合はGoogleドライブ
                if (path.StartsWith("G:", StringComparison.OrdinalIgnoreCase) || 
                    path.StartsWith("G\\", StringComparison.OrdinalIgnoreCase))
                {
                    return "Googleドライブ";
                }
                return "フォルダ";
            }

            // ファイルの場合
            if (System.IO.File.Exists(path))
            {
                // G:ドライブのファイルもGoogleドライブ
                if (path.StartsWith("G:", StringComparison.OrdinalIgnoreCase) || 
                    path.StartsWith("G\\", StringComparison.OrdinalIgnoreCase))
                {
                    return "Googleドライブ";
                }

                var ext = System.IO.Path.GetExtension(path).ToLower();
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

            // G:で始まる存在しないパスもGoogleドライブ
            if (path.StartsWith("G:", StringComparison.OrdinalIgnoreCase) || 
                path.StartsWith("G\\", StringComparison.OrdinalIgnoreCase))
            {
                return "Googleドライブ";
            }

            // コマンドの場合
            return "コマンド";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// アイコンとアイテムタイプを強制的に更新します
        /// </summary>
        public void RefreshIconAndType()
        {
            UpdateIconAndType();
        }
    }
}