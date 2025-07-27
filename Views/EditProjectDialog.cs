using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ModernLauncher.Models;

namespace ModernLauncher.Views
{
    /// <summary>
    /// EditProjectDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class EditProjectDialog : Window
    {
        public string ProjectName { get; private set; } = string.Empty;
        public ProjectNode? SelectedFolder { get; private set; }
        
        private readonly ProjectNode? _currentProject;
        private readonly List<ProjectNode> _availableFolders;
        private string _originalName;

        public EditProjectDialog(ProjectNode currentProject, List<ProjectNode> availableFolders)
        {
            InitializeComponent();
            
            _currentProject = currentProject;
            _availableFolders = availableFolders ?? throw new ArgumentNullException(nameof(availableFolders));
            _originalName = currentProject.Name;
            
            InitializeFields();
            PopulateFolders();
        }

        private void InitializeFields()
        {
            if (_currentProject != null)
            {
                ProjectNameTextBox.Text = _currentProject.Name;
                var itemType = _currentProject.IsFolder ? "フォルダー" : "プロジェクト";
                Title = $"{itemType}編集 - {_currentProject.Name}";
                
                // Update labels dynamically
                var headerBlock = (TextBlock)this.FindName("HeaderTextBlock");
                if (headerBlock != null)
                {
                    headerBlock.Text = $"{itemType}編集";
                }
                
                var subtitleBlock = (TextBlock)this.FindName("SubtitleTextBlock");
                if (subtitleBlock != null)
                {
                    subtitleBlock.Text = $"{itemType}の詳細を編集します。名前を変更し、別のフォルダに移動できます。";
                }
                
                var nameLabel = (TextBlock)this.FindName("NameLabel");
                if (nameLabel != null)
                {
                    nameLabel.Text = $"{itemType}名";
                }
                
                var updateButton = (Button)this.FindName("UpdateButton");
                if (updateButton != null)
                {
                    updateButton.Content = $"{itemType}を更新";
                }
            }
            
            // Focus on the project name textbox
            ProjectNameTextBox.Focus();
            ProjectNameTextBox.SelectAll();
        }

        private void PopulateFolders()
        {
            try
            {
                FolderComboBox.Items.Clear();
                
                // Add "Root" option
                var rootOption = new { DisplayName = "📁 [Root] - プロジェクトをルートレベルに配置", Node = (ProjectNode?)null };
                FolderComboBox.Items.Add(rootOption);
                
                // Filter out the current project and its descendants to prevent circular references
                var availableFolders = _availableFolders.Where(f => f.IsFolder && f.Id != _currentProject?.Id).ToList();
                
                // Remove descendants of the current project
                if (_currentProject != null)
                {
                    availableFolders = availableFolders.Where(f => !IsDescendantOf(f, _currentProject)).ToList();
                }
                
                // Get flattened folder structure
                var flattenedFolders = GetFlattenedFolders(availableFolders);
                
                foreach (var folderInfo in flattenedFolders)
                {
                    var indent = new string(' ', folderInfo.Level * 2);
                    var displayItem = new { DisplayName = $"📁 {indent}{folderInfo.Node.Name}", Node = folderInfo.Node };
                    FolderComboBox.Items.Add(displayItem);
                }

                // Select the current parent folder
                if (_currentProject?.ParentId != null)
                {
                    var currentParent = _availableFolders.FirstOrDefault(f => f.Id == _currentProject.ParentId);
                    if (currentParent != null)
                    {
                        var matchingItem = FolderComboBox.Items.Cast<object>()
                            .FirstOrDefault(item => ((dynamic)item).Node?.Id == currentParent.Id);
                        if (matchingItem != null)
                        {
                            FolderComboBox.SelectedItem = matchingItem;
                        }
                    }
                }
                else
                {
                    // Select root if no parent
                    FolderComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PopulateFolders error: {ex.Message}");
                
                // エラーが発生した場合は最低限のオプションを設定
                FolderComboBox.Items.Clear();
                var rootOption = new { DisplayName = "📁 [Root] - Create at top level", Node = (ProjectNode?)null };
                FolderComboBox.Items.Add(rootOption);
                FolderComboBox.SelectedIndex = 0;
            }
        }

        private bool IsDescendantOf(ProjectNode node, ProjectNode potentialAncestor)
        {
            var current = node;
            while (current?.ParentId != null)
            {
                if (current.ParentId == potentialAncestor.Id)
                {
                    return true;
                }
                current = _availableFolders.FirstOrDefault(f => f.Id == current.ParentId);
            }
            return false;
        }

        private List<(ProjectNode Node, int Level)> GetFlattenedFolders(List<ProjectNode> folders)
        {
            try
            {
                var result = new List<(ProjectNode Node, int Level)>();
                var folderList = folders.Where(f => f.IsFolder).ToList();
                var rootFolders = folderList.Where(f => string.IsNullOrEmpty(f.ParentId)).OrderBy(f => f.OrderIndex);
                
                foreach (var folder in rootFolders)
                {
                    AddFolderRecursive(result, folder, folderList, 0);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetFlattenedFolders error: {ex.Message}");
                return new List<(ProjectNode Node, int Level)>();
            }
        }

        private void AddFolderRecursive(List<(ProjectNode Node, int Level)> result, ProjectNode folder, List<ProjectNode> allFolders, int level)
        {
            try
            {
                result.Add((folder, level));
                
                var children = allFolders.Where(f => f.ParentId == folder.Id).OrderBy(f => f.OrderIndex);
                foreach (var child in children)
                {
                    AddFolderRecursive(result, child, allFolders, level + 1);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddFolderRecursive error: {ex.Message}");
                // 再帰処理でエラーが発生した場合は、このフォルダーだけを追加
                result.Add((folder, level));
            }
        }

        private void ProjectNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    UpdateButton_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    CancelButton_Click(sender, e);
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ProjectNameTextBox_KeyDown error: {ex.Message}");
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ValidateAndAccept())
                {
                    DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateButton_Click error: {ex.Message}");
                MessageBox.Show($"プロジェクトの更新中にエラーが発生しました: {ex.Message}", 
                    "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private bool ValidateAndAccept()
        {
            try
            {
                string projectName = ProjectNameTextBox.Text.Trim();
                
                // Validate project name
                // プロジェクト名を入力してください
                if (string.IsNullOrEmpty(projectName))
                {
                    var itemType = _currentProject?.IsFolder == true ? "フォルダー" : "プロジェクト";
                    MessageBox.Show($"{itemType}名を入力してください。", "入力エラー", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    ProjectNameTextBox.Focus();
                    return false;
                }

                // Check for invalid characters
                if (projectName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
                {
                    var itemType = _currentProject?.IsFolder == true ? "フォルダー" : "プロジェクト";
                    MessageBox.Show($"{itemType}名に使用できない文字が含まれています。", "入力エラー", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    ProjectNameTextBox.Focus();
                    return false;
                }

                // Check if name changed and if so, validate uniqueness
                if (projectName != _originalName)
                {
                    var selectedItem = FolderComboBox.SelectedItem as dynamic;
                    ProjectNode? selectedFolder = selectedItem?.Node;
                    
                    // Check for duplicate names in the same parent folder
                    var siblings = _availableFolders.Where(f => f.ParentId == selectedFolder?.Id && f.Id != _currentProject?.Id).ToList();
                    if (siblings.Any(s => s.Name.Equals(projectName, StringComparison.OrdinalIgnoreCase)))
                    {
                        var itemType = _currentProject?.IsFolder == true ? "フォルダー" : "プロジェクト";
                        MessageBox.Show($"同じフォルダ内に同じ名前の{itemType}が既に存在します。", "重複エラー", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        ProjectNameTextBox.Focus();
                        return false;
                    }
                }

                // Set results
                ProjectName = projectName;
                var selectedItemResult = FolderComboBox.SelectedItem as dynamic;
                SelectedFolder = selectedItemResult?.Node;
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ValidateAndAccept error: {ex.Message}");
                MessageBox.Show($"検証中にエラーが発生しました: {ex.Message}", 
                    "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}