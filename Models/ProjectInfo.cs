using System;

namespace ModernLauncher.Models
{
    public class ProjectInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public string? ParentId { get; set; } // �K�w�\���̂��߂̐e�v���W�F�N�gID
        public bool IsFolder { get; set; } // �t�H���_�[���ǂ����������t���O
    }
}