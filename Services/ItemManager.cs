using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using ModernLauncher.Interfaces;
using ModernLauncher.Models;
using ModernLauncher.Views;

namespace ModernLauncher.Services
{
    public class ItemManager
    {
        private readonly ILauncherService _launcherService;
        private readonly ISmartLauncherService _smartLauncherService;

        public ItemManager(ILauncherService launcherService, ISmartLauncherService smartLauncherService)
        {
            _launcherService = launcherService;
            _smartLauncherService = smartLauncherService;
        }

        public void AddItemFromPath(Project targetProject, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            try
            {
                if (targetProject.Items.Any(i => i.Path.Equals(path, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show($"\"{path}\" is already added.", "Info",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var (name, description, category) = GenerateItemInfo(path);

                var newItem = new LauncherItem
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = name,
                    Path = path,
                    Description = description,
                    Category = category,
                    GroupIds = new List<string>(),
                    OrderIndex = targetProject.Items.Count,
                    LastAccessed = DateTime.MinValue,
                    ProjectName = targetProject.Name
                };

                newItem.RefreshIconAndType();
                targetProject.Items.Add(newItem);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to add item: {ex.Message}", ex);
            }
        }

        public void ShowAddItemDialogWithPath(Project targetProject, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            try
            {
                if (targetProject.Items.Any(i => i.Path.Equals(path, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show($"\"{path}\" is already added.", "Info",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var dialog = new AddItemDialog(targetProject.Groups.ToList());
                var (name, description, category) = GenerateItemInfo(path);
                dialog.SetInitialValues(name, path, category, description, false);

                if (dialog.ShowDialog() == true && dialog.Result != null)
                {
                    var newItem = dialog.Result;
                    newItem.OrderIndex = targetProject.Items.Count;
                    newItem.ProjectName = targetProject.Name;
                    newItem.RefreshIconAndType();
                    targetProject.Items.Add(newItem);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to add item: {ex.Message}", ex);
            }
        }

        public void ShowAddItemDialog(Project targetProject)
        {
            var dialog = new AddItemDialog(targetProject.Groups.ToList());
            if (dialog.ShowDialog() == true && dialog.Result != null)
            {
                var newItem = dialog.Result;
                newItem.OrderIndex = targetProject.Items.Count;
                newItem.ProjectName = targetProject.Name;
                newItem.RefreshIconAndType();
                targetProject.Items.Add(newItem);
            }
        }

        public void LaunchItem(LauncherItem item)
        {
            try
            {
                RecordItemAccess(item);
                _launcherService.LaunchItem(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LaunchGroup(Project project, ItemGroup group)
        {
            IEnumerable<LauncherItem> itemsToLaunch;
            if (group.Id == "all")
            {
                itemsToLaunch = project.Items;
            }
            else
            {
                itemsToLaunch = project.Items.Where(i => i.GroupIds != null && i.GroupIds.Contains(group.Id));
            }

            var itemList = itemsToLaunch.ToList();

            if (itemList.Count == 0)
            {
                MessageBox.Show($"Group \"{group.Name}\" has no launchable items.", "Info",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Launch {itemList.Count} items from group \"{group.Name}\"?",
                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                int successCount = 0;
                int errorCount = 0;
                var errors = new List<string>();

                foreach (var item in itemList)
                {
                    try
                    {
                        RecordItemAccess(item);
                        _launcherService.LaunchItem(item);
                        successCount++;
                        System.Threading.Thread.Sleep(100);
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        errors.Add($"\"{item.Name}\": {ex.Message}");
                    }
                }

                if (errorCount > 0)
                {
                    var message = $"Group launch completed\\nSuccess: {successCount}\\nFailure: {errorCount}";
                    if (errors.Count > 0 && errors.Count <= 5)
                    {
                        message += "\\n\\nErrors:\\n" + string.Join("\\n", errors);
                    }
                    else if (errors.Count > 5)
                    {
                        message += "\\n\\nErrors:\\n" + string.Join("\\n", errors.Take(5)) + $"\\n...and {errors.Count - 5} more";
                    }

                    MessageBox.Show(message, "Group Launch Result", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        public void EditItem(LauncherItem item, Project project)
        {
            var dialog = new EditItemDialog(item, project.Groups.ToList(), new List<Project> { project }, project);
            if (dialog.ShowDialog() == true && dialog.Result != null)
            {
                var editedItem = dialog.Result;
                editedItem.RefreshIconAndType();

                var index = project.Items.IndexOf(item);
                if (index >= 0)
                {
                    project.Items[index] = editedItem;
                }
            }
        }

        public bool DeleteItem(LauncherItem item, Project project)
        {
            var result = MessageBox.Show($"Delete \"{item.Name}\"?", "Confirm",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                project.Items.Remove(item);
                return true;
            }
            return false;
        }

        public void MoveItemUp(LauncherItem item, Project project)
        {
            var index = project.Items.IndexOf(item);
            if (index > 0)
            {
                project.Items.Move(index, index - 1);
                UpdateItemOrderIndices(project);
            }
        }

        public void MoveItemDown(LauncherItem item, Project project)
        {
            var index = project.Items.IndexOf(item);
            if (index < project.Items.Count - 1)
            {
                project.Items.Move(index, index + 1);
                UpdateItemOrderIndices(project);
            }
        }

        private void UpdateItemOrderIndices(Project project)
        {
            for (int i = 0; i < project.Items.Count; i++)
            {
                project.Items[i].OrderIndex = i;
            }
        }

        private void RecordItemAccess(LauncherItem item)
        {
            try
            {
                var projectName = item.ProjectName ?? "Unknown";
                _smartLauncherService.RecordPathAccess(item.Path, item.Name, item.Category, projectName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error recording item access: {ex.Message}");
            }
        }

        private (string name, string description, string category) GenerateItemInfo(string path)
        {
            string name;
            string description;

            if (IsUrl(path))
            {
                try
                {
                    var uri = new Uri(path);
                    name = uri.Host;
                    if (name.StartsWith("www."))
                    {
                        name = name.Substring(4);
                    }
                    description = $"Web site: {path}";
                }
                catch
                {
                    name = "Web site";
                    description = $"Web site: {path}";
                }
            }
            else if (Directory.Exists(path))
            {
                name = Path.GetFileName(path.TrimEnd('\\', '/'));
                description = $"Folder: {path}";
            }
            else if (File.Exists(path))
            {
                name = Path.GetFileNameWithoutExtension(path);
                description = $"File: {Path.GetFileName(path)}";
            }
            else
            {
                name = Path.GetFileName(path);
                description = $"Added by drag & drop: {name}";
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                name = path;
            }

            string category = DetermineCategory(path);
            return (name, description, category);
        }

        private bool IsUrl(string path)
        {
            return !string.IsNullOrEmpty(path) &&
                   (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
        }

        private string DetermineCategory(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    return "Other";

                if (path.StartsWith("http://") || path.StartsWith("https://") || path.StartsWith("www."))
                {
                    var uri = new Uri(path.StartsWith("www.") ? "http://" + path : path);
                    var host = uri.Host.ToLower();
                    var lowerPath = path.ToLower();

                    // URLデコードして.oneが含まれているか確認（エンコードされた.oneも検出）
                    string decodedPath = Uri.UnescapeDataString(lowerPath);

                    // OneNoteの判定を最優先（.oneが含まれていればOneNote）
                    if (lowerPath.Contains(".one") || decodedPath.Contains(".one"))
                        return "OneNote";

                    if (host.Contains("github.com"))
                        return "GitHubURL";
                    else if (host.Contains("gitlab.com") || lowerPath.Contains("gitlab"))
                        return "GitLabURL";
                    else if (lowerPath.Contains("redmine"))
                        return "RedmineURL";
                    else if (host.Contains("drive.google.com") || host.Contains("docs.google.com"))
                        return "Google Drive";
                    else if (host.Contains("teams.microsoft.com") || host.Contains("teams.live.com"))
                        return "MicrosoftTeams";
                    else if (lowerPath.Contains("onenote") ||
                             (host.Contains("sharepoint.com") && lowerPath.Contains("onenote.aspx")))
                        return "OneNote";
                    else if (host.Contains("sharepoint.com") || host.Contains(".sharepoint.com") ||
                             host.EndsWith("sharepoint.com") || host.Contains("office365.sharepoint.com"))
                        return "SharePoint";
                    else if (host.Contains("outlook.office365.com") || host.Contains("outlook.office.com") ||
                             host.Contains("onedrive.live.com") || host.Contains("1drv.ms"))
                        return "OneDrive";
                    else
                        return "Web site";
                }

                if (Directory.Exists(path))
                {
                    return "Folder";
                }

                if (File.Exists(path))
                {
                    var ext = Path.GetExtension(path).ToLower();
                    return ext switch
                    {
                        ".exe" or ".msi" or ".bat" or ".cmd" => "Application",
                        ".txt" or ".rtf" => "Document",
                        ".doc" or ".docx" => "Word",
                        ".xls" or ".xlsx" => "Excel",
                        ".ppt" or ".pptx" => "PowerPoint",
                        ".pdf" => "PDF",
                        ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".svg" or ".webp" => "Image",
                        ".mp3" or ".wav" or ".wma" or ".flac" or ".aac" or ".ogg" => "Music",
                        ".mp4" or ".avi" or ".mkv" or ".wmv" or ".mov" or ".flv" or ".webm" => "Video",
                        ".zip" or ".rar" or ".7z" or ".tar" or ".gz" or ".bz2" => "Archive",
                        ".lnk" => "Shortcut",
                        ".py" or ".js" or ".html" or ".css" or ".cpp" or ".c" or ".cs" or ".java" or ".php" => "Program",
                        _ => "File"
                    };
                }

                return "Command";
            }
            catch (Exception)
            {
                return "Other";
            }
        }
    }
}
