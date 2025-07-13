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
        public string MenuFile => "?? ファイル(F)";
        public string MenuEdit => "?? 編集(E)";
        public string MenuView => "??? 表示(V)";
        public string MenuTools => "?? ツール(T)";
        public string MenuHelp => "? ヘルプ(H)";

        // Toolbar buttons
        public string AddItem => "アイテム追加";
        public string Archive => "?? エクスポート";
        public string Delete => "削除";
        public string AddGroup => "グループ追加";
        public string Search => "?? 検索:";

        // Project bar
        public string WorkTray => "?? 選択項目操作";
        public string New => "?? 新規";
        public string Import => "?? インポート";

        // Left panel
        public string SmartFolders => "??? グループ";
        public string Projects => "?? プロジェクト";
        public string NewFolder => "?? 新規フォルダー";
        public string NewProjectInFolder => "?? 新規プロジェクト";
        public string DeleteProjectFolder => "??? 削除";
        public string MoveToFolder => "?? 移動";

        // ListView headers
        public string Type => "?? 種類";
        public string Category => "??? 分類";
        public string Name => "?? 名前";
        public string Path => "?? パス";
        public string Group => "??? グループ";
        public string Description => "?? 説明";
        public string Actions => "? 操作";

        // Action buttons
        public string Up => "上";
        public string Down => "下";
        public string Edit => "編集";
        public string DeleteShort => "削";

        // Status bar
        public string StatusHelp => "?? Ctrl+N 新規 | Ctrl+I アイテム追加 | Ctrl+G グループ追加 | F1 ヘルプ | ??? ドラッグ&ドロップでアイテム追加";

        // Drag & Drop
        public string DropMessage => "?? ファイルやフォルダをここにドロップしてアイテムを追加";

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}