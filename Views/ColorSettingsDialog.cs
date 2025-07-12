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
            Title = "�F�ݒ� - ���ޕʃJ���[�ݒ�";
            Width = 500;
            Height = 600;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new SolidColorBrush(Color.FromRgb(236, 233, 216));
            ResizeMode = ResizeMode.NoResize;

            var mainGrid = new Grid();
            Content = mainGrid;

            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // �X�N���[���r���[�A
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Padding = new Thickness(20)
            };
            Grid.SetRow(scrollViewer, 0);
            mainGrid.Children.Add(scrollViewer);

            var stackPanel = new StackPanel();
            scrollViewer.Content = stackPanel;

            // �^�C�g��
            var titleBlock = new TextBlock
            {
                Text = "���ޕʐF�ݒ�",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            stackPanel.Children.Add(titleBlock);

            // ����
            var descBlock = new TextBlock
            {
                Text = "�e���ނ̕\���F��ݒ�ł��܂��B16�i���J���[�R�[�h�i��: #FF0000�j�œ��͂��Ă��������B",
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 20),
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102))
            };
            stackPanel.Children.Add(descBlock);

            // �e���ނ̐F�ݒ�
            var categoryColors = CategoryColorSettings.Instance.GetAllCategoryColors();
            var categories = categoryColors.Keys.OrderBy(k => k).ToList();

            foreach (var category in categories)
            {
                var categoryPanel = CreateCategoryPanel(category, categoryColors[category]);
                stackPanel.Children.Add(categoryPanel);
            }

            // �{�^���p�l��
            var buttonPanel = new DockPanel
            {
                Margin = new Thickness(20),
                LastChildFill = false
            };
            Grid.SetRow(buttonPanel, 1);
            mainGrid.Children.Add(buttonPanel);

            var resetButton = new Button
            {
                Content = "�f�t�H���g�ɖ߂�",
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
                Content = "�L�����Z��",
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

            // �J�e�S����
            var categoryLabel = new TextBlock
            {
                Text = category,
                Width = 120,
                FontSize = 13,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            panel.Children.Add(categoryLabel);

            // �F�v���r���[
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

            // �J���[�R�[�h����
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

            // ���O��`�F�I���R���{�{�b�N�X
            var colorComboBox = new ComboBox
            {
                Width = 100,
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 0)
            };
            
            var predefinedColors = new[]
            {
                new { Name = "��", Value = "#FF0000" },
                new { Name = "��", Value = "#008000" },
                new { Name = "��", Value = "#0000FF" },
                new { Name = "��", Value = "#FFFF00" },
                new { Name = "�I�����W", Value = "#FFA500" },
                new { Name = "��", Value = "#800080" },
                new { Name = "�s���N", Value = "#FFC0CB" },
                new { Name = "��", Value = "#A52A2A" },
                new { Name = "�O���[", Value = "#808080" },
                new { Name = "��", Value = "#000000" },
                new { Name = "Excel��", Value = "#217346" },
                new { Name = "Word��", Value = "#2B579A" },
                new { Name = "PowerPoint��", Value = "#D24726" },
                new { Name = "GitHub��", Value = "#24292e" },
                new { Name = "GitLab��", Value = "#FC6D26" },
                new { Name = "Google��", Value = "#4285F4" }
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
            var result = MessageBox.Show("���ׂĂ̐F�ݒ���f�t�H���g�ɖ߂��܂����H", "�m�F",
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
            // �ݒ��ۑ�
            foreach (var kvp in colorTextBoxes)
            {
                CategoryColorSettings.Instance.SetColorForCategory(kvp.Key, kvp.Value.Text);
            }
            
            DialogResult = true;
        }
    }
}