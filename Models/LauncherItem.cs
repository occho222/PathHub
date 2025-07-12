using System;
using System.Collections.Generic;

namespace ModernLauncher.Models
{
    public class LauncherItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty; // 説明
        public string Category { get; set; } = "その他"; // 新規追加：分類フィールド
        [Obsolete("Use GroupIds instead")]
        public string GroupId { get; set; } = string.Empty; // 後方互換性のため残す
        public List<string> GroupIds { get; set; } = new List<string>();
        public string Icon { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty; // Web, Folder, File, Command
        public int OrderIndex { get; set; } // 並び順
        public string GroupNames { get; set; } = string.Empty; // 表示用のグループ名リスト
    }
}