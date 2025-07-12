using System;
using System.Collections.ObjectModel;

namespace ModernLauncher.Models
{
    public class Project
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public string? ParentId { get; set; } // �K�w�\���̂��߂̐e�v���W�F�N�gID
        public bool IsFolder { get; set; } // �t�H���_�[���ǂ����������t���O
        public ObservableCollection<ItemGroup> Groups { get; set; } = new ObservableCollection<ItemGroup>();
        public ObservableCollection<LauncherItem> Items { get; set; } = new ObservableCollection<LauncherItem>();
    }
}