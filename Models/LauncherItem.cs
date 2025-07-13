using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ModernLauncher.Services;

namespace ModernLauncher.Models
{
    public class LauncherItem : INotifyPropertyChanged
    {
        private string id = string.Empty;
        private string name = string.Empty;
        private string path = string.Empty;
        private string description = string.Empty;
        private string category = "Other";
        private string groupId = string.Empty;
        private List<string> groupIds = new List<string>();
        private string icon = string.Empty;
        private string itemType = string.Empty;
        private int orderIndex;
        private string groupNames = string.Empty;

        // �V�����t�B�[���h - �t�H���_�p�X�\���p
        private string projectName = string.Empty;
        private string folderPath = string.Empty;

        public string Id
        {
            get => id;
            set => SetProperty(ref id, value);
        }

        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        public string Path
        {
            get => path;
            set
            {
                if (SetProperty(ref path, value))
                {
                    // �p�X���ύX���ꂽ���ɃA�C�R���ƃA�C�e���^�C�v���X�V
                    UpdateIconAndType();
                }
            }
        }

        public string Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }

        public string Category
        {
            get => category;
            set => SetProperty(ref category, value);
        }

        [Obsolete("Use GroupIds instead")]
        public string GroupId
        {
            get => groupId;
            set => SetProperty(ref groupId, value);
        }

        public List<string> GroupIds
        {
            get => groupIds;
            set => SetProperty(ref groupIds, value);
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

        public int OrderIndex
        {
            get => orderIndex;
            set => SetProperty(ref orderIndex, value);
        }

        public string GroupNames
        {
            get => groupNames;
            set => SetProperty(ref groupNames, value);
        }

        // �V�����v���p�e�B - �����v���W�F�N�g��
        public string ProjectName
        {
            get => projectName;
            set => SetProperty(ref projectName, value);
        }

        // �V�����v���p�e�B - �t�H���_�p�X
        public string FolderPath
        {
            get => folderPath;
            set => SetProperty(ref folderPath, value);
        }

        private void UpdateIconAndType()
        {
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    var launcherService = ServiceLocator.Instance.GetService<Interfaces.ILauncherService>();
                    if (launcherService != null)
                    {
                        Icon = launcherService.GetIconForPath(path);
                        ItemType = launcherService.GetItemType(path);
                    }
                    else
                    {
                        // �t�H�[���o�b�N: ��{�I�ȃA�C�R������
                        Icon = GetFallbackIcon(path);
                        ItemType = GetFallbackItemType(path);
                    }
                }
                catch
                {
                    // �G���[�����������ꍇ�̃t�H�[���o�b�N
                    Icon = GetFallbackIcon(path);
                    ItemType = GetFallbackItemType(path);
                }
            }
            else
            {
                Icon = "?";
                ItemType = "Unknown";
            }
        }

        private string GetFallbackIcon(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "?";

            // URL�̏ꍇ
            if (path.StartsWith("http://") || path.StartsWith("https://") || path.StartsWith("www."))
                return "??";

            // �f�B���N�g���̏ꍇ
            if (System.IO.Directory.Exists(path))
                return "??";

            // �t�@�C���̏ꍇ
            if (System.IO.File.Exists(path))
            {
                var ext = System.IO.Path.GetExtension(path).ToLower();
                return ext switch
                {
                    ".exe" or ".msi" or ".bat" or ".cmd" => "??",
                    ".txt" or ".rtf" => "??",
                    ".doc" or ".docx" => "??",
                    ".xls" or ".xlsx" => "??",
                    ".ppt" or ".pptx" => "??",
                    ".pdf" => "??",
                    ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".svg" or ".webp" => "???",
                    _ => "??"
                };
            }

            // �R�}���h�̏ꍇ
            return "?";
        }

        private string GetFallbackItemType(string path)
        {
            if (path.StartsWith("http://") || path.StartsWith("https://") || path.StartsWith("www."))
                return "Web";
            else if (System.IO.Directory.Exists(path))
                return "Folder";
            else if (System.IO.File.Exists(path))
                return "File";
            else
                return "Command";
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

        /// <summary>
        /// �A�C�R���ƃA�C�e���^�C�v�������I�ɍX�V���܂�
        /// </summary>
        public void RefreshIconAndType()
        {
            UpdateIconAndType();
        }
    }
}