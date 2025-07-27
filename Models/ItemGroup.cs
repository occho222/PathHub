namespace ModernLauncher.Models
{
    public class ItemGroup
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ItemCount { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
    }
}