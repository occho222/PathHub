using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ModernLauncher.Models
{
    public class ProjectNode : INotifyPropertyChanged
    {
        private bool isExpanded = true;
        private bool isSelected;

        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsFolder { get; set; }
        public int OrderIndex { get; set; }
        public string? ParentId { get; set; }
        public ProjectNode? Parent { get; set; }
        public ObservableCollection<ProjectNode> Children { get; set; } = new ObservableCollection<ProjectNode>();
        public Project? Project { get; set; } // ���ۂ̃v���W�F�N�g�f�[�^�i�t�H���_�[�̏ꍇ��null�j

        public bool IsExpanded
        {
            get => isExpanded;
            set => SetProperty(ref isExpanded, value);
        }

        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }

        public string DisplayName => IsFolder ? $"[�t�H���_] {Name}" : Name;
        public int Level => Parent?.Level + 1 ?? 0;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public void AddChild(ProjectNode child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        public void RemoveChild(ProjectNode child)
        {
            child.Parent = null;
            Children.Remove(child);
        }
    }
}