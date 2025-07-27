using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ModernLauncher.Models;
using ModernLauncher.ViewModels;

namespace ModernLauncher.Services
{
    public class DragDropManager
    {
        private Border? _dropOverlay;
        
        // �v���W�F�N�g�h���b�O&�h���b�v�֘A
        private bool _isDraggingProject = false;
        private Point _projectDragStartPoint;
        private ProjectNode? _draggedProjectNode;
        
        // �O���[�v�h���b�O&�h���b�v�֘A
        private bool _isDraggingGroup = false;
        private Point _groupDragStartPoint;
        private ItemGroup? _draggedGroup;

        public void Initialize(Window window)
        {
            _dropOverlay = window.FindName("DropOverlay") as Border;

            // �h���b�O&�h���b�v�C�x���g�n���h���[��ݒ�
            window.DragEnter += MainWindow_DragEnter;
            window.DragOver += MainWindow_DragOver;
            window.DragLeave += MainWindow_DragLeave;
            window.Drop += MainWindow_Drop;
        }

        public void SetupProjectTreeViewEvents(TreeView projectTreeView)
        {
            projectTreeView.PreviewMouseLeftButtonDown += ProjectTreeView_PreviewMouseLeftButtonDown;
            projectTreeView.PreviewMouseMove += ProjectTreeView_PreviewMouseMove;
            projectTreeView.DragOver += ProjectTreeView_DragOver;
            projectTreeView.Drop += ProjectTreeView_Drop;
        }

        public void SetupGroupTreeViewEvents(TreeView groupTreeView)
        {
            groupTreeView.PreviewMouseLeftButtonDown += GroupTreeView_PreviewMouseLeftButtonDown;
            groupTreeView.PreviewMouseMove += GroupTreeView_PreviewMouseMove;
            groupTreeView.DragOver += GroupTreeView_DragOver;
            groupTreeView.Drop += GroupTreeView_Drop;
        }

        private void MainWindow_DragEnter(object sender, DragEventArgs e)
        {
            bool canDrop = false;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    canDrop = true;
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.Html) || 
                     e.Data.GetDataPresent(DataFormats.Text) || 
                     e.Data.GetDataPresent(DataFormats.UnicodeText))
            {
                canDrop = true;
            }

            if (canDrop)
            {
                e.Effects = DragDropEffects.Copy;
                ShowDropOverlay();
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void MainWindow_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) ||
                e.Data.GetDataPresent(DataFormats.Html) ||
                e.Data.GetDataPresent(DataFormats.Text) ||
                e.Data.GetDataPresent(DataFormats.UnicodeText))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void MainWindow_DragLeave(object sender, DragEventArgs e)
        {
            if (sender is Window window)
            {
                var position = e.GetPosition(window);
                var bounds = new Rect(0, 0, window.ActualWidth, window.ActualHeight);
                
                if (!bounds.Contains(position))
                {
                    HideDropOverlay();
                }
            }
            e.Handled = true;
        }

        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            HideDropOverlay();

            if (sender is Window window && window.DataContext is MainViewModel viewModel)
            {
                try
                {
                    if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    {
                        var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                        if (files != null && files.Length > 0)
                        {
                            foreach (var file in files)
                            {
                                try
                                {
                                    viewModel.ShowAddItemDialogWithPath(file);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"�t�@�C���u{Path.GetFileName(file)}�v�̒ǉ��Ɏ��s���܂���: {ex.Message}", 
                                                  "�G���[", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                        }
                    }
                    else if (e.Data.GetDataPresent(DataFormats.Html) || 
                             e.Data.GetDataPresent(DataFormats.Text) || 
                             e.Data.GetDataPresent(DataFormats.UnicodeText))
                    {
                        string? url = ExtractUrlFromDropData(e.Data);
                        if (!string.IsNullOrEmpty(url))
                        {
                            viewModel.ShowAddItemDialogWithPath(url);
                        }
                        else
                        {
                            MessageBox.Show("�L����URL��������܂���ł����B", "���", 
                                          MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"�h���b�v�����A�C�e���̏����Ɏ��s���܂���: {ex.Message}", 
                                  "�G���[", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            e.Handled = true;
        }

        private string? ExtractUrlFromDropData(IDataObject dataObject)
        {
            // HTML����URL�𒊏o
            if (dataObject.GetDataPresent(DataFormats.Html))
            {
                try
                {
                    var html = dataObject.GetData(DataFormats.Html) as string;
                    if (!string.IsNullOrEmpty(html))
                    {
                        var hrefMatch = System.Text.RegularExpressions.Regex.Match(html, @"href=[""']([^""']+)[""']", 
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        if (hrefMatch.Success)
                        {
                            string url = hrefMatch.Groups[1].Value;
                            if (IsValidUrl(url))
                            {
                                return url;
                            }
                        }
                    }
                }
                catch
                {
                    // HTML�̉�͂Ɏ��s�����ꍇ�̓e�L�X�g����̒��o�Ƀt�H�[���o�b�N
                }
            }

            // �e�L�X�g����URL�𒊏o
            string? text = null;
            if (dataObject.GetDataPresent(DataFormats.UnicodeText))
            {
                text = dataObject.GetData(DataFormats.UnicodeText) as string;
            }
            else if (dataObject.GetDataPresent(DataFormats.Text))
            {
                text = dataObject.GetData(DataFormats.Text) as string;
            }

            if (!string.IsNullOrEmpty(text))
            {
                var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if (IsValidUrl(trimmedLine))
                    {
                        return trimmedLine;
                    }
                }

                var urlMatch = System.Text.RegularExpressions.Regex.Match(text, 
                    @"https?://[^\s]+", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (urlMatch.Success)
                {
                    return urlMatch.Value;
                }
            }

            return null;
        }

        private bool IsValidUrl(string url)
        {
            return !string.IsNullOrEmpty(url) && 
                   (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                    url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) &&
                   Uri.TryCreate(url, UriKind.Absolute, out _);
        }

        private void ShowDropOverlay()
        {
            if (_dropOverlay != null)
            {
                _dropOverlay.Visibility = Visibility.Visible;
            }
        }

        private void HideDropOverlay()
        {
            if (_dropOverlay != null)
            {
                _dropOverlay.Visibility = Visibility.Collapsed;
            }
        }

        // �v���W�F�N�g�h���b�O&�h���b�v����
        private void ProjectTreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement element)
            {
                var treeViewItem = FindAncestor<TreeViewItem>(element);
                if (treeViewItem?.DataContext is ProjectNode projectNode)
                {
                    _projectDragStartPoint = e.GetPosition(null);
                    _draggedProjectNode = projectNode;
                }
            }
        }

        private void ProjectTreeView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _draggedProjectNode != null)
            {
                var currentPosition = e.GetPosition(null);
                var diff = _projectDragStartPoint - currentPosition;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    _isDraggingProject = true;
                    var treeView = sender as TreeView;
                    var data = new DataObject("ProjectNode", _draggedProjectNode);
                    
                    DragDrop.DoDragDrop(treeView, data, DragDropEffects.Move);
                    
                    _isDraggingProject = false;
                    _draggedProjectNode = null;
                }
            }
        }

        private void ProjectTreeView_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("ProjectNode"))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void ProjectTreeView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("ProjectNode") && 
                e.Data.GetData("ProjectNode") is ProjectNode draggedNode)
            {
                var dropTarget = GetDropTarget<ProjectNode>(e.OriginalSource as FrameworkElement);
                
                if (dropTarget != null && dropTarget != draggedNode &&
                    sender is TreeView treeView && treeView.DataContext is MainViewModel viewModel)
                {
                    // �����e�̉��ŕ��ёւ�
                    if (draggedNode.ParentId == dropTarget.ParentId)
                    {
                        var project = viewModel.Projects.FirstOrDefault(p => p.Id == draggedNode.Id);
                        var targetProject = viewModel.Projects.FirstOrDefault(p => p.Id == dropTarget.Id);
                        
                        if (project != null && targetProject != null)
                        {
                            var siblings = viewModel.Projects.Where(p => p.ParentId == project.ParentId)
                                                           .OrderBy(p => p.OrderIndex)
                                                           .ToList();
                            
                            var newIndex = siblings.IndexOf(targetProject);
                            viewModel.MoveProjectToPosition(draggedNode.Id, newIndex);
                        }
                    }
                }
            }
        }

        // �O���[�v�h���b�O&�h���b�v����
        private void GroupTreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement element)
            {
                var treeViewItem = FindAncestor<TreeViewItem>(element);
                if (treeViewItem?.DataContext is ItemGroup group && group.Id != "all")
                {
                    _groupDragStartPoint = e.GetPosition(null);
                    _draggedGroup = group;
                }
            }
        }

        private void GroupTreeView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _draggedGroup != null)
            {
                var currentPosition = e.GetPosition(null);
                var diff = _groupDragStartPoint - currentPosition;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    _isDraggingGroup = true;
                    var treeView = sender as TreeView;
                    var data = new DataObject("ItemGroup", _draggedGroup);
                    
                    DragDrop.DoDragDrop(treeView, data, DragDropEffects.Move);
                    
                    _isDraggingGroup = false;
                    _draggedGroup = null;
                }
            }
        }

        private void GroupTreeView_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("ItemGroup"))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void GroupTreeView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("ItemGroup") && 
                e.Data.GetData("ItemGroup") is ItemGroup draggedGroup)
            {
                var dropTarget = GetDropTarget<ItemGroup>(e.OriginalSource as FrameworkElement);
                
                if (dropTarget != null && dropTarget != draggedGroup &&
                    sender is TreeView treeView && treeView.DataContext is MainViewModel viewModel &&
                    viewModel.CurrentProject != null)
                {
                    var groups = viewModel.CurrentProject.Groups.OrderBy(g => g.OrderIndex).ToList();
                    var newIndex = groups.IndexOf(dropTarget);
                    
                    if (newIndex >= 0)
                    {
                        viewModel.MoveGroupToPosition(draggedGroup.Id, newIndex);
                    }
                }
            }
        }

        private T? FindAncestor<T>(DependencyObject current) where T : class
        {
            do
            {
                if (current is T ancestor)
                {
                    return ancestor;
                }
                current = System.Windows.Media.VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        private T? GetDropTarget<T>(FrameworkElement? element) where T : class
        {
            var treeViewItem = FindAncestor<TreeViewItem>(element);
            return treeViewItem?.DataContext as T;
        }
    }
}