using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ModernLauncher.Services;
using ModernLauncher.ViewModels;
using ModernLauncher.Models;
using ModernLauncher.Interfaces;
using PathHub.Utils;

namespace ModernLauncher.Views
{
    public partial class MainWindow : Window
    {
        private readonly WindowLayoutManager _layoutManager;
        private readonly DragDropManager _dragDropManager;
        private readonly KeyboardShortcutManager _keyboardManager;
        private readonly ColumnSortManager _columnSortManager;

        public MainWindow()
        {
            // 依存関係を注入
            var projectService = ServiceLocator.Instance.GetService<IProjectService>();
            _layoutManager = new WindowLayoutManager(projectService);
            _dragDropManager = new DragDropManager();
            _keyboardManager = new KeyboardShortcutManager();
            _columnSortManager = new ColumnSortManager(_layoutManager);

            // ウィンドウレイアウトを事前に読み込み
            _layoutManager.LoadWindowLayoutEarly(this);
            
            InitializeComponent();

            // ContextMenu関連のイベントを追跡
            ProjectTreeView.ContextMenuOpening += (s, e) => {
                DebugLogger.Log($"=== CONTEXT MENU OPENING ===");
                DebugLogger.Log($"Sender: {s?.GetType().Name}");
                DebugLogger.Log($"SelectedItem: {ProjectTreeView.SelectedItem?.GetType().Name} - {ProjectTreeView.SelectedItem}");
                if (DataContext is MainViewModel vm)
                {
                    DebugLogger.Log($"ViewModel.SelectedProjectNode: {vm.SelectedProjectNode?.Name ?? "null"}");
                    DebugLogger.Log($"NewProjectCommand: {vm.NewProjectCommand}");
                    DebugLogger.Log($"NewProjectCommand.CanExecute: {vm.NewProjectCommand?.CanExecute(null)}");
                }
                DebugLogger.Log("===========================");
            };

            ProjectTreeView.ContextMenuClosing += (s, e) => {
                DebugLogger.Log("=== CONTEXT MENU CLOSING ===");
            };

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            SizeChanged += MainWindow_SizeChanged;
            LocationChanged += MainWindow_LocationChanged;
            KeyDown += MainWindow_KeyDown;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 各マネージャーを初期化
            _layoutManager.ApplyLayoutSettings(this);
            _dragDropManager.Initialize(this);
            _keyboardManager.SetupKeyboardShortcuts(this);
            _columnSortManager.SetupColumnSorting(this);

            // TreeViewのドラッグ&ドロップを設定
            SetupTreeViewDragDrop();

            // MainListViewの横スクロール問題を修正
            SetupMainListViewScrolling();
        }

        private void SetupTreeViewDragDrop()
        {
            var projectTreeView = FindName("ProjectTreeView") as TreeView;
            if (projectTreeView != null)
            {
                _dragDropManager.SetupProjectTreeViewEvents(projectTreeView);
            }

            var groupTreeView = FindName("GroupTreeView") as TreeView;
            if (groupTreeView != null)
            {
                _dragDropManager.SetupGroupTreeViewEvents(groupTreeView);
            }
        }

        private void SetupMainListViewScrolling()
        {
            var mainListView = FindName("MainListView") as ListView;
            if (mainListView != null)
            {
                mainListView.PreviewMouseWheel += MainListView_PreviewMouseWheel;
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is MainViewModel viewModel && e.NewValue is ItemGroup selectedGroup)
            {
                viewModel.SelectedViewGroup = selectedGroup;
            }
        }

        private void ProjectTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            DebugLogger.Log($"=== PROJECT TREE SELECTION EVENT ===");
            DebugLogger.Log($"Sender: {sender?.GetType().Name}");
            DebugLogger.Log($"OldValue: {e.OldValue?.GetType().Name} - {e.OldValue}");
            DebugLogger.Log($"NewValue: {e.NewValue?.GetType().Name} - {e.NewValue}");

            if (DataContext is MainViewModel viewModel && e.NewValue is ProjectNode selectedProjectNode)
            {
                DebugLogger.Log($"Selected ProjectNode: {selectedProjectNode.Name}");
                DebugLogger.Log($"IsFolder: {selectedProjectNode.IsFolder}");
                DebugLogger.Log($"ID: {selectedProjectNode.Id}");

                viewModel.SelectedProjectNode = selectedProjectNode;
                DebugLogger.Log($"SelectedProjectNode set to: {viewModel.SelectedProjectNode?.Name}");
            }
            else
            {
                DebugLogger.Log($"DataContext type: {DataContext?.GetType().Name}");
                DebugLogger.Log($"e.NewValue type: {e.NewValue?.GetType().Name}");
                DebugLogger.Log($"Failed to set SelectedProjectNode");
            }
            DebugLogger.Log("====================================");
        }

        private void ProjectTreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem treeViewItem && 
                treeViewItem.DataContext is ProjectNode projectNode &&
                DataContext is MainViewModel viewModel)
            {
                System.Diagnostics.Debug.WriteLine($"=== PROJECT SELECTION DEBUG ===");
                System.Diagnostics.Debug.WriteLine($"Selected: {projectNode.Name}");
                System.Diagnostics.Debug.WriteLine($"IsFolder: {projectNode.IsFolder}");
                System.Diagnostics.Debug.WriteLine($"ID: {projectNode.Id}");
                System.Diagnostics.Debug.WriteLine($"Previous Selection: {viewModel.SelectedProjectNode?.Name ?? "None"}");
                System.Diagnostics.Debug.WriteLine($"Timestamp: {DateTime.Now:HH:mm:ss.fff}");
                System.Diagnostics.Debug.WriteLine("================================");
                
                viewModel.SelectedProjectNode = projectNode;
                e.Handled = true;
            }
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListView listView && 
                listView.SelectedItem != null && 
                DataContext is MainViewModel viewModel)
            {
                viewModel.LaunchItemCommand.Execute(listView.SelectedItem);
            }
        }

        private void MainListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var listView = sender as ListView;
            if (listView == null) return;

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                var scrollViewer = FindScrollViewer(listView);
                if (scrollViewer != null)
                {
                    if (e.Delta > 0)
                    {
                        scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - 50);
                    }
                    else
                    {
                        scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + 50);
                    }
                    e.Handled = true;
                }
            }
        }

        private ScrollViewer? FindScrollViewer(DependencyObject parent)
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is ScrollViewer scrollViewer)
                {
                    return scrollViewer;
                }
                
                var result = FindScrollViewer(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // 空のイベントハンドラー（必要に応じて実装）
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            _layoutManager.SaveWindowLayout(this);
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // サイズ変更はLayoutManagerが自動的に処理
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            // 位置変更はLayoutManagerが自動的に処理
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl+F で検索テキストボックスにフォーカス
            if (e.Key == Key.F && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && (Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift)
            {
                var searchTextBox = FindName("SearchTextBox") as TextBox;
                if (searchTextBox != null)
                {
                    searchTextBox.Focus();
                    searchTextBox.SelectAll();
                }
                e.Handled = true;
            }
            // Ctrl+Shift+F で全てのプロジェクトから検索
            else if (e.Key == Key.F && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                if (DataContext is MainViewModel viewModel)
                {
                    viewModel.SearchAllProjectsCommand.Execute(null);
                }
                e.Handled = true;
            }
        }
    }
}