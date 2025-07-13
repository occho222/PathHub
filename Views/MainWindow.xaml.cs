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
            // ドロップオーバーレイの参照を取得
            dropOverlay = FindName("DropOverlay") as Border;

            // ドラッグ&ドロップイベントハンドラーを設定
            DragEnter += MainWindow_DragEnter;
            DragOver += MainWindow_DragOver;
            DragLeave += MainWindow_DragLeave;
            Drop += MainWindow_Drop;

            // GridViewColumnHeaderのClickイベントをウィンドウレベルでハンドル
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
                e.Handled = true; // イベントのバブリングを停止
            }
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // ListView項目のダブルクリックでアイテムを起動
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
                // メインListViewを探す
                var listView = FindMainListView();
                if (listView?.ItemsSource == null) return;

                var view = CollectionViewSource.GetDefaultView(listView.ItemsSource);
                if (view == null) return;

                // ソート方向の切り替え
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
            // MainListViewを名前で直接検索
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

            // ファイルドロップをチェック
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    canDrop = true;
                }
            }
            // WebブラウザからのHTML/テキストドロップをチェック
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
            // マウスがウィンドウの境界を出た場合のみオーバーレイを隠す
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
                    // ファイルドロップの処理
                    if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    {
                        var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                        if (files != null && files.Length > 0)
                        {
                            foreach (var file in files)
                            {
                                try
                                {
                                    // 追加用ダイアログを経由してアイテムを追加
                                    viewModel.ShowAddItemDialogWithPath(file);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"ファイル「{Path.GetFileName(file)}」の追加に失敗しました: {ex.Message}", 
                                                  "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                        }
                    }
                    // WebブラウザからのHTML/テキストドロップの処理
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
                            MessageBox.Show("有効なURLが見つかりませんでした。", "情報", 
                                          MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"ドロップしたアイテムの処理に失敗しました: {ex.Message}", 
                                  "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            e.Handled = true;
        }

        private string? ExtractUrlFromDropData(IDataObject dataObject)
        {
            // HTMLからURLを抽出
            if (dataObject.GetDataPresent(DataFormats.Html))
            {
                try
                {
                    var html = dataObject.GetData(DataFormats.Html) as string;
                    if (!string.IsNullOrEmpty(html))
                    {
                        // HTMLからhref属性を抽出
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
                    // HTMLの解析に失敗した場合はテキストからの抽出にフォールバック
                }
            }

            // テキストからURLを抽出
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
                // 行ごとに分割してURLを探す
                var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if (IsValidUrl(trimmedLine))
                    {
                        return trimmedLine;
                    }
                }

                // 正規表現でURLパターンを抽出
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