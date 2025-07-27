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
                // �����̃A�C�e���œ����p�X���Ȃ����`�F�b�N
                if (targetProject.Items.Any(i => i.Path.Equals(path, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show($"�u{path}�v�͊��ɒǉ�����Ă��܂��B", "���",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // �t�@�C�������疼�O�𐶐�
                var (name, description, category) = GenerateItemInfo(path);

                // �V�����A�C�e�����쐬
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

                // �A�C�R���ƃ^�C�v��ݒ�
                newItem.RefreshIconAndType();

                // �v���W�F�N�g�ɒǉ�
                targetProject.Items.Add(newItem);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"�A�C�e���̒ǉ��Ɏ��s���܂���: {ex.Message}", ex);
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
                // �����̃A�C�e���œ����p�X���Ȃ����`�F�b�N
                if (targetProject.Items.Any(i => i.Path.Equals(path, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show($"�u{path}�v�͊��ɒǉ�����Ă��܂��B", "���",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // AddItemDialog�Ɏ��O����ݒ肵�ĕ\��
                var dialog = new AddItemDialog(targetProject.Groups.ToList());
                
                // �t�@�C�������疼�O�𐶐�
                var (name, description, category) = GenerateItemInfo(path);

                // �_�C�A���O�ɏ����l��ݒ�
                dialog.SetInitialValues(name, path, category, description, false);

                if (dialog.ShowDialog() == true && dialog.Result != null)
                {
                    var newItem = dialog.Result;
                    newItem.OrderIndex = targetProject.Items.Count;
                    newItem.ProjectName = targetProject.Name;
                    
                    // �A�C�R���ƃ^�C�v��ݒ�
                    newItem.RefreshIconAndType();
                    
                    targetProject.Items.Add(newItem);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"�A�C�e���̒ǉ��Ɏ��s���܂���: {ex.Message}", ex);
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
                
                // �A�C�R���ƃ^�C�v��ݒ�
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
                MessageBox.Show(ex.Message, "�G���[", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LaunchGroup(Project project, ItemGroup group)
        {
            // �u���ׂāv�O���[�v���I�����ꂽ�ꍇ
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
                MessageBox.Show($"�O���[�v�u{group.Name}�v�ɂ͋N���\�ȃA�C�e��������܂���", "���", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // �m�F���b�Z�[�W��\��
            var result = MessageBox.Show($"�O���[�v�u{group.Name}�v��{itemList.Count}�̃A�C�e�����ꊇ�N�����܂����H", 
                "�m�F", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
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
                        
                        // �e�A�C�e���̋N���Ԋu��݂���
                        System.Threading.Thread.Sleep(100);
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        errors.Add($"�u{item.Name}�v: {ex.Message}");
                    }
                }

                // ���ʃ��b�Z�[�W��Ԃ�
                if (errorCount > 0)
                {
                    var message = $"�O���[�v�ꊇ�N������\n����: {successCount}��\n���s: {errorCount}��";
                    if (errors.Count > 0 && errors.Count <= 5)
                    {
                        message += "\n\n�G���[�ڍ�:\n" + string.Join("\n", errors);
                    }
                    else if (errors.Count > 5)
                    {
                        message += "\n\n�G���[�ڍ�:\n" + string.Join("\n", errors.Take(5)) + $"\n...��{errors.Count - 5}��";
                    }
                    
                    MessageBox.Show(message, "�ꊇ�N������", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        public void EditItem(LauncherItem item, Project project)
        {
            var dialog = new EditItemDialog(item, project.Groups.ToList());
            if (dialog.ShowDialog() == true && dialog.Result != null)
            {
                var editedItem = dialog.Result;
                
                // �A�C�R���ƃ^�C�v��ݒ�
                editedItem.RefreshIconAndType();
                
                // ���̃A�C�e���ƒu������
                var index = project.Items.IndexOf(item);
                if (index >= 0)
                {
                    project.Items[index] = editedItem;
                }
            }
        }

        public bool DeleteItem(LauncherItem item, Project project)
        {
            var result = MessageBox.Show($"�u{item.Name}�v���폜���܂����H", "�m�F",
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
                // URL�̏ꍇ�̓h���C�����𖼑O�Ƃ��Ďg�p
                try
                {
                    var uri = new Uri(path);
                    name = uri.Host;
                    if (name.StartsWith("www."))
                    {
                        name = name.Substring(4);
                    }
                    description = $"Web�T�C�g: {path}";
                }
                catch
                {
                    name = "Web�T�C�g";
                    description = $"Web�T�C�g: {path}";
                }
            }
            else if (Directory.Exists(path))
            {
                name = Path.GetFileName(path.TrimEnd('\\', '/'));
                description = $"�t�H���_: {path}";
            }
            else if (File.Exists(path))
            {
                name = Path.GetFileNameWithoutExtension(path);
                description = $"�t�@�C��: {Path.GetFileName(path)}";
            }
            else
            {
                name = Path.GetFileName(path);
                description = $"�h���b�O&�h���b�v�Œǉ�: {name}";
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                name = path;
            }

            // ���ނ���������
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
                    return "���̑�";

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
                        return "Google�h���C�u";
                    else if (host.Contains("teams.microsoft.com") || host.Contains("teams.live.com"))
                        return "MicrosoftTeams";
                    else if (host.Contains("sharepoint.com") || host.Contains(".sharepoint.com") || 
                             host.EndsWith("sharepoint.com") || host.Contains("office365.sharepoint.com"))
                        return "SharePoint";
                    else if (host.Contains("outlook.office365.com") || host.Contains("outlook.office.com") ||
                             host.Contains("onedrive.live.com") || host.Contains("1drv.ms"))
                        return "OneDrive";
                    else
                        return "Web�T�C�g";
                }

                if (Directory.Exists(path))
                {
                    return "�t�H���_";
                }

                if (File.Exists(path))
                {
                    var ext = Path.GetExtension(path).ToLower();
                    return ext switch
                    {
                        ".exe" or ".msi" or ".bat" or ".cmd" => "�A�v���P�[�V����",
                        ".txt" or ".rtf" => "�h�L�������g",
                        ".doc" or ".docx" => "Word",
                        ".xls" or ".xlsx" => "Excel",
                        ".ppt" or ".pptx" => "PowerPoint",
                        ".pdf" => "PDF",
                        ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".svg" or ".webp" => "�摜",
                        ".mp3" or ".wav" or ".wma" or ".flac" or ".aac" or ".ogg" => "���y",
                        ".mp4" or ".avi" or ".mkv" or ".wmv" or ".mov" or ".flv" or ".webm" => "����",
                        ".zip" or ".rar" or ".7z" or ".tar" or ".gz" or ".bz2" => "�A�[�J�C�u",
                        ".lnk" => "�V���[�g�J�b�g",
                        ".py" or ".js" or ".html" or ".css" or ".cpp" or ".c" or ".cs" or ".java" or ".php" => "�v���O����",
                        _ => "�t�@�C��"
                    };
                }

                return "�R�}���h";
            }
            catch (Exception)
            {
                return "���̑�";
            }
        }
    }
}