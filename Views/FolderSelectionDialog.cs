using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ModernLauncher.Models;

namespace ModernLauncher.Views
{
    public partial class FolderSelectionDialog : Window
    {
        private ComboBox? folderComboBox;
        private TreeView? folderTreeView;
        private bool useComboBox = false; // Toggle between ComboBox and TreeView
        public ProjectNode? SelectedFolder { get; private set; }

        public FolderSelectionDialog(IEnumerable<ProjectNode> folders, string title = "Folder Selection")
        {
            InitializeComponent();
            Title = title;
            PopulateFolders(folders);
        }

        private void InitializeComponent()
        {
            Width = 400;
            Height = 300;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.CanResize;

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            if (useComboBox)
            {
                // ComboBox approach (fallback)
                folderComboBox = new ComboBox
                {
                    Margin = new Thickness(10),
                    FontSize = 13,
                    DisplayMemberPath = "DisplayName"
                };
                Grid.SetRow(folderComboBox, 0);
                grid.Children.Add(folderComboBox);
            }
            else
            {
                // TreeView approach with minimal styling
                folderTreeView = new TreeView
                {
                    Margin = new Thickness(10, 10, 10, 10),
                    FontSize = 13,
                    Background = System.Windows.Media.Brushes.White,
                    BorderThickness = new Thickness(1, 1, 1, 1),
                    BorderBrush = System.Windows.Media.Brushes.Gray
                };
                
                // Create a simple TreeViewItem style to avoid conflicts
                var treeViewItemStyle = new Style(typeof(TreeViewItem));
                treeViewItemStyle.Setters.Add(new Setter(TreeViewItem.PaddingProperty, new Thickness(4)));
                treeViewItemStyle.Setters.Add(new Setter(TreeViewItem.MarginProperty, new Thickness(0, 1, 0, 1)));
                treeViewItemStyle.Setters.Add(new Setter(TreeViewItem.FontSizeProperty, 13.0));
                
                folderTreeView.ItemContainerStyle = treeViewItemStyle;
                
                Grid.SetRow(folderTreeView, 0);
                grid.Children.Add(folderTreeView);
            }

            // Buttons
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(10)
            };

            var okButton = new Button
            {
                Content = "OK",
                Width = 80,
                Height = 28,
                Margin = new Thickness(5),
                FontSize = 13,
                IsDefault = true
            };
            okButton.Click += OkButton_Click;

            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 80,
                Height = 28,
                Margin = new Thickness(5),
                FontSize = 13,
                IsCancel = true
            };
            cancelButton.Click += CancelButton_Click;

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            Grid.SetRow(buttonPanel, 2);
            grid.Children.Add(buttonPanel);

            Content = grid;
        }

        private void PopulateFolders(IEnumerable<ProjectNode> folders)
        {
            var folderList = folders.Where(f => f.IsFolder).ToList();
            System.Diagnostics.Debug.WriteLine($"PopulateFolders: Available folders count: {folderList.Count}");

            if (useComboBox)
            {
                PopulateComboBox(folderList);
            }
            else
            {
                PopulateTreeView(folderList);
            }
        }

        private void PopulateComboBox(List<ProjectNode> folders)
        {
            if (folderComboBox == null) return;

            folderComboBox.Items.Clear();

            // Add root option
            var rootOption = new { DisplayName = "[Root]", Node = (ProjectNode?)null };
            folderComboBox.Items.Add(rootOption);

            // Add all folders with indentation to show hierarchy
            var allFolders = GetFlattenedFolders(folders);
            foreach (var folderInfo in allFolders)
            {
                var indent = new string(' ', folderInfo.Level * 2);
                var displayItem = new { DisplayName = $"{indent}[F] {folderInfo.Node.Name}", Node = folderInfo.Node };
                folderComboBox.Items.Add(displayItem);
            }

            folderComboBox.SelectedIndex = 0; // Select root by default
        }

        private void PopulateTreeView(List<ProjectNode> folders)
        {
            if (folderTreeView == null) return;

            folderTreeView.Items.Clear();

            // Add "Root" option
            var rootItem = new TreeViewItem
            {
                Header = "[Root]",
                Tag = null,
                FontSize = 13,
                IsExpanded = true
            };
            folderTreeView.Items.Add(rootItem);

            // Add folder hierarchy - ルートレベルのフォルダーを追加
            var rootFolders = folders.Where(f => string.IsNullOrEmpty(f.ParentId)).ToList();
            System.Diagnostics.Debug.WriteLine($"Root level folders: {rootFolders.Count}");
            
            foreach (var rootFolder in rootFolders.OrderBy(f => f.OrderIndex))
            {
                var treeViewItem = CreateTreeViewItem(rootFolder);
                rootItem.Items.Add(treeViewItem);
                PopulateTreeViewRecursive(treeViewItem, folders, rootFolder.Id);
            }

            // デフォルトでルートを選択
            rootItem.IsSelected = true;
            
            // フォルダーが0個の場合の警告
            if (folders.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("WARNING: No folders available for selection!");
            }
        }

        private List<(ProjectNode Node, int Level)> GetFlattenedFolders(List<ProjectNode> folders)
        {
            var result = new List<(ProjectNode Node, int Level)>();
            var rootFolders = folders.Where(f => string.IsNullOrEmpty(f.ParentId)).OrderBy(f => f.OrderIndex);
            
            foreach (var folder in rootFolders)
            {
                AddFolderRecursive(result, folder, folders, 0);
            }
            
            return result;
        }

        private void AddFolderRecursive(List<(ProjectNode Node, int Level)> result, ProjectNode folder, List<ProjectNode> allFolders, int level)
        {
            result.Add((folder, level));
            
            var children = allFolders.Where(f => f.ParentId == folder.Id).OrderBy(f => f.OrderIndex);
            foreach (var child in children)
            {
                AddFolderRecursive(result, child, allFolders, level + 1);
            }
        }

        private TreeViewItem CreateTreeViewItem(ProjectNode folder)
        {
            return new TreeViewItem
            {
                Header = $"[F] {folder.Name}",
                Tag = folder,
                FontSize = 13,
                IsExpanded = true
            };
        }

        private void PopulateTreeViewRecursive(TreeViewItem parentItem, List<ProjectNode> allFolders, string parentId)
        {
            var childFolders = allFolders.Where(f => f.ParentId == parentId).OrderBy(f => f.OrderIndex);
            
            foreach (var folder in childFolders)
            {
                System.Diagnostics.Debug.WriteLine($"Adding child folder: {folder.Name} under parent {parentId}");
                
                var treeViewItem = CreateTreeViewItem(folder);
                parentItem.Items.Add(treeViewItem);

                // 再帰的に子フォルダーを追加
                PopulateTreeViewRecursive(treeViewItem, allFolders, folder.Id);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (useComboBox)
            {
                if (folderComboBox?.SelectedItem != null)
                {
                    var selectedItem = folderComboBox.SelectedItem;
                    var nodeProperty = selectedItem.GetType().GetProperty("Node");
                    SelectedFolder = nodeProperty?.GetValue(selectedItem) as ProjectNode;
                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Please select a folder.", "Selection Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                if (folderTreeView?.SelectedItem is TreeViewItem selectedItem)
                {
                    SelectedFolder = selectedItem.Tag as ProjectNode;
                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Please select a folder.", "Selection Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}