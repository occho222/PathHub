using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ModernLauncher.ViewModels;
using ModernLauncher.Models;

namespace ModernLauncher.Services
{
    public class KeyboardShortcutManager
    {
        public void SetupKeyboardShortcuts(Window window)
        {
            SetupSearchTextBoxShortcuts(window);
            SetupProjectTreeViewShortcuts(window);
            SetupGroupTreeViewShortcuts(window);
            SetupMainListViewShortcuts(window);
            SetupSmartLauncherShortcuts(window);
        }

        private void SetupSearchTextBoxShortcuts(Window window)
        {
            var searchTextBox = window.FindName("SearchTextBox") as TextBox;
            if (searchTextBox != null)
            {
                searchTextBox.KeyDown += SearchTextBox_KeyDown;
            }
        }

        private void SetupProjectTreeViewShortcuts(Window window)
        {
            var projectTreeView = window.FindName("ProjectTreeView") as TreeView;
            if (projectTreeView != null)
            {
                projectTreeView.KeyDown += ProjectTreeView_KeyDown;
            }
        }

        private void SetupGroupTreeViewShortcuts(Window window)
        {
            var groupTreeView = window.FindName("GroupTreeView") as TreeView;
            if (groupTreeView != null)
            {
                groupTreeView.KeyDown += GroupTreeView_KeyDown;
            }
        }

        private void SetupMainListViewShortcuts(Window window)
        {
            var mainListView = window.FindName("MainListView") as ListView;
            if (mainListView != null)
            {
                mainListView.KeyDown += MainListView_KeyDown;
            }
        }

        private void SetupSmartLauncherShortcuts(Window window)
        {
            if (window.FindName("LeftPanel") is Grid leftPanel)
            {
                for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(leftPanel); i++)
                {
                    var child = System.Windows.Media.VisualTreeHelper.GetChild(leftPanel, i);
                    if (child is ListView listView && Grid.GetRow(listView) == 1)
                    {
                        listView.KeyDown += SmartLauncherListView_KeyDown;
                        return;
                    }
                }
            }
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (sender is TextBox textBox && textBox.DataContext is MainViewModel viewModel)
                {
                    viewModel.ClearSearchCommand.Execute(null);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Tab || e.Key == Key.Down)
            {
                // TabまたはDownキーでメインListViewにフォーカスを移動
                if (sender is TextBox textBox)
                {
                    var window = Window.GetWindow(textBox);
                    if (window != null)
                    {
                        var mainListView = window.FindName("MainListView") as ListView;
                        if (mainListView != null && mainListView.Items.Count > 0)
                        {
                            mainListView.Focus();
                            mainListView.SelectedIndex = 0;
                        }
                    }
                }
                e.Handled = true;
            }
        }

        private void ProjectTreeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TreeView treeView && treeView.DataContext is MainViewModel viewModel)
            {
                if (treeView.SelectedItem is ProjectNode selectedNode)
                {
                    viewModel.SelectedProjectNode = selectedNode;
                }
                e.Handled = true;
            }
        }

        private void GroupTreeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TreeView treeView && treeView.DataContext is MainViewModel viewModel)
            {
                if (treeView.SelectedItem is ItemGroup selectedGroup)
                {
                    viewModel.SelectedViewGroup = selectedGroup;
                }
                e.Handled = true;
            }
        }

        private void MainListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is ListView listView && listView.DataContext is MainViewModel viewModel)
            {
                if (e.Key == Key.Enter && viewModel.SelectedItem != null)
                {
                    viewModel.LaunchItemCommand.Execute(viewModel.SelectedItem);
                    e.Handled = true;
                }
                else if (e.Key == Key.Delete && viewModel.SelectedItem != null)
                {
                    viewModel.DeleteItemCommand.Execute(viewModel.SelectedItem);
                    e.Handled = true;
                }
            }
        }

        private void SmartLauncherListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is ListView listView && listView.DataContext is MainViewModel viewModel)
            {
                if (e.Key == Key.Enter && viewModel.SelectedSmartLauncherItem != null)
                {
                    var selectedItem = viewModel.SelectedSmartLauncherItem;
                    if (selectedItem != null)
                    {
                        viewModel.SelectedSmartLauncherItem = selectedItem;
                        viewModel.StatusText = $"?? SmartLauncher: {selectedItem.DisplayName} ��I�����܂���";
                    }
                    e.Handled = true;
                }
            }
        }
    }
}