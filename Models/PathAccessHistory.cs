using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ModernLauncher.Models
{
    public class PathAccessHistory : INotifyPropertyChanged
    {
        private string path = string.Empty;
        private string name = string.Empty;
        private DateTime lastAccessTime;
        private int accessCount;
        private string category = string.Empty;
        private string icon = string.Empty;
        private string itemType = string.Empty;
        private string projectName = string.Empty;

        public string Path
        {
            get => path;
            set => SetProperty(ref path, value);
        }

        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        public DateTime LastAccessTime
        {
            get => lastAccessTime;
            set => SetProperty(ref lastAccessTime, value);
        }

        public int AccessCount
        {
            get => accessCount;
            set => SetProperty(ref accessCount, value);
        }

        public string Category
        {
            get => category;
            set => SetProperty(ref category, value);
        }

        public string Icon
        {
            get => icon;
            set => SetProperty(ref icon, value);
        }

        public string ItemType
        {
            get => itemType;
            set => SetProperty(ref itemType, value);
        }

        public string ProjectName
        {
            get => projectName;
            set => SetProperty(ref projectName, value);
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