using System.ComponentModel;
using System.Globalization;

namespace ModernLauncher.Resources
{
    public class LocalizedStrings : INotifyPropertyChanged
    {
        private static LocalizedStrings? instance;
        public static LocalizedStrings Instance => instance ??= new LocalizedStrings();

        public event PropertyChangedEventHandler? PropertyChanged;

        // Menu items
        public string MenuFile => "?? �t�@�C��(F)";
        public string MenuEdit => "?? �ҏW(E)";
        public string MenuView => "??? �\��(V)";
        public string MenuTools => "?? �c�[��(T)";
        public string MenuHelp => "? �w���v(H)";

        // Toolbar buttons
        public string AddItem => "�A�C�e���ǉ�";
        public string Archive => "?? �G�N�X�|�[�g";
        public string Delete => "�폜";
        public string AddGroup => "�O���[�v�ǉ�";
        public string Search => "?? ����:";

        // Project bar
        public string WorkTray => "?? �I�����ڑ���";
        public string New => "?? �V�K";
        public string Import => "?? �C���|�[�g";

        // Left panel
        public string SmartFolders => "??? �O���[�v";
        public string Projects => "?? �v���W�F�N�g";
        public string NewFolder => "?? �V�K�t�H���_�[";
        public string NewProjectInFolder => "?? �V�K�v���W�F�N�g";
        public string DeleteProjectFolder => "??? �폜";
        public string MoveToFolder => "?? �ړ�";

        // ListView headers
        public string Type => "?? ���";
        public string Category => "??? ����";
        public string Name => "?? ���O";
        public string Path => "?? �p�X";
        public string Group => "??? �O���[�v";
        public string Description => "?? ����";
        public string Actions => "? ����";

        // Action buttons
        public string Up => "��";
        public string Down => "��";
        public string Edit => "�ҏW";
        public string DeleteShort => "��";

        // Status bar
        public string StatusHelp => "?? Ctrl+N �V�K | Ctrl+I �A�C�e���ǉ� | Ctrl+G �O���[�v�ǉ� | F1 �w���v | ??? �h���b�O&�h���b�v�ŃA�C�e���ǉ�";

        // Drag & Drop
        public string DropMessage => "?? �t�@�C����t�H���_�������Ƀh���b�v���ăA�C�e����ǉ�";

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}