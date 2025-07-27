using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace ModernLauncher.Views
{
    /// <summary>
    /// HelpDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class HelpDialog : Window, INotifyPropertyChanged
    {
        public HelpDialog()
        {
            InitializeComponent();
            InitializeHelpContent();
            DataContext = this;
        }

        public List<string> BasicUsageItems { get; private set; }
        public List<string> KeyboardShortcuts { get; private set; }
        public List<string> ButtonGuide { get; private set; }
        public List<string> Features { get; private set; }
        public List<string> Tips { get; private set; }
        public List<string> AboutInfo { get; private set; }

        private void InitializeHelpContent()
        {
            BasicUsageItems = new List<string>
            {
                "🎯 アイテム追加: ツールバーの「➕ アイテム追加」ボタンまたは Ctrl+I",
                "📂 ドラッグ&ドロップ: ファイルやフォルダをメイン画面にドロップして追加",
                "🌐 Webサイト追加: ブラウザからWebページやURLを直接ドラッグ&ドロップ",
                "📁 グループ追加: グループセクションの「➕」ボタンまたは Ctrl+G",
                "🚀 グループ一括起動: グループを選択して「🚀一括起動」ボタンでグループ内全アイテムを起動",
                "📊 新規プロジェクト: プロジェクトセクションの「➕」ボタンまたは Ctrl+N",
                "🔍 検索機能: Ctrl+Fで検索、Ctrl+Shift+Fで全プロジェクト検索",
                "💾 データ保存: Ctrl+S（自動保存も有効）",
                "🖱️ アイテム起動: アイテムをダブルクリック"
            };

            KeyboardShortcuts = new List<string>
            {
                "🆕 Ctrl+N : 新規プロジェクト作成",
                "➕ Ctrl+I : アイテム追加",
                "📂 Ctrl+G : グループ追加",
                "🚀 Ctrl+L : グループ一括起動",
                "💾 Ctrl+S : データ保存",
                "❓ F1 : ヘルプ表示",
                "🔍 Ctrl+F : 検索ボックスにフォーカス",
                "🔍 Ctrl+Shift+F : 全プロジェクトから検索",
                "🚀 Ctrl+1 : SmartLauncherにフォーカス",
                "📁 Ctrl+2 : プロジェクト一覧にフォーカス",
                "🗂️ Ctrl+3 : グループ一覧にフォーカス",
                "📋 Ctrl+4 : メインリストにフォーカス",
                "⬇️ Tab/Down : 検索エリアからメインリストに移動",
                "❌ Escape : 検索をクリア",
                "🗑️ Delete : 選択アイテム削除",
                "▶️ Enter : 選択アイテム起動"
            };

            ButtonGuide = new List<string>
            {
                "🟢 緑色ボタン（✏️編集、➕追加）: 安全な重要操作",
                "🔴 赤色ボタン（🗑️削除）: 危険な操作（要注意）",
                "🔵 青色ボタン（⚡VSCode）: VS Code で開く",
                "🟠 橙色ボタン（📋Office）: Office アプリで開く",
                "⚪ グレーボタン（⬆️上、⬇️下、📁フォルダ、📦移動）: 補助操作"
            };

            Features = new List<string>
            {
                "📊 プロジェクト管理: 複数のプロジェクトを作成・管理",
                "🗂️ グループ分け: アイテムをカテゴリごとに整理",
                "🚀 グループ一括起動: グループ内のすべてのアイテムをワンクリックで起動",
                "📂 ドラッグ&ドロップ: ファイル、フォルダ、WebサイトのURL等を直接ドロップして追加",
                "🔍 検索機能: 名前、パス、分類、説明での絞り込み検索（Ctrl+F現在表示、Ctrl+Shift+F全プロジェクト）",
                "🔄 並び替え: アイテム、プロジェクト、グループの順序をドラッグまたはボタンで変更",
                "📊 列ヘッダーソート: 各列のヘッダーをクリックして昇順・降順で並び替え",
                "🗂️ プロジェクト並び替え: ドラッグ&ドロップまたは⬆️⬇️ボタンでプロジェクトの順序を変更",
                "🏷️ グループ並び替え: ドラッグ&ドロップまたは⬆️⬇️ボタンでグループの順序を変更",
                "💾 データの永続化: プロジェクトとアイテムの自動保存",
                "🎨 分類別色付け: ファイル種類やサービス別に自動で色分け表示",
                "🌈 色設定カスタマイズ: Tools > Color Settingsから色をカスタマイズ可能",
                "✨ モダンUI: 直感的でユーザーフレンドリーなインターフェース"
            };

            Tips = new List<string>
            {
                "⭐ よく使うアプリは「よく使う」グループに追加",
                "🚀 業務開始時は「業務」グループを一括起動で効率化",
                "🏷️ 分類を活用してアイテムを整理",
                "🔍 検索機能で素早くアイテムを見つける（Ctrl+F、Ctrl+Shift+F）",
                "📁 プロジェクトを用途別に分けて管理",
                "🖱️ ドラッグ&ドロップで簡単にアイテムを追加",
                "🌐 ブラウザからWebページを直接ドラッグして追加可能",
                "📝 説明欄を活用してアイテムの詳細情報を記録",
                "🔄 プロジェクトやグループをドラッグして順序を変更",
                "⬆️⬇️ 並び替えボタンでプロジェクトやグループの順序を調整",
                "🗂️ フォルダーを活用してプロジェクトを階層化",
                "⬇️ 検索後はTabキーまたは↓キーでメインリストに移動",
                "🌐 スマートランチャーで最近使ったアイテムに素早くアクセス",
                "💾 Ctrl+Sでいつでも手動保存が可能",
                "🎨 色設定でアイテムの見た目をカスタマイズ"
            };

            AboutInfo = new List<string>
            {
                "🏗️ MVVMパターンとSOLID原則に従って設計されており、",
                "🔧 拡張性と保守性を重視した構造になっています。",
                "",
                "👨‍💻 開発: ModernLauncher Team",
                "⚙️ 技術: .NET 6.0, WPF, MVVM",
                "🎯 目標: 効率的で美しいプロジェクト管理"
            };
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
            }
            
            base.OnKeyDown(e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}