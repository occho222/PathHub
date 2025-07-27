using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ModernLauncher.Models;

namespace ModernLauncher.Views
{
    /// <summary>
    /// FolderSelectionDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderSelectionDialog : Window
    {
        public ProjectNode? SelectedFolder { get; private set; }

        public FolderSelectionDialog(IEnumerable<ProjectNode> folders, string title = "フォルダ選択")
        {
            InitializeComponent();
            
            Title = title;
            HeaderTextBlock.Text = title;
            
            PopulateFolders(folders);
        }

        private void PopulateFolders(IEnumerable<ProjectNode> folders)
        {
            var folderList = folders.Where(f => f.IsFolder).ToList();
            System.Diagnostics.Debug.WriteLine($"PopulateFolders: Available folders count: {folderList.Count}");

            FolderTreeView.Items.Clear();

            // Add "Root" option
            var rootItem = new TreeViewItem
            {
                Header = "📁 [ルート]",
                Tag = null,
                FontSize = 13,
                IsExpanded = true,
                FontWeight = FontWeights.SemiBold
            };
            FolderTreeView.Items.Add(rootItem);

            // Add folder hierarchy - add root level folders
            var rootFolders = folderList.Where(f => string.IsNullOrEmpty(f.ParentId)).ToList();
            System.Diagnostics.Debug.WriteLine($"Root level folders: {rootFolders.Count}");
            
            foreach (var rootFolder in rootFolders.OrderBy(f => f.OrderIndex))
            {
                var treeViewItem = CreateTreeViewItem(rootFolder);
                rootItem.Items.Add(treeViewItem);
                PopulateTreeViewRecursive(treeViewItem, folderList, rootFolder.Id);
            }

            // Select root by default
            rootItem.IsSelected = true;
            
            // Warning if no folders available
            if (folderList.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("WARNING: No folders available for selection!");
            }
        }

        private TreeViewItem CreateTreeViewItem(ProjectNode folder)
        {
            return new TreeViewItem
            {
                Header = $"📁 {folder.Name}",
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

                // Recursively add child folders
                PopulateTreeViewRecursive(treeViewItem, allFolders, folder.Id);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTreeView.SelectedItem is TreeViewItem selectedItem)
            {
                SelectedFolder = selectedItem.Tag as ProjectNode;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("フォルダを選択してください。", "選択エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}