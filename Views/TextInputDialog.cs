using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ModernLauncher.Models;

namespace ModernLauncher.Views
{
    public class TextInputDialog : Window
    {
        public string InputText { get; private set; } = string.Empty;
        private TextBox textBox;

        public TextInputDialog(string title, string prompt)
        {
            Title = title;
            Width = 480;
            Height = 220;
            MinWidth = 400; // �ŏ�����ݒ�
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

            var cancelButton = CreateButton("�L�����Z��", false);
            var okButton = CreateButton("�O���[�v�ǉ�", true);

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
                Width = content == "�O���[�v�ǉ�" ? 110 : 90, // �O���[�v�ǉ��{�^���͕����L����
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

    // �V�K�v���W�F�N�g�쐬�p�̐V�����_�C�A���O�N���X
    public class NewProjectDialog : Window
    {
        public string ProjectName { get; private set; } = string.Empty;
        public ProjectNode? SelectedFolder { get; private set; }
        
        private TextBox projectNameTextBox;
        private ComboBox folderComboBox;
        private readonly List<ProjectNode> availableFolders;

        public NewProjectDialog(List<ProjectNode> folders)
        {
            availableFolders = folders ?? new List<ProjectNode>();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Title = "�V�K�v���W�F�N�g";
            Width = 500;
            Height = 280;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            var grid = new Grid { Margin = new Thickness(24) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // �v���W�F�N�g������
            var nameLabel = new TextBlock
            {
                Text = "�v���W�F�N�g������͂��Ă�������:",
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 8)
            };
            grid.Children.Add(nameLabel);

            projectNameTextBox = new TextBox
            {
                FontSize = 14,
                Padding = new Thickness(10, 6, 10, 6),
                Background = Brushes.White,
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(204, 204, 204)),
                MinHeight = 30,
                Margin = new Thickness(0, 0, 0, 16)
            };
            Grid.SetRow(projectNameTextBox, 1);
            grid.Children.Add(projectNameTextBox);

            // �t�H���_�I��
            var folderLabel = new TextBlock
            {
                Text = "�ǉ�����t�H���_��I�����Ă�������:",
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 8)
            };
            Grid.SetRow(folderLabel, 2);
            grid.Children.Add(folderLabel);

            folderComboBox = new ComboBox
            {
                FontSize = 14,
                Padding = new Thickness(8, 6, 8, 6),
                Background = Brushes.White,
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(204, 204, 204)),
                MinHeight = 30,
                Margin = new Thickness(0, 0, 0, 24),
                DisplayMemberPath = "DisplayName"
            };
            Grid.SetRow(folderComboBox, 3);
            grid.Children.Add(folderComboBox);

            PopulateFolders();

            // �{�^���p�l��
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 0, 0)
            };

            var createButton = CreateButton("�v���W�F�N�g�쐬", true);
            var cancelButton = CreateButton("�L�����Z��", false);

            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(createButton);

            Grid.SetRow(buttonPanel, 4);
            grid.Children.Add(buttonPanel);

            Content = grid;

            projectNameTextBox.Focus();
            projectNameTextBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    if (ValidateAndAccept())
                    {
                        DialogResult = true;
                    }
                }
                else if (e.Key == Key.Escape)
                {
                    DialogResult = false;
                }
            };
        }

        private void PopulateFolders()
        {
            folderComboBox.Items.Clear();

            // ���[�g�I�v�V������ǉ�
            var rootOption = new { DisplayName = "[���[�g] - �g�b�v���x���ɍ쐬", Node = (ProjectNode?)null };
            folderComboBox.Items.Add(rootOption);

            // ���p�\�ȃt�H���_���K�w�t���Œǉ�
            var allFolders = GetFlattenedFolders(availableFolders);
            foreach (var folderInfo in allFolders)
            {
                var indent = new string(' ', folderInfo.Level * 4); // ���p�X�y�[�X�ŃC���f���g�i���x���~4�����j
                var displayItem = new { DisplayName = $"{indent}{folderInfo.Node.Name}", Node = folderInfo.Node };
                folderComboBox.Items.Add(displayItem);
            }

            folderComboBox.SelectedIndex = 0; // �f�t�H���g�Ń��[�g��I��
        }

        private List<(ProjectNode Node, int Level)> GetFlattenedFolders(List<ProjectNode> folders)
        {
            var result = new List<(ProjectNode Node, int Level)>();
            var folderList = folders.Where(f => f.IsFolder).ToList();
            var rootFolders = folderList.Where(f => string.IsNullOrEmpty(f.ParentId)).OrderBy(f => f.OrderIndex);
            
            foreach (var folder in rootFolders)
            {
                AddFolderRecursive(result, folder, folderList, 0);
            }
            
            return result;
        }

        private void AddFolderRecursive(List<(ProjectNode Node, int Level)> result, ProjectNode folder, List<ProjectNode> allFolders, int level)
        {
            result.Add((folder, level));
            
            var children = allFolders.Where(f => f.ParentId == folder.Id).OrderBy(f => f.OrderIndex);
            foreach (var child in children)
            {
                AddFolderRecursive(result, child, allFolders, level + 1);
            }
        }

        private Button CreateButton(string content, bool isDefault)
        {
            var button = new Button
            {
                Content = content,
                Width = 120,
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
                    if (ValidateAndAccept())
                    {
                        DialogResult = true;
                    }
                }
                else
                {
                    DialogResult = false;
                }
            };

            return button;
        }

        private bool ValidateAndAccept()
        {
            if (string.IsNullOrWhiteSpace(projectNameTextBox.Text))
            {
                MessageBox.Show("�v���W�F�N�g������͂��Ă��������B", "���̓G���[", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                projectNameTextBox.Focus();
                return false;
            }

            if (folderComboBox.SelectedItem == null)
            {
                MessageBox.Show("�t�H���_��I�����Ă��������B", "�I���G���[", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                folderComboBox.Focus();
                return false;
            }

            ProjectName = projectNameTextBox.Text.Trim();
            
            var selectedItem = folderComboBox.SelectedItem;
            var nodeProperty = selectedItem?.GetType().GetProperty("Node");
            SelectedFolder = nodeProperty?.GetValue(selectedItem) as ProjectNode;

            return true;
        }
    }
}