using ModernLauncher.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModernLauncher.Interfaces
{
    public interface ISmartLauncherService
    {
        void RecordPathAccess(string path, string name, string category, string projectName);
        List<PathAccessHistory> GetTodaysOpenedPaths();
        List<PathAccessHistory> GetWeeklyOpenedPaths();
        List<LauncherItem> GetAllProjectItems(IEnumerable<Project> projects);
        List<SmartLauncherItem> GetSmartLauncherItems(IEnumerable<Project> projects);
        void SaveAccessHistory();
        void LoadAccessHistory();
    }
}