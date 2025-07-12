using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ModernLauncher.Views
{
    public class HelpDialog : Window
    {
        public HelpDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Title = "ヘルプ - Pathhub";
            Width = 600;
            Height = 500;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new SolidColorBrush(Color.FromRgb(236, 233, 216));

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Padding = new Thickness(20)
            };

            var stackPanel = new StackPanel();

            // Title
            var titleBlock = new TextBlock
            {
                Text = "Pathhub ",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            stackPanel.Children.Add(titleBlock);

            // Version
            var versionBlock = new TextBlock
            {
                Text = "バージョン 1.4.0",
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 20),
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102))
            };
            stackPanel.Children.Add(versionBlock);

            // Basic Usage
            AddSection(stackPanel, "基本的な使い方", new[]
            {
                "• アイテム追加: ツールバーの「アイテム追加」ボタンまたは Ctrl+I",
                "• ドラッグ&ドロップ: ファイルやフォルダをメイン画面にドロップして追加",
                "• Webサイト追加: ブラウザからWebページやURLを直接ドラッグ&ドロップ",
                "• グループ追加: ツールバーの「ラベル」ボタンまたは Ctrl+G",
                "• 新規プロジェクト: プロジェクトバーの「新規」ボタンまたは Ctrl+N",
                "• データ保存: Ctrl+S（自動保存も有効）",
                "• アイテム起動: アイテムをダブルクリック"
            });

            // Keyboard Shortcuts
            AddSection(stackPanel, "キーボードショートカット", new[]
            {
                "Ctrl+N : 新規プロジェクト作成",
                "Ctrl+I : アイテム追加",
                "Ctrl+G : グループ追加",
                "Ctrl+S : データ保存",
                "F1 : ヘルプ表示",
                "Delete : 選択アイテム削除",
                "Enter : 選択アイテム起動"
            });

            // Features
            AddSection(stackPanel, "主な機能", new[]
            {
                "• プロジェクト管理: 複数のプロジェクトを作成・管理",
                "• グループ分け: アイテムをカテゴリごとに整理",
                "• ドラッグ&ドロップ: ファイル、フォルダ、WebサイトのURL等を直接ドロップして追加",
                "• 検索機能: 名前、パス、分類、説明での絞り込み検索",
                "• 並び替え: アイテムの順序をドラッグまたはボタンで変更",
                "• データの永続化: プロジェクトとアイテムの自動保存",
                "• 分類別色付け: ファイル種類やサービス別に自動で色分け表示",
                "• 色設定カスタマイズ: Tools > Color Settingsから色をカスタマイズ可能",
                "• クラシックUI: FenrirFS風の親しみやすいインターフェース"
            });

            // Tips
            AddSection(stackPanel, "使用のコツ", new[]
            {
                "• よく使うアプリは「よく使う」グループに追加",
                "• 分類を活用してアイテムを整理",
                "• 検索機能で素早くアイテムを見つける",
                "• プロジェクトを用途別に分けて管理",
                "• ドラッグ&ドロップで簡単にアイテムを追加",
                "• ブラウザからWebページを直接ドラッグして追加可能",
                "• 説明欄を活用してアイテムの詳細情報を記録",
                "• 色設定で分類をより見やすくカスタマイズ",
                "• GitHubやGitLabのURLは自動で専用色で表示",
                "• ExcelやWordファイルは対応アプリの色で表示",
                "• Microsoft TeamsとSharePointのリンクも自動判定",
                "• G:ドライブのGoogleドライブファイルも自動で色分け"
            });

            // About
            AddSection(stackPanel, "このソフトについて", new[]
            {
                "MVVMパターンとSOLID原則に従って設計されており、",
                "拡張性と保守性を重視した構造になっています。",
                "",
                "開発: ModernLauncher Team",
                "技術: .NET 6.0, WPF, MVVM"
            });

            scrollViewer.Content = stackPanel;

            // Close button
            var buttonPanel = new DockPanel
            {
                Margin = new Thickness(20),
                LastChildFill = false
            };

            var closeButton = new Button
            {
                Content = "閉じる",
                Width = 80,
                FontSize = 13,
                Padding = new Thickness(8, 5, 8, 5),
                IsDefault = true,
                IsCancel = true
            };
            closeButton.Click += (s, e) => Close();
            DockPanel.SetDock(closeButton, Dock.Right);
            buttonPanel.Children.Add(closeButton);

            var mainPanel = new DockPanel();
            DockPanel.SetDock(buttonPanel, Dock.Bottom);
            mainPanel.Children.Add(buttonPanel);
            mainPanel.Children.Add(scrollViewer);

            Content = mainPanel;
        }

        private void AddSection(StackPanel parent, string title, string[] items)
        {
            var titleBlock = new TextBlock
            {
                Text = title,
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 20, 0, 10)
            };
            parent.Children.Add(titleBlock);

            foreach (var item in items)
            {
                var itemBlock = new TextBlock
                {
                    Text = item,
                    FontSize = 13,
                    Margin = new Thickness(10, 2, 0, 2),
                    TextWrapping = TextWrapping.Wrap
                };
                parent.Children.Add(itemBlock);
            }
        }
    }
}