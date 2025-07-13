using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using ModernLauncher.Models;

namespace ModernLauncher.Views
{
    public class EditItemDialog : Window
    {
        public LauncherItem? Result { get; private set; }
        
        private TextBox nameTextBox = null!;
        private TextBox pathTextBox = null!;
        private TextBox descriptionTextBox = null!;
        private ComboBox categoryComboBox = null!;
        private ListBox groupsListBox = null!;
        private readonly List<ItemGroup> availableGroups;
        private readonly LauncherItem originalItem;

        public EditItemDialog(LauncherItem item, List<ItemGroup> groups)
        {
            originalItem = item;
            availableGroups = groups;
            InitializeComponent();
            LoadItemData();
        }

        private void InitializeComponent()
        {
            Title = "アイテム編集";
            Width = 500;
            Height = 520; // 高さを増やしてグループ部分が見えるようにする
            MinWidth = 420; // 最小幅を設定
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new SolidColorBrush(Color.FromRgb(236, 233, 216));

            var grid = new Grid { Margin = new Thickness(20) };
            
            // Row definitions - 正しい構造に修正
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row 0 - Name Label
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row 1 - Name TextBox
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row 2 - Path Label
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row 3 - Path Panel
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row 4 - Category Label
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row 5 - Category ComboBox
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row 6 - Description Label
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row 7 - Description TextBox
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row 8 - Groups Label
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(120, GridUnitType.Pixel) }); // Row 9 - Groups ListBox (固定高さ)
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row 10 - Button panel

            // Name
            var nameLabel = new TextBlock
            {
                Text = "名前:",
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 5)
            };
            Grid.SetRow(nameLabel, 0);
            grid.Children.Add(nameLabel);

            nameTextBox = new TextBox
            {
                FontSize = 13,
                Padding = new Thickness(6, 4, 6, 4),
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(nameTextBox, 1);
            grid.Children.Add(nameTextBox);

            // Path
            var pathLabel = new TextBlock
            {
                Text = "パス:",
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 5)
            };
            Grid.SetRow(pathLabel, 2);
            grid.Children.Add(pathLabel);

            var pathPanel = new DockPanel { Margin = new Thickness(0, 0, 0, 10) };
            var browseButton = new Button
            {
                Content = "参照...",
                Width = 80,
                FontSize = 13,
                Padding = new Thickness(8, 5, 8, 5),
                Margin = new Thickness(5, 0, 0, 0)
            };
            browseButton.Click += BrowseButton_Click;
            DockPanel.SetDock(browseButton, Dock.Right);

            pathTextBox = new TextBox
            {
                FontSize = 13,
                Padding = new Thickness(6, 4, 6, 4)
            };
            pathPanel.Children.Add(browseButton);
            pathPanel.Children.Add(pathTextBox);
            Grid.SetRow(pathPanel, 3);
            grid.Children.Add(pathPanel);

            // Category
            var categoryLabel = new TextBlock
            {
                Text = "分類:",
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 5)
            };
            Grid.SetRow(categoryLabel, 4);
            grid.Children.Add(categoryLabel);

            categoryComboBox = new ComboBox
            {
                FontSize = 13,
                Padding = new Thickness(6, 4, 6, 4),
                Margin = new Thickness(0, 0, 0, 10),
                IsEditable = true
            };
            categoryComboBox.Items.Add("アプリケーション");
            categoryComboBox.Items.Add("ゲーム");
            categoryComboBox.Items.Add("ツール");
            categoryComboBox.Items.Add("ドキュメント");
            categoryComboBox.Items.Add("その他");
            Grid.SetRow(categoryComboBox, 5);
            grid.Children.Add(categoryComboBox);

            // Description
            var descLabel = new TextBlock
            {
                Text = "説明:",
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 5)
            };
            Grid.SetRow(descLabel, 6);
            grid.Children.Add(descLabel);

            descriptionTextBox = new TextBox
            {
                FontSize = 13,
                Padding = new Thickness(6, 4, 6, 4),
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(descriptionTextBox, 7);
            grid.Children.Add(descriptionTextBox);

            // Groups Label
            var groupsLabel = new TextBlock
            {
                Text = "グループ:",
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 5)
            };
            Grid.SetRow(groupsLabel, 8);
            grid.Children.Add(groupsLabel);

            // Groups ListBox
            groupsListBox = new ListBox
            {
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 15),
                SelectionMode = SelectionMode.Multiple
            };
            
            foreach (var group in availableGroups.Where(g => g.Id != "all"))
            {
                var checkBox = new CheckBox
                {
                    Content = group.Name,
                    Tag = group,
                    FontSize = 13,
                    Margin = new Thickness(4, 2, 4, 2)
                };
                groupsListBox.Items.Add(checkBox);
            }
            
            Grid.SetRow(groupsListBox, 9);
            grid.Children.Add(groupsListBox);

            // Buttons
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var okButton = new Button
            {
                Content = "OK",
                Width = 80,
                FontSize = 13,
                Padding = new Thickness(8, 5, 8, 5),
                Margin = new Thickness(0, 0, 5, 0)
            };
            okButton.Click += OkButton_Click;

            var cancelButton = new Button
            {
                Content = "キャンセル",
                Width = 80,
                FontSize = 13,
                Padding = new Thickness(8, 5, 8, 5)
            };
            cancelButton.Click += (s, e) => DialogResult = false;

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            Grid.SetRow(buttonPanel, 10);
            grid.Children.Add(buttonPanel);

            Content = grid;

            nameTextBox.Focus();
        }

        private void LoadItemData()
        {
            nameTextBox.Text = originalItem.Name;
            pathTextBox.Text = originalItem.Path;
            descriptionTextBox.Text = originalItem.Description ?? string.Empty;
            categoryComboBox.Text = originalItem.Category ?? "その他";

            // Set selected groups
            if (originalItem.GroupIds != null)
            {
                foreach (CheckBox checkBox in groupsListBox.Items)
                {
                    if (checkBox.Tag is ItemGroup group && originalItem.GroupIds.Contains(group.Id))
                    {
                        checkBox.IsChecked = true;
                    }
                }
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "実行ファイルを選択",
                Filter = "実行ファイル (*.exe)|*.exe|すべてのファイル (*.*)|*.*",
                FilterIndex = 1,
                FileName = pathTextBox.Text
            };

            if (openFileDialog.ShowDialog() == true)
            {
                pathTextBox.Text = openFileDialog.FileName;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("名前を入力してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                nameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(pathTextBox.Text))
            {
                MessageBox.Show("パスを入力してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                pathTextBox.Focus();
                return;
            }

            var selectedGroups = new List<string>();
            foreach (CheckBox checkBox in groupsListBox.Items)
            {
                if (checkBox.IsChecked == true && checkBox.Tag is ItemGroup group)
                {
                    selectedGroups.Add(group.Id);
                }
            }

            Result = new LauncherItem
            {
                Id = originalItem.Id,
                Name = nameTextBox.Text.Trim(),
                Path = pathTextBox.Text.Trim(),
                Description = descriptionTextBox.Text.Trim(),
                Category = categoryComboBox.Text?.Trim() ?? "その他",
                GroupIds = selectedGroups,
                OrderIndex = originalItem.OrderIndex
            };

            DialogResult = true;
        }
    }
}