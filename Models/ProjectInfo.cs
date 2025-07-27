using System;

namespace ModernLauncher.Models
{
    public class ProjectInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public string? ParentId { get; set; } // 階層構造のための親プロジェクトID
        public bool IsFolder { get; set; } // フォルダーかどうかを示すフラグ
    }
}