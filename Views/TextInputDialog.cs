using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ModernLauncher.Views
{
    public class TextInputDialog : Window
    {
        public string InputText { get; private set; } = string.Empty;
        private TextBox textBox;

        public TextInputDialog(string title, string prompt)
        {
            Title = title;
            Width = 450;
            Height = 220;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            var grid = new Grid { Margin = new Thickness(24) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var promptBlock = new TextBlock
            {
                Text = prompt,
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 12)
            };
            grid.Children.Add(promptBlock);

            textBox = new TextBox
            {
                FontSize = 14,
                Padding = new Thickness(10, 6, 10, 6),
                Background = Brushes.White,
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(204, 204, 204)),
                MinHeight = 30
            };
            Grid.SetRow(textBox, 1);
            grid.Children.Add(textBox);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 24, 0, 0)
            };

            var okButton = CreateButton("グループ追加", true);
            var cancelButton = CreateButton("キャンセル", false);

            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(okButton);

            Grid.SetRow(buttonPanel, 2);
            grid.Children.Add(buttonPanel);

            Content = grid;

            textBox.Focus();
            textBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    InputText = textBox.Text;
                    DialogResult = true;
                }
                else if (e.Key == Key.Escape)
                {
                    DialogResult = false;
                }
            };
        }

        private Button CreateButton(string content, bool isDefault)
        {
            var button = new Button
            {
                Content = content,
                Width = 90,
                Height = 32,
                Margin = new Thickness(6, 0, 0, 0),
                FontSize = 13,
                Background = isDefault ? new SolidColorBrush(Color.FromRgb(0, 120, 212))
                                       : new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                Foreground = isDefault ? Brushes.White
                                       : new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                BorderBrush = isDefault ? new SolidColorBrush(Color.FromRgb(0, 120, 212))
                                        : new SolidColorBrush(Color.FromRgb(204, 204, 204)),
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand
            };

            button.Click += (s, e) =>
            {
                if (isDefault)
                {
                    InputText = textBox.Text;
                    DialogResult = true;
                }
                else
                {
                    DialogResult = false;
                }
            };

            return button;
        }
    }
}