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
                // 既存のアイテムで同じパスがないかチェック
                if (targetProject.Items.Any(i => i.Path.Equals(path, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show($"「{path}」は既に追加されています。", "情報",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // ファイル名から名前を生成
                var (name, description, category) = GenerateItemInfo(path);

                // 新しいアイテムを作成
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

                // アイコンとタイプを設定
                newItem.RefreshIconAndType();

                // プロジェクトに追加
                targetProject.Items.Add(newItem);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"アイテムの追加に失敗しました: {ex.Message}", ex);
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
                // 既存のアイテムで同じパスがないかチェック
                if (targetProject.Items.Any(i => i.Path.Equals(path, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show($"「{path}」は既に追加されています。", "情報",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // AddItemDialogに事前情報を設定して表示
                var dialog = new AddItemDialog(targetProject.Groups.ToList());
                
                // ファイル名から名前を生成
                var (name, description, category) = GenerateItemInfo(path);

                // ダイアログに初期値を設定
                dialog.SetInitialValues(name, path, category, description, false);

                if (dialog.ShowDialog() == true && dialog.Result != null)
                {
                    var newItem = dialog.Result;
                    newItem.OrderIndex = targetProject.Items.Count;
                    newItem.ProjectName = targetProject.Name;
                    
                    // アイコンとタイプを設定
                    newItem.RefreshIconAndType();
                    
                    targetProject.Items.Add(newItem);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"アイテムの追加に失敗しました: {ex.Message}", ex);
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
                
                // アイコンとタイプを設定
                newItem.RefreshIconAndType();
                
                targetProject.Items.Add(newItem);
            }
        }

        public void LaunchItem(LauncherItem item)
        {
            try
            {
                // Record the access before launching
                RecordItemAccess(item);
                
                _launcherService.LaunchItem(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LaunchGroup(Project project, ItemGroup group)
        {
            // 「すべて」グループが選択された場合
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
                MessageBox.Show($"グループ「{group.Name}」には起動可能なアイテムがありません", "情報", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 確認メッセージを表示
            var result = MessageBox.Show($"グループ「{group.Name}」の{itemList.Count}個のアイテムを一括起動しますか？", 
                "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                int successCount = 0;
                int errorCount = 0;
                var errors = new List<string>();

                foreach (var item in itemList)
                {
                    try
                    {
                        // Record the access before launching
                        RecordItemAccess(item);
                        
                        _launcherService.LaunchItem(item);
                        successCount++;
                        
                        // 各アイテムの起動間隔を設ける
                        System.Threading.Thread.Sleep(100);
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        errors.Add($"「{item.Name}」: {ex.Message}");
                    }
                }

                // 結果メッセージを返す
                if (errorCount > 0)
                {
                    var message = $"グループ一括起動完了\n成功: {successCount}個\n失敗: {errorCount}個";
                    if (errors.Count > 0 && errors.Count <= 5)
                    {
                        message += "\n\nエラー詳細:\n" + string.Join("\n", errors);
                    }
                    else if (errors.Count > 5)
                    {
                        message += "\n\nエラー詳細:\n" + string.Join("\n", errors.Take(5)) + $"\n...他{errors.Count - 5}件";
                    }
                    
                    MessageBox.Show(message, "一括起動結果", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        public void EditItem(LauncherItem item, Project project)
        {
            var dialog = new EditItemDialog(item, project.Groups.ToList());
            if (dialog.ShowDialog() == true && dialog.Result != null)
            {
                var editedItem = dialog.Result;
                
                // アイコンとタイプを設定
                editedItem.RefreshIconAndType();
                
                // 元のアイテムと置き換え
                var index = project.Items.IndexOf(item);
                if (index >= 0)
                {
                    project.Items[index] = editedItem;
                }
            }
        }

        public bool DeleteItem(LauncherItem item, Project project)
        {
            var result = MessageBox.Show($"「{item.Name}」を削除しますか？", "確認",
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
                // URLの場合はドメイン名を名前として使用
                try
                {
                    var uri = new Uri(path);
                    name = uri.Host;
                    if (name.StartsWith("www."))
                    {
                        name = name.Substring(4);
                    }
                    description = $"Webサイト: {path}";
                }
                catch
                {
                    name = "Webサイト";
                    description = $"Webサイト: {path}";
                }
            }
            else if (Directory.Exists(path))
            {
                name = Path.GetFileName(path.TrimEnd('\\', '/'));
                description = $"フォルダ: {path}";
            }
            else if (File.Exists(path))
            {
                name = Path.GetFileNameWithoutExtension(path);
                description = $"ファイル: {Path.GetFileName(path)}";
            }
            else
            {
                name = Path.GetFileName(path);
                description = $"ドラッグ&ドロップで追加: {name}";
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                name = path;
            }

            // 分類を自動判定
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
                    return "その他";

                if (path.StartsWith("http://") || path.StartsWith("https://") || path.StartsWith("www."))
                {
                    var uri = new Uri(path.StartsWith("www.") ? "http://" + path : path);
                    var host = uri.Host.ToLower();
                    var lowerPath = path.ToLower();
                    
                    if (host.Contains("github.com"))
                        return "GitHubURL";
                    else if (host.Contains("gitlab.com") || lowerPath.Contains("gitlab"))
                        return "GitLabURL";
                    else if (lowerPath.Contains("redmine"))
                        return "RedmineURL";
                    else if (host.Contains("drive.google.com") || host.Contains("docs.google.com"))
                        return "Googleドライブ";
                    else if (host.Contains("teams.microsoft.com") || host.Contains("teams.live.com"))
                        return "MicrosoftTeams";
                    else if (host.Contains("sharepoint.com") || host.Contains(".sharepoint.com") || 
                             host.EndsWith("sharepoint.com") || host.Contains("office365.sharepoint.com"))
                        return "SharePoint";
                    else if (host.Contains("outlook.office365.com") || host.Contains("outlook.office.com") ||
                             host.Contains("onedrive.live.com") || host.Contains("1drv.ms"))
                        return "OneDrive";
                    else
                        return "Webサイト";
                }

                if (Directory.Exists(path))
                {
                    return "フォルダ";
                }

                if (File.Exists(path))
                {
                    var ext = Path.GetExtension(path).ToLower();
                    return ext switch
                    {
                        ".exe" or ".msi" or ".bat" or ".cmd" => "アプリケーション",
                        ".txt" or ".rtf" => "ドキュメント",
                        ".doc" or ".docx" => "Word",
                        ".xls" or ".xlsx" => "Excel",
                        ".ppt" or ".pptx" => "PowerPoint",
                        ".pdf" => "PDF",
                        ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".svg" or ".webp" => "画像",
                        ".mp3" or ".wav" or ".wma" or ".flac" or ".aac" or ".ogg" => "音楽",
                        ".mp4" or ".avi" or ".mkv" or ".wmv" or ".mov" or ".flv" or ".webm" => "動画",
                        ".zip" or ".rar" or ".7z" or ".tar" or ".gz" or ".bz2" => "アーカイブ",
                        ".lnk" => "ショートカット",
                        ".py" or ".js" or ".html" or ".css" or ".cpp" or ".c" or ".cs" or ".java" or ".php" => "プログラム",
                        _ => "ファイル"
                    };
                }

                return "コマンド";
            }
            catch (Exception)
            {
                return "その他";
            }
        }
    }
}