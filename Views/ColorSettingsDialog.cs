using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ModernLauncher.Models;

namespace ModernLauncher.Views
{
    public partial class ColorSettingsDialog : Window
    {
        private readonly Dictionary<string, TextBox> colorTextBoxes = new Dictionary<string, TextBox>();
        private readonly Dictionary<string, Border> colorPreviews = new Dictionary<string, Border>();

        public ColorSettingsDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Title = "色設定 - 分類別カラー設定";
            Width = 500;
            Height = 600;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new SolidColorBrush(Color.FromRgb(236, 233, 216));
            ResizeMode = ResizeMode.NoResize;

            var mainGrid = new Grid();
            Content = mainGrid;

            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // スクロールビューア
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Padding = new Thickness(20)
            };
            Grid.SetRow(scrollViewer, 0);
            mainGrid.Children.Add(scrollViewer);

            var stackPanel = new StackPanel();
            scrollViewer.Content = stackPanel;

            // タイトル
            var titleBlock = new TextBlock
            {
                Text = "分類別色設定",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            stackPanel.Children.Add(titleBlock);

            // 説明
            var descBlock = new TextBlock
            {
                Text = "各分類の表示色を設定できます。16進数カラーコード（例: #FF0000）で入力してください。",
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 20),
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102))
            };
            stackPanel.Children.Add(descBlock);

            // 各分類の色設定
            var categoryColors = CategoryColorSettings.Instance.GetAllCategoryColors();
            var categories = categoryColors.Keys.OrderBy(k => k).ToList();

            foreach (var category in categories)
            {
                var categoryPanel = CreateCategoryPanel(category, categoryColors[category]);
                stackPanel.Children.Add(categoryPanel);
            }

            // ボタンパネル
            var buttonPanel = new DockPanel
            {
                Margin = new Thickness(20),
                LastChildFill = false
            };
            Grid.SetRow(buttonPanel, 1);
            mainGrid.Children.Add(buttonPanel);

            var resetButton = new Button
            {
                Content = "デフォルトに戻す",
                Width = 120,
                FontSize = 13,
                Padding = new Thickness(8, 5, 8, 5),
                Margin = new Thickness(0, 0, 10, 0)
            };
            resetButton.Click += ResetButton_Click;
            DockPanel.SetDock(resetButton, Dock.Left);
            buttonPanel.Children.Add(resetButton);

            var cancelButton = new Button
            {
                Content = "キャンセル",
                Width = 80,
                FontSize = 13,
                Padding = new Thickness(8, 5, 8, 5),
                Margin = new Thickness(0, 0, 10, 0),
                IsCancel = true
            };
            cancelButton.Click += (s, e) => DialogResult = false;
            DockPanel.SetDock(cancelButton, Dock.Right);
            buttonPanel.Children.Add(cancelButton);

            var okButton = new Button
            {
                Content = "OK",
                Width = 80,
                FontSize = 13,
                Padding = new Thickness(8, 5, 8, 5),
                IsDefault = true
            };
            okButton.Click += OkButton_Click;
            DockPanel.SetDock(okButton, Dock.Right);
            buttonPanel.Children.Add(okButton);
        }

        private StackPanel CreateCategoryPanel(string category, string color)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 5)
            };

            // カテゴリ名
            var categoryLabel = new TextBlock
            {
                Text = category,
                Width = 120,
                FontSize = 13,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            panel.Children.Add(categoryLabel);

            // 色プレビュー
            var colorPreview = new Border
            {
                Width = 30,
                Height = 20,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 10, 0)
            };
            UpdateColorPreview(colorPreview, color);
            colorPreviews[category] = colorPreview;
            panel.Children.Add(colorPreview);

            // カラーコード入力
            var colorTextBox = new TextBox
            {
                Text = color,
                Width = 100,
                FontSize = 13,
                Padding = new Thickness(6, 4, 6, 4),
                Margin = new Thickness(0, 0, 10, 0)
            };
            colorTextBox.TextChanged += (s, e) => OnColorTextChanged(category, colorTextBox.Text);
            colorTextBoxes[category] = colorTextBox;
            panel.Children.Add(colorTextBox);

            // 事前定義色選択コンボボックス
            var colorComboBox = new ComboBox
            {
                Width = 100,
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 0)
            };
            
            var predefinedColors = new[]
            {
                new { Name = "赤", Value = "#FF0000" },
                new { Name = "緑", Value = "#008000" },
                new { Name = "青", Value = "#0000FF" },
                new { Name = "黄", Value = "#FFFF00" },
                new { Name = "オレンジ", Value = "#FFA500" },
                new { Name = "紫", Value = "#800080" },
                new { Name = "ピンク", Value = "#FFC0CB" },
                new { Name = "茶", Value = "#A52A2A" },
                new { Name = "グレー", Value = "#808080" },
                new { Name = "黒", Value = "#000000" },
                new { Name = "Excel緑", Value = "#217346" },
                new { Name = "Word青", Value = "#2B579A" },
                new { Name = "PowerPoint橙", Value = "#D24726" },
                new { Name = "GitHub黒", Value = "#24292e" },
                new { Name = "GitLab橙", Value = "#FC6D26" },
                new { Name = "Google青", Value = "#4285F4" }
            };

            colorComboBox.ItemsSource = predefinedColors;
            colorComboBox.DisplayMemberPath = "Name";
            colorComboBox.SelectedValuePath = "Value";
            colorComboBox.SelectionChanged += (s, e) =>
            {
                if (colorComboBox.SelectedValue is string selectedColor)
                {
                    colorTextBox.Text = selectedColor;
                    OnColorTextChanged(category, selectedColor);
                }
            };
            panel.Children.Add(colorComboBox);

            return panel;
        }

        private void OnColorTextChanged(string category, string colorText)
        {
            if (colorPreviews.TryGetValue(category, out var preview))
            {
                UpdateColorPreview(preview, colorText);
            }
        }

        private void UpdateColorPreview(Border preview, string colorText)
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(colorText);
                preview.Background = new SolidColorBrush(color);
            }
            catch
            {
                preview.Background = new SolidColorBrush(Colors.LightGray);
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("すべての色設定をデフォルトに戻しますか？", "確認",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                CategoryColorSettings.Instance.ResetToDefaults();
                var categoryColors = CategoryColorSettings.Instance.GetAllCategoryColors();
                
                foreach (var kvp in categoryColors)
                {
                    if (colorTextBoxes.TryGetValue(kvp.Key, out var textBox))
                    {
                        textBox.Text = kvp.Value;
                        OnColorTextChanged(kvp.Key, kvp.Value);
                    }
                }
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // 設定を保存
            foreach (var kvp in colorTextBoxes)
            {
                CategoryColorSettings.Instance.SetColorForCategory(kvp.Key, kvp.Value.Text);
            }
            
            DialogResult = true;
        }
    }
}