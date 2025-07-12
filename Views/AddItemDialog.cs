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
    public class AddItemDialog : Window
    {
        public LauncherItem? Result { get; private set; }
        
        private TextBox nameTextBox = null!;
        private TextBox pathTextBox = null!;
        private TextBox descriptionTextBox = null!;
        private ComboBox categoryComboBox = null!;
        private ListBox groupsListBox = null!;
        private readonly List<ItemGroup> availableGroups;

        public AddItemDialog(List<ItemGroup> groups)
        {
            availableGroups = groups;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Title = "�A�C�e���ǉ�";
            Width = 500;
            Height = 450;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new SolidColorBrush(Color.FromRgb(236, 233, 216));

            var grid = new Grid { Margin = new Thickness(20) };
            
            // Row definitions
            for (int i = 0; i < 8; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Name
            var nameLabel = new TextBlock
            {
                Text = "���O:",
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
                Text = "�p�X:",
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 5)
            };
            Grid.SetRow(pathLabel, 2);
            grid.Children.Add(pathLabel);

            var pathPanel = new DockPanel { Margin = new Thickness(0, 0, 0, 10) };
            var browseButton = new Button
            {
                Content = "�Q��...",
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
                Text = "����:",
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
            categoryComboBox.Items.Add("�A�v���P�[�V����");
            categoryComboBox.Items.Add("�Q�[��");
            categoryComboBox.Items.Add("�c�[��");
            categoryComboBox.Items.Add("�h�L�������g");
            categoryComboBox.Items.Add("���̑�");
            categoryComboBox.SelectedIndex = 0;
            Grid.SetRow(categoryComboBox, 5);
            grid.Children.Add(categoryComboBox);

            // Description
            var descLabel = new TextBlock
            {
                Text = "����:",
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

            // Groups
            var groupsLabel = new TextBlock
            {
                Text = "�O���[�v:",
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 5)
            };
            Grid.SetRow(groupsLabel, 8);
            grid.Children.Add(groupsLabel);

            groupsListBox = new ListBox
            {
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 20),
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
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };

            var okButton = new Button
            {
                Content = "OK",
                Width = 80,
                FontSize = 13,
                Padding = new Thickness(8, 5, 8, 5),
                Margin = new Thickness(5, 0, 0, 0),
                IsDefault = true
            };
            okButton.Click += OkButton_Click;

            var cancelButton = new Button
            {
                Content = "�L�����Z��",
                Width = 80,
                FontSize = 13,
                Padding = new Thickness(8, 5, 8, 5),
                IsCancel = true
            };

            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(okButton);
            Grid.SetRow(buttonPanel, 10);
            grid.Children.Add(buttonPanel);

            Content = grid;

            nameTextBox.Focus();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "���s�t�@�C����I��",
                Filter = "���s�t�@�C�� (*.exe)|*.exe|���ׂẴt�@�C�� (*.*)|*.*",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                pathTextBox.Text = openFileDialog.FileName;
                
                // �t�@�C�������疼�O�������ݒ�
                if (string.IsNullOrWhiteSpace(nameTextBox.Text))
                {
                    nameTextBox.Text = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                }
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("���O����͂��Ă��������B", "�G���[", MessageBoxButton.OK, MessageBoxImage.Warning);
                nameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(pathTextBox.Text))
            {
                MessageBox.Show("�p�X����͂��Ă��������B", "�G���[", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                Id = Guid.NewGuid().ToString(),
                Name = nameTextBox.Text.Trim(),
                Path = pathTextBox.Text.Trim(),
                Description = descriptionTextBox.Text.Trim(),
                Category = categoryComboBox.Text?.Trim() ?? "���̑�",
                GroupIds = selectedGroups,
                OrderIndex = 0
            };

            DialogResult = true;
        }

        public void SetInitialValues(string name, string path, string category, string description)
        {
            nameTextBox.Text = name;
            pathTextBox.Text = path;
            descriptionTextBox.Text = description;
            
            // �J�e�S����ݒ�i�����̃A�C�e������I���܂��̓e�L�X�g�ݒ�j
            bool categoryFound = false;
            foreach (var item in categoryComboBox.Items)
            {
                if (item.ToString() == category)
                {
                    categoryComboBox.SelectedItem = item;
                    categoryFound = true;
                    break;
                }
            }
            
            if (!categoryFound)
            {
                categoryComboBox.Text = category;
            }
        }
    }
}