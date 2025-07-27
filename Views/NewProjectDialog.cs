using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ModernLauncher.Models;

namespace ModernLauncher.Views
{
    /// <summary>
    /// NewProjectDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class NewProjectDialog : Window
    {
        public string ProjectName { get; private set; } = string.Empty;
        public ProjectNode? SelectedFolder { get; private set; }
        
        private readonly List<ProjectNode> availableFolders;

        public NewProjectDialog(List<ProjectNode> folders)
        {
            try
            {
                InitializeComponent();
                
                availableFolders = folders ?? new List<ProjectNode>();
                PopulateFolders();
                
                ProjectNameTextBox.Focus();
            }
            catch (Exception ex)
            {
                // エラー情報をログに出力
                System.Diagnostics.Debug.WriteLine($"NewProjectDialog constructor error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // 最低限の初期化
                availableFolders = new List<ProjectNode>();
                
                // ユーザーにエラーを通知
                MessageBox.Show($"Dialog initialization failed: {ex.Message}\n\nPlease try again.", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                
                // ダイアログを閉じる
                DialogResult = false;
            }
        }

        private void PopulateFolders()
        {
            try
            {
                FolderComboBox.Items.Clear();

                // Add root option
                var rootOption = new { DisplayName = "📁 [Root] - Create at top level", Node = (ProjectNode?)null };
                FolderComboBox.Items.Add(rootOption);

                // Add available folders with hierarchy
                var allFolders = GetFlattenedFolders(availableFolders);
                foreach (var folderInfo in allFolders)
                {
                    var indent = new string(' ', folderInfo.Level * 4); // より見やすくするため4文字インデント
                    var displayItem = new { DisplayName = $"📁 {indent}{folderInfo.Node.Name}", Node = folderInfo.Node };
                    FolderComboBox.Items.Add(displayItem);
                }

                // Select root by default
                if (FolderComboBox.Items.Count > 0)
                {
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
                    CreateButton_Click(sender, e);
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

        private void CreateButton_Click(object sender, RoutedEventArgs e)
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
                System.Diagnostics.Debug.WriteLine($"CreateButton_Click error: {ex.Message}");
                MessageBox.Show($"An error occurred while creating the project: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogResult = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CancelButton_Click error: {ex.Message}");
                Close();
            }
        }

        private bool ValidateAndAccept()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ProjectNameTextBox.Text))
                {
                    MessageBox.Show("Please enter a project name.", "Input Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    ProjectNameTextBox.Focus();
                    return false;
                }

                if (FolderComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Please select a destination folder.", "Selection Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    FolderComboBox.Focus();
                    return false;
                }

                ProjectName = ProjectNameTextBox.Text.Trim();
                
                var selectedItem = FolderComboBox.SelectedItem;
                var nodeProperty = selectedItem?.GetType().GetProperty("Node");
                SelectedFolder = nodeProperty?.GetValue(selectedItem) as ProjectNode;

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ValidateAndAccept error: {ex.Message}");
                MessageBox.Show($"Validation error: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}