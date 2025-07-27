using ModernLauncher.Models;

namespace ModernLauncher.Interfaces
{
    public interface ILauncherService
    {
        void LaunchItem(LauncherItem item);
        void LaunchItemWithVSCode(LauncherItem item);
        void LaunchItemWithOffice(LauncherItem item);
        string GetIconForPath(string path);
        string GetItemType(string path);
        string DetectCategory(string path);
    }
}