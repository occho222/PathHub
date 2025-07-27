using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ModernLauncher.Models
{
    public class SmartLauncherItem : INotifyPropertyChanged
    {
        private string id = string.Empty;
        private string displayName = string.Empty;
        private string icon = string.Empty;
        private SmartLauncherItemType itemType;
        private List<LauncherItem> items = new List<LauncherItem>();
        private int itemCount;

        public string Id
        {
            get => id;
            set => SetProperty(ref id, value);
        }

        public string DisplayName
        {
            get => displayName;
            set => SetProperty(ref displayName, value);
        }

        public string Icon
        {
            get => icon;
            set => SetProperty(ref icon, value);
        }

        public SmartLauncherItemType ItemType
        {
            get => itemType;
            set => SetProperty(ref itemType, value);
        }

        public List<LauncherItem> Items
        {
            get => items;
            set => SetProperty(ref items, value);
        }

        public int ItemCount
        {
            get => itemCount;
            set => SetProperty(ref itemCount, value);
        }

        public string ItemCountText => $"({ItemCount} items)";

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

    public enum SmartLauncherItemType
    {
        AllProjects,
        TodaysOpenedPaths,
        WeeklyOpenedPaths
    }
}