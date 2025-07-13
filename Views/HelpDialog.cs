using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

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
            Title = "🆘 ヘルプ - PathHub";
            Width = 650;
            Height = 550;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new SolidColorBrush(Color.FromRgb(248, 249, 250));

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Padding = new Thickness(25),
                Background = new SolidColorBrush(Colors.Transparent)
            };

            var stackPanel = new StackPanel();

            // Header with app icon and title
            var headerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 30)
            };

            var appIcon = new TextBlock
            {
                Text = "🛣️",
                FontSize = 32,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new FontFamily("Segoe UI, Segoe UI Emoji, Noto Color Emoji, Arial Unicode MS")
            };
            System.Windows.Media.TextOptions.SetTextFormattingMode(appIcon, TextFormattingMode.Display);
            System.Windows.Media.TextOptions.SetTextRenderingMode(appIcon, TextRenderingMode.ClearType);
            headerPanel.Children.Add(appIcon);

            var titleBlock = new TextBlock
            {
                Text = "PathHub",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41)),
                FontFamily = new FontFamily("Segoe UI, Segoe UI Emoji, Noto Color Emoji, Arial Unicode MS")
            };
            System.Windows.Media.TextOptions.SetTextFormattingMode(titleBlock, TextFormattingMode.Display);
            System.Windows.Media.TextOptions.SetTextRenderingMode(titleBlock, TextRenderingMode.ClearType);
            headerPanel.Children.Add(titleBlock);

            stackPanel.Children.Add(headerPanel);

            // Subtitle with version
            var subtitleBlock = new TextBlock
            {
                Text = "✨ モダンなプロジェクト管理ツール v1.4.0",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 25),
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
                FontFamily = new FontFamily("Segoe UI, Segoe UI Emoji, Noto Color Emoji, Arial Unicode MS")
            };
            System.Windows.Media.TextOptions.SetTextFormattingMode(subtitleBlock, TextFormattingMode.Display);
            System.Windows.Media.TextOptions.SetTextRenderingMode(subtitleBlock, TextRenderingMode.ClearType);
            stackPanel.Children.Add(subtitleBlock);

            // Basic Usage
            AddSection(stackPanel, "🚀 基本的な使い方", new[]
            {
                "🎯 アイテム追加: ツールバーの「➕ アイテム追加」ボタンまたは Ctrl+I",
                "📂 ドラッグ&ドロップ: ファイルやフォルダをメイン画面にドロップして追加",
                "🌐 Webサイト追加: ブラウザからWebページやURLを直接ドラッグ&ドロップ",
                "📁 グループ追加: グループセクションの「➕」ボタンまたは Ctrl+G",
                "📊 新規プロジェクト: プロジェクトセクションの「➕」ボタンまたは Ctrl+N",
                "💾 データ保存: Ctrl+S（自動保存も有効）",
                "🖱️ アイテム起動: アイテムをダブルクリック"
            });

            // Keyboard Shortcuts
            AddSection(stackPanel, "⌨️ キーボードショートカット", new[]
            {
                "🆕 Ctrl+N : 新規プロジェクト作成",
                "➕ Ctrl+I : アイテム追加",
                "📂 Ctrl+G : グループ追加",
                "💾 Ctrl+S : データ保存",
                "❓ F1 : ヘルプ表示",
                "🗑️ Delete : 選択アイテム削除",
                "▶️ Enter : 選択アイテム起動"
            });

            // Button Guide
            AddSection(stackPanel, "🎨 ボタンガイド", new[]
            {
                "🟢 緑色ボタン（✏️編集、➕追加）: 安全な重要操作",
                "🔴 赤色ボタン（🗑️削除）: 危険な操作（要注意）",
                "🔵 青色ボタン（⚡VSCode）: VS Code で開く",
                "🟠 橙色ボタン（📋Office）: Office アプリで開く",
                "⚪ グレーボタン（⬆️上、⬇️下、📁フォルダ、📦移動）: 補助操作"
            });

            // Features
            AddSection(stackPanel, "🎯 主な機能", new[]
            {
                "📊 プロジェクト管理: 複数のプロジェクトを作成・管理",
                "🗂️ グループ分け: アイテムをカテゴリごとに整理",
                "📂 ドラッグ&ドロップ: ファイル、フォルダ、WebサイトのURL等を直接ドロップして追加",
                "🔍 検索機能: 名前、パス、分類、説明での絞り込み検索",
                "🔄 並び替え: アイテムの順序をドラッグまたはボタンで変更",
                "📊 列ヘッダーソート: 各列のヘッダーをクリックして昇順・降順で並び替え",
                "💾 データの永続化: プロジェクトとアイテムの自動保存",
                "🎨 分類別色付け: ファイル種類やサービス別に自動で色分け表示",
                "🌈 色設定カスタマイズ: Tools > Color Settingsから色をカスタマイズ可能",
                "✨ モダンUI: 直感的でユーザーフレンドリーなインターフェース"
            });

            // Tips
            AddSection(stackPanel, "💡 使用のコツ", new[]
            {
                "⭐ よく使うアプリは「よく使う」グループに追加",
                "🏷️ 分類を活用してアイテムを整理",
                "🔍 検索機能で素早くアイテムを見つける",
                "📁 プロジェクトを用途別に分けて管理",
                "🖱️ ドラッグ&ドロップで簡単にアイテムを追加",
                "🌐 ブラウザからWebページを直接ドラッグして追加可能",
                "📝 説明欄を活用してアイテムの詳細情報を記録",
                "🎨 色設定で分類をより見やすくカスタマイズ",
                "📊 列ヘッダーをクリックしてアイテムを並び替え（再クリックで昇順/降順切替）",
                "⬆️⬇️ ボタンで手動順序変更（列ソートは自動でクリア）",
                "🎨 色付きボタンで操作の種類を瞬時に判別",
                "🐙 GitHubやGitLabのURLは自動で専用色で表示",
                "📊 ExcelやWordファイルは対応アプリの色で表示",
                "👥 Microsoft TeamsとSharePointのリンクも自動判定",
                "☁️ G:ドライブのGoogleドライブファイルも自動で色分け"
            });

            // About
            AddSection(stackPanel, "ℹ️ このソフトについて", new[]
            {
                "🏗️ MVVMパターンとSOLID原則に従って設計されており、",
                "🔧 拡張性と保守性を重視した構造になっています。",
                "",
                "👨‍💻 開発: ModernLauncher Team",
                "⚙️ 技術: .NET 6.0, WPF, MVVM",
                "🎯 目標: 効率的で美しいプロジェクト管理"
            });

            scrollViewer.Content = stackPanel;

            // Close button with enhanced styling
            var buttonPanel = new DockPanel
            {
                Margin = new Thickness(25, 15, 25, 20),
                LastChildFill = false,
                Background = new SolidColorBrush(Color.FromRgb(248, 249, 250))
            };

            var closeButton = new Button
            {
                Content = "✨ 閉じる",
                Width = 100,
                Height = 35,
                FontSize = 13,
                Padding = new Thickness(12, 8, 12, 8),
                IsDefault = true,
                IsCancel = true,
                FontFamily = new FontFamily("Segoe UI, Segoe UI Emoji, Noto Color Emoji, Arial Unicode MS"),
                Background = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
                Foreground = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };

            // Add rounded corners effect
            closeButton.Style = CreateModernButtonStyle();
            closeButton.Click += (s, e) => Close();
            DockPanel.SetDock(closeButton, Dock.Right);
            buttonPanel.Children.Add(closeButton);

            var mainPanel = new DockPanel
            {
                Background = new SolidColorBrush(Color.FromRgb(248, 249, 250))
            };
            DockPanel.SetDock(buttonPanel, Dock.Bottom);
            mainPanel.Children.Add(buttonPanel);
            mainPanel.Children.Add(scrollViewer);

            Content = mainPanel;
        }

        private Style CreateModernButtonStyle()
        {
            var style = new Style(typeof(Button));
            
            // Create a template for rounded corners
            var template = new ControlTemplate(typeof(Button));
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            borderFactory.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Button.BorderBrushProperty));
            borderFactory.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Button.BorderThicknessProperty));
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
            borderFactory.SetValue(Border.PaddingProperty, new TemplateBindingExtension(Button.PaddingProperty));

            var contentPresenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenterFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenterFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            
            borderFactory.AppendChild(contentPresenterFactory);
            template.VisualTree = borderFactory;

            style.Setters.Add(new Setter(Button.TemplateProperty, template));
            
            return style;
        }

        private void AddSection(StackPanel parent, string title, string[] items)
        {
            // Add section divider
            if (parent.Children.Count > 1)
            {
                var divider = new Border
                {
                    Height = 1,
                    Background = new SolidColorBrush(Color.FromRgb(222, 226, 230)),
                    Margin = new Thickness(0, 15, 0, 20)
                };
                parent.Children.Add(divider);
            }

            var titleBlock = new TextBlock
            {
                Text = title,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 12),
                Foreground = new SolidColorBrush(Color.FromRgb(52, 58, 64)),
                FontFamily = new FontFamily("Segoe UI, Segoe UI Emoji, Noto Color Emoji, Arial Unicode MS")
            };
            
            // テキストレンダリングオプションを設定
            System.Windows.Media.TextOptions.SetTextFormattingMode(titleBlock, TextFormattingMode.Display);
            System.Windows.Media.TextOptions.SetTextRenderingMode(titleBlock, TextRenderingMode.ClearType);
            
            parent.Children.Add(titleBlock);

            foreach (var item in items)
            {
                var itemPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 3, 0, 3)
                };

                var bullet = new TextBlock
                {
                    Text = "▶️",
                    FontSize = 11,
                    Margin = new Thickness(15, 0, 8, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    FontFamily = new FontFamily("Segoe UI, Segoe UI Emoji, Noto Color Emoji, Arial Unicode MS")
                };
                System.Windows.Media.TextOptions.SetTextFormattingMode(bullet, TextFormattingMode.Display);
                System.Windows.Media.TextOptions.SetTextRenderingMode(bullet, TextRenderingMode.ClearType);

                var itemBlock = new TextBlock
                {
                    Text = item,
                    FontSize = 13,
                    TextWrapping = TextWrapping.Wrap,
                    FontFamily = new FontFamily("Segoe UI, Segoe UI Emoji, Noto Color Emoji, Arial Unicode MS"),
                    UseLayoutRounding = true,
                    Foreground = new SolidColorBrush(Color.FromRgb(73, 80, 87)),
                    LineHeight = 18
                };
                
                // テキストレンダリングオプションを設定
                System.Windows.Media.TextOptions.SetTextFormattingMode(itemBlock, TextFormattingMode.Display);
                System.Windows.Media.TextOptions.SetTextRenderingMode(itemBlock, TextRenderingMode.ClearType);
                
                itemPanel.Children.Add(bullet);
                itemPanel.Children.Add(itemBlock);
                parent.Children.Add(itemPanel);
            }
        }
    }
}