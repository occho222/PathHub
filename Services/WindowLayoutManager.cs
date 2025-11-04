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
                System.Diagnostics.Debug.WriteLine($"���O�E�B���h�E���C�A�E�g�̓ǂݍ��݂Ɏ��s���܂���: {ex.Message}");
                _windowLayoutSettings = new WindowLayoutSettings();
                CenterWindowOnScreen(window);
            }
        }

        private void ApplyWindowPosition(Window window)
        {
            if (_windowLayoutSettings == null) return;

            // �E�B���h�E�T�C�Y��ݒ�
            if (_windowLayoutSettings.WindowWidth > 0 && _windowLayoutSettings.WindowHeight > 0)
            {
                window.Width = _windowLayoutSettings.WindowWidth;
                window.Height = _windowLayoutSettings.WindowHeight;
            }

            // �E�B���h�E�ʒu��ݒ�
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

            // �ő剻��Ԃ�ݒ�
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
                System.Diagnostics.Debug.WriteLine($"���C�A�E�g�ݒ�̓K�p�Ɏ��s���܂���: {ex.Message}");
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

            // SmartLauncher�̍����ݒ�
            if (leftPanel.RowDefinitions.Count > 1)
            {
                leftPanel.RowDefinitions[1].Height = new GridLength(_windowLayoutSettings.SmartLauncherHeight);
            }

            // �v���W�F�N�g�G���A�̍����ݒ�
            if (leftPanel.RowDefinitions.Count > 4)
            {
                leftPanel.RowDefinitions[4].Height = new GridLength(_windowLayoutSettings.ProjectAreaHeight);
            }

            // �O���[�v�G���A�̍����ݒ� - ���I�ȃT�C�Y�ݒ�ɕύX
            if (leftPanel.RowDefinitions.Count > 7)
            {
                // 2*�̔��ݒ���ێ����A�ŏ������̂ݓ��I�ɒ���
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
                    // 列順序の復元
                    var columnOrder = _windowLayoutSettings?.ColumnOrder;
                    if (columnOrder != null && columnOrder.Count > 0)
                    {
                        var columnMap = new Dictionary<string, GridViewColumn>();
                        foreach (var column in gridView.Columns)
                        {
                            if (column.Header is GridViewColumnHeader header && header.Tag is string tag)
                            {
                                columnMap[tag] = column;
                            }
                        }

                        // 保存された順序に基づいて列を並び替え
                        gridView.Columns.Clear();
                        foreach (var tag in columnOrder)
                        {
                            if (columnMap.ContainsKey(tag))
                            {
                                gridView.Columns.Add(columnMap[tag]);
                            }
                        }

                        // マップにあるが順序リストにない列を末尾に追加（新しく追加された列の場合）
                        foreach (var kvp in columnMap)
                        {
                            if (!columnOrder.Contains(kvp.Key))
                            {
                                gridView.Columns.Add(kvp.Value);
                            }
                        }
                    }

                    // 列幅の復元
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
                System.Diagnostics.Debug.WriteLine($"GridView�񕝂̐ݒ�Ɏ��s���܂���: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"�E�B���h�E���C�A�E�g�̕ۑ��Ɏ��s���܂���: {ex.Message}");
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
                // �O���[�v�G���A�͔��ݒ�Ȃ̂Ŏ��ۂ̍�����ۑ�
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
                    var columnOrder = new List<string>();

                    foreach (var column in gridView.Columns)
                    {
                        if (column.Header is GridViewColumnHeader header && header.Tag is string tag)
                        {
                            columnWidths[tag] = column.ActualWidth;
                            columnOrder.Add(tag);
                        }
                    }

                    _windowLayoutSettings.ColumnWidths = columnWidths;
                    _windowLayoutSettings.ColumnOrder = columnOrder;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GridView�񕝂̕ۑ��Ɏ��s���܂���: {ex.Message}");
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