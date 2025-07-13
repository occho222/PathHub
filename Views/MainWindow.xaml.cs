using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Data;
using ModernLauncher.ViewModels;
using ModernLauncher.Models;

namespace ModernLauncher.Views
{
    public partial class MainWindow : Window
    {
        private Border? dropOverlay;
        private string? _lastSortProperty;
        private ListSortDirection _lastSortDirection = ListSortDirection.Ascending;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // �h���b�v�I�[�o�[���C�̎Q�Ƃ��擾
            dropOverlay = FindName("DropOverlay") as Border;

            // �h���b�O&�h���b�v�C�x���g�n���h���[��ݒ�
            DragEnter += MainWindow_DragEnter;
            DragOver += MainWindow_DragOver;
            DragLeave += MainWindow_DragLeave;
            Drop += MainWindow_Drop;

            // GridViewColumnHeader��Click�C�x���g���E�B���h�E���x���Ńn���h��
            AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(ListViewColumnHeader_Click));
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is MainViewModel viewModel && e.NewValue is ItemGroup selectedGroup)
            {
                viewModel.SelectedViewGroup = selectedGroup;
            }
        }

        private void ProjectTreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem treeViewItem && 
                treeViewItem.DataContext is ProjectNode projectNode &&
                DataContext is MainViewModel viewModel)
            {
                viewModel.SelectedProjectNode = projectNode;
                e.Handled = true; // �C�x���g�̃o�u�����O���~
            }
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // ListView���ڂ̃_�u���N���b�N�ŃA�C�e�����N��
            if (sender is ListView listView && 
                listView.SelectedItem != null && 
                DataContext is MainViewModel viewModel)
            {
                viewModel.LaunchItemCommand.Execute(listView.SelectedItem);
            }
        }

        private void ListViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is GridViewColumnHeader header && header.Tag is string propertyName)
            {
                // ���C��ListView��T��
                var listView = FindMainListView();
                if (listView?.ItemsSource == null) return;

                var view = CollectionViewSource.GetDefaultView(listView.ItemsSource);
                if (view == null) return;

                // �\�[�g�����̐؂�ւ�
                ListSortDirection direction = ListSortDirection.Ascending;
                if (_lastSortProperty == propertyName && _lastSortDirection == ListSortDirection.Ascending)
                {
                    direction = ListSortDirection.Descending;
                }
                _lastSortProperty = propertyName;
                _lastSortDirection = direction;

                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription(propertyName, direction));
                view.Refresh();
            }
        }

        private ListView? FindMainListView()
        {
            // MainListView�𖼑O�Œ��ڌ���
            return FindName("MainListView") as ListView ?? FindListViewInVisualTree(this);
        }

        private ListView? FindListViewInVisualTree(DependencyObject parent)
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                
                if (child is ListView listView && listView.View is GridView)
                {
                    return listView;
                }

                var result = FindListViewInVisualTree(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void MainWindow_DragEnter(object sender, DragEventArgs e)
        {
            bool canDrop = false;

            // �t�@�C���h���b�v���`�F�b�N
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    canDrop = true;
                }
            }
            // Web�u���E�U�����HTML/�e�L�X�g�h���b�v���`�F�b�N
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
            // �}�E�X���E�B���h�E�̋��E���o���ꍇ�̂݃I�[�o�[���C���B��
            var position = e.GetPosition(this);
            var bounds = new Rect(0, 0, ActualWidth, ActualHeight);
            
            if (!bounds.Contains(position))
            {
                HideDropOverlay();
            }
            e.Handled = true;
        }

        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            HideDropOverlay();

            if (DataContext is MainViewModel viewModel)
            {
                try
                {
                    // �t�@�C���h���b�v�̏���
                    if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    {
                        var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                        if (files != null && files.Length > 0)
                        {
                            foreach (var file in files)
                            {
                                try
                                {
                                    // �ǉ��p�_�C�A���O���o�R���ăA�C�e����ǉ�
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
                    // Web�u���E�U�����HTML/�e�L�X�g�h���b�v�̏���
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
                        // HTML����href�����𒊏o
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
                // �s���Ƃɕ�������URL��T��
                var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if (IsValidUrl(trimmedLine))
                    {
                        return trimmedLine;
                    }
                }

                // ���K�\����URL�p�^�[���𒊏o
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
            if (dropOverlay != null)
            {
                dropOverlay.Visibility = Visibility.Visible;
            }
        }

        private void HideDropOverlay()
        {
            if (dropOverlay != null)
            {
                dropOverlay.Visibility = Visibility.Collapsed;
            }
        }
    }
}