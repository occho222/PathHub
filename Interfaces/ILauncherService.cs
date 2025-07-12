using ModernLauncher.Models;

namespace ModernLauncher.Interfaces
{
    public interface ILauncherService
    {
        void LaunchItem(LauncherItem item);
        string GetIconForPath(string path);
        string GetItemType(string path);
    }
}