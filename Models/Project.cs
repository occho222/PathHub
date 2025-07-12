using System;
using System.Collections.ObjectModel;

namespace ModernLauncher.Models
{
    public class Project
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public string? ParentId { get; set; } // 階層構造のための親プロジェクトID
        public bool IsFolder { get; set; } // フォルダーかどうかを示すフラグ
        public ObservableCollection<ItemGroup> Groups { get; set; } = new ObservableCollection<ItemGroup>();
        public ObservableCollection<LauncherItem> Items { get; set; } = new ObservableCollection<LauncherItem>();
    }
}