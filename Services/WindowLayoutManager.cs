using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ModernLauncher.Interfaces;
using ModernLauncher.Models;

namespace ModernLauncher.Services
{
    public class WindowLayoutManager
    {
        private readonly IProjectService _projectService;
        private WindowLayoutSettings? _windowLayoutSettings;

        public WindowLayoutManager(IProjectService projectService)
        {
            _projectService = projectService;
        }

        public void LoadWindowLayoutEarly(Window window)
        {
            try
            {
                _windowLayoutSettings = _projectService.LoadWindowLayout();
                if (_windowLayoutSettings == null)
                {
                    _windowLayoutSettings = new WindowLayoutSettings();
                    CenterWindowOnScreen(window);
                    return;
                }

                ApplyWindowPosition(window);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"事前ウィンドウレイアウトの読み込みに失敗しました: {ex.Message}");
                _windowLayoutSettings = new WindowLayoutSettings();
                CenterWindowOnScreen(window);
            }
        }

        private void ApplyWindowPosition(Window window)
        {
            if (_windowLayoutSettings == null) return;

            // ウィンドウサイズを設定
            if (_windowLayoutSettings.WindowWidth > 0 && _windowLayoutSettings.WindowHeight > 0)
            {
                window.Width = _windowLayoutSettings.WindowWidth;
                window.Height = _windowLayoutSettings.WindowHeight;
            }

            // ウィンドウ位置を設定
            if (_windowLayoutSettings.WindowLeft >= 0 && _windowLayoutSettings.WindowTop >= 0)
            {
                var screenBounds = SystemParameters.WorkArea;
                if (_windowLayoutSettings.WindowLeft < screenBounds.Right - 100 &&
                    _windowLayoutSettings.WindowTop < screenBounds.Bottom - 100)
                {
                    window.Left = _windowLayoutSettings.WindowLeft;
                    window.Top = _windowLayoutSettings.WindowTop;
                }
                else
                {
                    CenterWindowOnScreen(window);
                }
            }
            else
            {
                CenterWindowOnScreen(window);
            }

            // 最大化状態を設定
            if (_windowLayoutSettings.IsMaximized)
            {
                window.WindowState = WindowState.Maximized;
            }
        }

        private void CenterWindowOnScreen(Window window)
        {
            var screenBounds = SystemParameters.WorkArea;
            window.Left = (screenBounds.Width - window.Width) / 2;
            window.Top = (screenBounds.Height - window.Height) / 2;
        }

        public void ApplyLayoutSettings(Window window)
        {
            if (_windowLayoutSettings == null) return;

            try
            {
                ApplyGridLayoutSettings(window);
                ApplyRowLayoutSettings(window);
                ApplyGridViewColumnWidths(window);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"レイアウト設定の適用に失敗しました: {ex.Message}");
            }
        }

        private void ApplyGridLayoutSettings(Window window)
        {
            var grid = window.FindName("MainGrid") as Grid;
            if (grid?.ColumnDefinitions.Count > 0)
            {
                var leftColumn = grid.ColumnDefinitions[0];
                if (leftColumn != null)
                {
                    leftColumn.Width = new GridLength(_windowLayoutSettings.LeftPanelWidth);
                }
            }
        }

        private void ApplyRowLayoutSettings(Window window)
        {
            var leftPanel = window.FindName("LeftPanel") as Grid;
            if (leftPanel == null) return;

            // SmartLauncherの高さ設定
            if (leftPanel.RowDefinitions.Count > 1)
            {
                leftPanel.RowDefinitions[1].Height = new GridLength(_windowLayoutSettings.SmartLauncherHeight);
            }

            // プロジェクトエリアの高さ設定
            if (leftPanel.RowDefinitions.Count > 4)
            {
                leftPanel.RowDefinitions[4].Height = new GridLength(_windowLayoutSettings.ProjectAreaHeight);
            }

            // グループエリアの高さ設定 - 比例的なサイズ設定に変更
            if (leftPanel.RowDefinitions.Count > 7)
            {
                // 2*の比例設定を維持し、最小高さのみ動的に調整
                leftPanel.RowDefinitions[7].Height = new GridLength(2, GridUnitType.Star);
                leftPanel.RowDefinitions[7].MinHeight = Math.Max(50, _windowLayoutSettings.GroupAreaHeight * 0.3);
            }
        }

        private void ApplyGridViewColumnWidths(Window window)
        {
            try
            {
                var mainListView = window.FindName("MainListView") as ListView;
                if (mainListView?.View is GridView gridView)
                {
                    var columnWidths = _windowLayoutSettings?.ColumnWidths;
                    if (columnWidths != null)
                    {
                        foreach (var column in gridView.Columns)
                        {
                            if (column.Header is GridViewColumnHeader header && header.Tag is string tag)
                            {
                                if (columnWidths.ContainsKey(tag))
                                {
                                    column.Width = columnWidths[tag];
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GridView列幅の設定に失敗しました: {ex.Message}");
            }
        }

        public void SaveWindowLayout(Window window)
        {
            try
            {
                if (_windowLayoutSettings == null) return;

                SaveWindowState(window);
                SaveGridLayoutSettings(window);
                SaveRowLayoutSettings(window);
                SaveGridViewColumnWidths(window);

                _projectService.SaveWindowLayout(_windowLayoutSettings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ウィンドウレイアウトの保存に失敗しました: {ex.Message}");
            }
        }

        private void SaveWindowState(Window window)
        {
            _windowLayoutSettings.IsMaximized = window.WindowState == WindowState.Maximized;
            
            if (window.WindowState != WindowState.Maximized)
            {
                _windowLayoutSettings.WindowWidth = window.ActualWidth;
                _windowLayoutSettings.WindowHeight = window.ActualHeight;
                _windowLayoutSettings.WindowLeft = window.Left;
                _windowLayoutSettings.WindowTop = window.Top;
            }
        }

        private void SaveGridLayoutSettings(Window window)
        {
            var grid = window.FindName("MainGrid") as Grid;
            if (grid?.ColumnDefinitions.Count > 0)
            {
                var leftColumn = grid.ColumnDefinitions[0];
                if (leftColumn != null)
                {
                    _windowLayoutSettings.LeftPanelWidth = leftColumn.ActualWidth;
                }
            }
        }

        private void SaveRowLayoutSettings(Window window)
        {
            var leftPanel = window.FindName("LeftPanel") as Grid;
            if (leftPanel == null) return;

            if (leftPanel.RowDefinitions.Count > 1)
            {
                _windowLayoutSettings.SmartLauncherHeight = leftPanel.RowDefinitions[1].ActualHeight;
            }

            if (leftPanel.RowDefinitions.Count > 4)
            {
                _windowLayoutSettings.ProjectAreaHeight = leftPanel.RowDefinitions[4].ActualHeight;
            }

            if (leftPanel.RowDefinitions.Count > 7)
            {
                // グループエリアは比例設定なので実際の高さを保存
                _windowLayoutSettings.GroupAreaHeight = Math.Max(50, leftPanel.RowDefinitions[7].ActualHeight);
            }
        }

        private void SaveGridViewColumnWidths(Window window)
        {
            try
            {
                var mainListView = window.FindName("MainListView") as ListView;
                if (mainListView?.View is GridView gridView && _windowLayoutSettings != null)
                {
                    var columnWidths = new Dictionary<string, double>();
                    
                    foreach (var column in gridView.Columns)
                    {
                        if (column.Header is GridViewColumnHeader header && header.Tag is string tag)
                        {
                            columnWidths[tag] = column.ActualWidth;
                        }
                    }
                    
                    _windowLayoutSettings.ColumnWidths = columnWidths;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GridView列幅の保存に失敗しました: {ex.Message}");
            }
        }

        public void SetLastSortSettings(string? property, bool isAscending)
        {
            if (_windowLayoutSettings != null)
            {
                _windowLayoutSettings.LastSortColumn = property;
                _windowLayoutSettings.IsAscending = isAscending;
            }
        }
    }
}