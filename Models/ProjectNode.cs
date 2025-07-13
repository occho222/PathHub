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

        // �t�H���_�A�C�R���ƍ��ڐ����܂ޕ\����
        public string DisplayName 
        { 
            get 
            {
                if (IsFolder)
                {
                    var childCount = GetDescendantProjectCount();
                    return $"?? {Name} ({childCount})";
                }
                else if (Project != null)
                {
                    var itemCount = Project.Items?.Count ?? 0;
                    return $"{Name} ({itemCount})";
                }
                return Name;
            }
        }

        public int Level => Parent?.Level + 1 ?? 0;

        // �t�H���_�A�C�R���̂�
        public string FolderIcon => IsFolder ? "??" : "";

        // ���ڐ��̂�
        public string ItemCountText
        {
            get
            {
                if (IsFolder)
                {
                    var childCount = GetDescendantProjectCount();
                    return $"({childCount})";
                }
                else if (Project != null)
                {
                    var itemCount = Project.Items?.Count ?? 0;
                    return $"({itemCount})";
                }
                return "";
            }
        }

        private int GetDescendantProjectCount()
        {
            int count = 0;
            foreach (var child in Children)
            {
                if (child.IsFolder)
                {
                    count += child.GetDescendantProjectCount();
                }
                else
                {
                    count++;
                }
            }
            return count;
        }

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

        // DisplayName�̍X�V���g���K�[���郁�\�b�h
        public void RefreshDisplayName()
        {
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(ItemCountText));
        }
    }
}