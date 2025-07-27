using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ModernLauncher.Models
{
    /// <summary>
    /// ウィンドウレイアウト設定を保存・復元するためのモデル
    /// </summary>
    public class WindowLayoutSettings : INotifyPropertyChanged
    {
        private double windowWidth = 1400;
        private double windowHeight = 800;
        private double windowLeft = 100;
        private double windowTop = 100;
        private bool isMaximized = false;
        
        // 左側パネルの設定
        private double leftPanelWidth = 400;
        private double smartLauncherHeight = 100;
        private double projectAreaHeight = 200;
        private double groupAreaHeight = 60;
        
        // GridView列幅の設定
        private Dictionary<string, double> columnWidths = new Dictionary<string, double>
        {
            { "Category", 130 },
            { "Name", 240 },
            { "Description", 200 },
            { "Path", 280 },
            { "GroupNames", 120 },
            { "ProjectName", 150 },
            { "FolderPath", 120 },
            { "LastAccessed", 140 }
        };
        
        // ソート設定
        private string? lastSortColumn;
        private bool isAscending = true;

        public double WindowWidth
        {
            get => windowWidth;
            set => SetProperty(ref windowWidth, value);
        }

        public double WindowHeight
        {
            get => windowHeight;
            set => SetProperty(ref windowHeight, value);
        }

        public double WindowLeft
        {
            get => windowLeft;
            set => SetProperty(ref windowLeft, value);
        }

        public double WindowTop
        {
            get => windowTop;
            set => SetProperty(ref windowTop, value);
        }

        public bool IsMaximized
        {
            get => isMaximized;
            set => SetProperty(ref isMaximized, value);
        }

        public double LeftPanelWidth
        {
            get => leftPanelWidth;
            set => SetProperty(ref leftPanelWidth, value);
        }

        public double SmartLauncherHeight
        {
            get => smartLauncherHeight;
            set => SetProperty(ref smartLauncherHeight, value);
        }

        public double ProjectAreaHeight
        {
            get => projectAreaHeight;
            set => SetProperty(ref projectAreaHeight, value);
        }

        public double GroupAreaHeight
        {
            get => groupAreaHeight;
            set => SetProperty(ref groupAreaHeight, value);
        }

        public Dictionary<string, double> ColumnWidths
        {
            get => columnWidths;
            set => SetProperty(ref columnWidths, value);
        }

        public string? LastSortColumn
        {
            get => lastSortColumn;
            set => SetProperty(ref lastSortColumn, value);
        }

        public bool IsAscending
        {
            get => isAscending;
            set => SetProperty(ref isAscending, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}