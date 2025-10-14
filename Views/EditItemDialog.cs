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
using ModernLauncher.Services;

namespace ModernLauncher.Views
{
    /// <summary>
    /// EditItemDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class EditItemDialog : Window
    {
        public LauncherItem? Result { get; private set; }
        public Project? SelectedProject { get; private set; }

        private readonly List<ItemGroup> availableGroups;
        private readonly LauncherItem originalItem;
        private readonly CategoryService _categoryService;
        private readonly List<Project> availableProjects;
        private readonly Project currentProject;

        public EditItemDialog(LauncherItem item, List<ItemGroup> groups, List<Project> projects, Project currentProject)
        {
            InitializeComponent();

            originalItem = item;
            availableGroups = groups;
            availableProjects = projects;
            this.currentProject = currentProject;
            _categoryService = new CategoryService();

            InitializeControls();
            LoadItemData();

            // Set focus to name textbox
            NameTextBox.Focus();
        }

        private void InitializeControls()
        {
            // Initialize project ComboBox
            foreach (var project in availableProjects)
            {
                ProjectComboBox.Items.Add(project);
            }

            // Initialize category ComboBox with all available categories
            LoadCategories();

            // Initialize groups ListBox
            foreach (var group in availableGroups.Where(g => g.Id != "all"))
            {
                var checkBox = new CheckBox
                {
                    Content = group.Name,
                    Tag = group,
                    FontSize = 13,
                    Margin = new Thickness(4, 2, 4, 2)
                };
                GroupsListBox.Items.Add(checkBox);
            }
        }

        private void LoadCategories()
        {
            var categories = _categoryService.GetCategories();
            CategoryComboBox.Items.Clear();
            foreach (var category in categories)
            {
                CategoryComboBox.Items.Add(category);
            }
        }

        private void LoadItemData()
        {
            NameTextBox.Text = originalItem.Name;
            PathTextBox.Text = originalItem.Path;
            DescriptionTextBox.Text = originalItem.Description ?? string.Empty;

            // Set project
            ProjectComboBox.SelectedItem = currentProject;

            // Set category (select from existing items or set text)
            bool categoryFound = false;
            foreach (var item in CategoryComboBox.Items)
            {
                if (item.ToString() == originalItem.Category)
                {
                    CategoryComboBox.SelectedItem = item;
                    categoryFound = true;
                    break;
                }
            }

            if (!categoryFound)
            {
                CategoryComboBox.Text = originalItem.Category ?? "その他";
            }
            
            // VSCodeCheckBoxが存在する場合のみ設定
            if (VSCodeCheckBox != null)
            {
                VSCodeCheckBox.IsChecked = originalItem.OpenWithVSCode;
            }

            // OfficeCheckBoxが存在する場合のみ設定
            if (OfficeCheckBox != null)
            {
                OfficeCheckBox.IsChecked = originalItem.OpenWithOffice;
            }

            // Set selected groups
            if (originalItem.GroupIds != null)
            {
                foreach (CheckBox checkBox in GroupsListBox.Items)
                {
                    if (checkBox.Tag is ItemGroup group && originalItem.GroupIds.Contains(group.Id))
                    {
                        checkBox.IsChecked = true;
                    }
                }
            }
        }

        private void ManageCategoriesButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CategoryManagementDialog();
            if (dialog.ShowDialog() == true)
            {
                var currentCategory = CategoryComboBox.Text;
                LoadCategories();
                
                // Try to restore the previously selected category
                if (!string.IsNullOrEmpty(currentCategory))
                {
                    bool found = false;
                    foreach (var item in CategoryComboBox.Items)
                    {
                        if (item.ToString() == currentCategory)
                        {
                            CategoryComboBox.SelectedItem = item;
                            found = true;
                            break;
                        }
                    }
                    
                    if (!found)
                    {
                        CategoryComboBox.Text = currentCategory;
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
                FileName = PathTextBox.Text
            };

            if (openFileDialog.ShowDialog() == true)
            {
                PathTextBox.Text = openFileDialog.FileName;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                ShowValidationError("名前を入力してください。", NameTextBox);
                return;
            }

            if (string.IsNullOrWhiteSpace(PathTextBox.Text))
            {
                ShowValidationError("パスを入力してください。", PathTextBox);
                return;
            }

            var selectedGroups = new List<string>();
            foreach (CheckBox checkBox in GroupsListBox.Items)
            {
                if (checkBox.IsChecked == true && checkBox.Tag is ItemGroup group)
                {
                    selectedGroups.Add(group.Id);
                }
            }

            Result = new LauncherItem
            {
                Id = originalItem.Id,
                Name = NameTextBox.Text.Trim(),
                Path = PathTextBox.Text.Trim(),
                Description = DescriptionTextBox.Text.Trim(),
                Category = CategoryComboBox.Text?.Trim() ?? "その他",
                GroupIds = selectedGroups,
                OrderIndex = originalItem.OrderIndex,
                LastAccessed = originalItem.LastAccessed,
                OpenWithVSCode = VSCodeCheckBox?.IsChecked == true,
                OpenWithOffice = OfficeCheckBox?.IsChecked == true
            };

            // Set selected project
            SelectedProject = ProjectComboBox.SelectedItem as Project;

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ShowValidationError(string message, Control controlToFocus)
        {
            MessageBox.Show(message, "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
            controlToFocus.Focus();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !e.Handled)
            {
                OkButton_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (e.Key == Key.Escape && !e.Handled)
            {
                CancelButton_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
            
            base.OnKeyDown(e);
        }
    }
}