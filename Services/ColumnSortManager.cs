using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ModernLauncher.Services;

namespace ModernLauncher.Services
{
    public class ColumnSortManager
    {
        private string? _lastSortProperty;
        private ListSortDirection _lastSortDirection = ListSortDirection.Ascending;
        private readonly WindowLayoutManager _layoutManager;

        public ColumnSortManager(WindowLayoutManager layoutManager)
        {
            _layoutManager = layoutManager;
        }

        public void SetupColumnSorting(Window window)
        {
            window.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(ListViewColumnHeader_Click));
        }

        private void ListViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is GridViewColumnHeader header && header.Tag is string propertyName)
            {
                var listView = FindMainListView(sender as Window);
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

                // レイアウトマネージャーにソート設定を保存
                _layoutManager.SetLastSortSettings(_lastSortProperty, direction == ListSortDirection.Ascending);
            }
        }

        private ListView? FindMainListView(Window? window)
        {
            if (window == null) return null;
            
            return window.FindName("MainListView") as ListView ?? FindListViewInVisualTree(window);
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

        public void ClearListViewSort(Window window)
        {
            var listView = window.FindName("MainListView") as ListView;
            if (listView?.ItemsSource != null)
            {
                var view = CollectionViewSource.GetDefaultView(listView.ItemsSource);
                view?.SortDescriptions.Clear();
                view?.Refresh();
            }
        }
    }
}