using ModernLauncher.Interfaces;
using ModernLauncher.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ModernLauncher.Services
{
    public class SmartLauncherService : ISmartLauncherService
    {
        private readonly string accessHistoryFile;
        private readonly ILauncherService launcherService;
        private List<PathAccessHistory> accessHistory = new List<PathAccessHistory>();

        public SmartLauncherService(ILauncherService launcherService)
        {
            this.launcherService = launcherService;

            // ユーザーフォルダ（AppData\Roaming\NicoPath）を使用
            var appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "NicoPath");

            accessHistoryFile = Path.Combine(appDataFolder, "pathAccessHistory.json");

            // フォルダが存在しない場合は作成
            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }

            LoadAccessHistory();
        }

        public void RecordPathAccess(string path, string name, string category, string projectName)
        {
            try
            {
                var existingRecord = accessHistory.FirstOrDefault(h => h.Path.Equals(path, StringComparison.OrdinalIgnoreCase));
                
                if (existingRecord != null)
                {
                    existingRecord.LastAccessTime = DateTime.Now;
                    existingRecord.AccessCount++;
                    existingRecord.Name = name; // Update name in case it changed
                    existingRecord.Category = category;
                    existingRecord.ProjectName = projectName;
                }
                else
                {
                    var newRecord = new PathAccessHistory
                    {
                        Path = path,
                        Name = name,
                        LastAccessTime = DateTime.Now,
                        AccessCount = 1,
                        Category = category,
                        ProjectName = projectName,
                        Icon = launcherService.GetIconForPath(path),
                        ItemType = launcherService.GetItemType(path)
                    };
                    accessHistory.Add(newRecord);
                }

                SaveAccessHistory();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error recording path access: {ex.Message}");
            }
        }

        public List<PathAccessHistory> GetTodaysOpenedPaths()
        {
            var today = DateTime.Today;
            return accessHistory
                .Where(h => h.LastAccessTime.Date == today)
                .OrderByDescending(h => h.LastAccessTime)
                .ThenByDescending(h => h.AccessCount)
                .Take(50) // Limit to 50 most recent
                .ToList();
        }

        public List<PathAccessHistory> GetWeeklyOpenedPaths()
        {
            var weekAgo = DateTime.Now.AddDays(-7);
            return accessHistory
                .Where(h => h.LastAccessTime >= weekAgo)
                .OrderByDescending(h => h.LastAccessTime)
                .ThenByDescending(h => h.AccessCount)
                .Take(100) // Limit to 100 most recent
                .ToList();
        }

        public List<PathAccessHistory> GetRecentlyUsedPaths()
        {
            return accessHistory
                .OrderByDescending(h => h.LastAccessTime)
                .ThenByDescending(h => h.AccessCount)
                .Take(30) // Limit to 30 most recently used
                .ToList();
        }

        public List<LauncherItem> GetAllItemsSortedByRecentUsage(IEnumerable<Project> projects)
        {
            var allProjectItems = GetAllProjectItems(projects);
            var accessLookup = accessHistory.ToDictionary(h => h.Path, h => h);

            return allProjectItems
                .Select(item => new
                {
                    Item = item,
                    LastAccess = accessLookup.ContainsKey(item.Path) ? accessLookup[item.Path].LastAccessTime : DateTime.MinValue,
                    AccessCount = accessLookup.ContainsKey(item.Path) ? accessLookup[item.Path].AccessCount : 0
                })
                .OrderByDescending(x => x.LastAccess)
                .ThenByDescending(x => x.AccessCount)
                .ThenBy(x => x.Item.ProjectName)
                .ThenBy(x => x.Item.Name)
                .Select(x => x.Item)
                .ToList();
        }

        public List<LauncherItem> GetAllProjectItems(IEnumerable<Project> projects)
        {
            var allItems = new List<LauncherItem>();
            
            foreach (var project in projects.Where(p => !p.IsFolder))
            {
                foreach (var item in project.Items)
                {
                    // Create a copy with project information
                    var itemCopy = new LauncherItem
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Path = item.Path,
                        Description = item.Description,
                        Category = item.Category,
                        GroupIds = item.GroupIds,
                        Icon = item.Icon,
                        ItemType = item.ItemType,
                        OrderIndex = item.OrderIndex,
                        GroupNames = item.GroupNames,
                        ProjectName = project.Name,
                        FolderPath = GetProjectFolderPath(project, projects)
                    };
                    allItems.Add(itemCopy);
                }
            }

            return allItems.OrderBy(i => i.ProjectName).ThenBy(i => i.OrderIndex).ToList();
        }

        public List<SmartLauncherItem> GetSmartLauncherItems(IEnumerable<Project> projects)
        {
            var smartItems = new List<SmartLauncherItem>();

            // 最近使った項目（最上位に配置）- 全ての項目を最近使った順でソート
            var allItemsSortedByUsage = GetAllItemsSortedByRecentUsage(projects);
            smartItems.Add(new SmartLauncherItem
            {
                Id = "recently-used",
                DisplayName = "最近使った",
                Icon = "⏱️",
                ItemType = SmartLauncherItemType.RecentlyUsed,
                Items = allItemsSortedByUsage,
                ItemCount = allItemsSortedByUsage.Count
            });

            // すべてのプロジェクト
            var allProjectItems = GetAllProjectItems(projects);
            smartItems.Add(new SmartLauncherItem
            {
                Id = "all-projects",
                DisplayName = "すべてのプロジェクト",
                Icon = "🗂️",
                ItemType = SmartLauncherItemType.AllProjects,
                Items = allProjectItems,
                ItemCount = allProjectItems.Count
            });

            // 今日開いたパス
            var todaysItems = GetTodaysOpenedPaths();
            var todaysLauncherItems = ConvertToLauncherItems(todaysItems);
            smartItems.Add(new SmartLauncherItem
            {
                Id = "todays-opened",
                DisplayName = "今日開いたパス",
                Icon = "📅",
                ItemType = SmartLauncherItemType.TodaysOpenedPaths,
                Items = todaysLauncherItems,
                ItemCount = todaysLauncherItems.Count
            });

            // 週間内に開いたパス
            var weeklyItems = GetWeeklyOpenedPaths();
            var weeklyLauncherItems = ConvertToLauncherItems(weeklyItems);
            smartItems.Add(new SmartLauncherItem
            {
                Id = "weekly-opened",
                DisplayName = "週間内に開いたパス",
                Icon = "📆",
                ItemType = SmartLauncherItemType.WeeklyOpenedPaths,
                Items = weeklyLauncherItems,
                ItemCount = weeklyLauncherItems.Count
            });

            return smartItems;
        }

        public void SaveAccessHistory()
        {
            try
            {
                // Keep only last 500 records to prevent file from growing too large
                var recordsToKeep = accessHistory
                    .OrderByDescending(h => h.LastAccessTime)
                    .Take(500)
                    .ToList();

                accessHistory = recordsToKeep;

                var json = JsonConvert.SerializeObject(accessHistory, Formatting.Indented);
                File.WriteAllText(accessHistoryFile, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving access history: {ex.Message}");
            }
        }

        public void LoadAccessHistory()
        {
            try
            {
                if (File.Exists(accessHistoryFile))
                {
                    var json = File.ReadAllText(accessHistoryFile, Encoding.UTF8);
                    accessHistory = JsonConvert.DeserializeObject<List<PathAccessHistory>>(json) ?? new List<PathAccessHistory>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading access history: {ex.Message}");
                accessHistory = new List<PathAccessHistory>();
            }
        }

        private List<LauncherItem> ConvertToLauncherItems(List<PathAccessHistory> historyItems)
        {
            return historyItems.Select(h => new LauncherItem
            {
                Id = Guid.NewGuid().ToString(),
                Name = h.Name,
                Path = h.Path,
                Description = $"Last accessed: {h.LastAccessTime:yyyy-MM-dd HH:mm} ({h.AccessCount} times)",
                Category = h.Category,
                GroupIds = new List<string>(),
                Icon = h.Icon,
                ItemType = h.ItemType,
                OrderIndex = 0,
                GroupNames = "",
                ProjectName = h.ProjectName,
                FolderPath = ""
            }).ToList();
        }

        private string GetProjectFolderPath(Project project, IEnumerable<Project> allProjects)
        {
            var pathParts = new List<string>();
            var currentProject = project;
            
            while (!string.IsNullOrEmpty(currentProject.ParentId))
            {
                var parentProject = allProjects.FirstOrDefault(p => p.Id == currentProject.ParentId);
                if (parentProject != null)
                {
                    pathParts.Insert(0, parentProject.Name);
                    currentProject = parentProject;
                }
                else
                {
                    break;
                }
            }
            
            return pathParts.Count > 0 ? string.Join(" > ", pathParts) : "Root";
        }
    }
}