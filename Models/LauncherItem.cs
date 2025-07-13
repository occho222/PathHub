using System;
using System.Collections.Generic;

namespace ModernLauncher.Models
{
    public class LauncherItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty; // Description
        public string Category { get; set; } = "Other"; // Newly added: Category field
        [Obsolete("Use GroupIds instead")]
        public string GroupId { get; set; } = string.Empty; // Retained for backward compatibility
        public List<string> GroupIds { get; set; } = new List<string>();
        public string Icon { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty; // Web, Folder, File, Command
        public int OrderIndex { get; set; } // Order index
        public string GroupNames { get; set; } = string.Empty; // Display group name list
    }
}