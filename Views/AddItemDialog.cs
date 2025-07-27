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
    /// AddItemDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class AddItemDialog : Window
    {
        public LauncherItem? Result { get; private set; }
        
        private readonly List<ItemGroup> availableGroups;
        private readonly CategoryService _categoryService;

        public AddItemDialog(List<ItemGroup> groups)
        {
            InitializeComponent();
            
            availableGroups = groups;
            _categoryService = new CategoryService();
            InitializeControls();
            
            // Set focus to name textbox
            NameTextBox.Focus();
        }

        private void InitializeControls()
        {
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

            // Add path changed event
            PathTextBox.TextChanged += PathTextBox_TextChanged;
        }

        private void LoadCategories()
        {
            var categories = _categoryService.GetCategories();
            CategoryComboBox.Items.Clear();
            foreach (var category in categories)
            {
                CategoryComboBox.Items.Add(category);
            }
            
            if (CategoryComboBox.Items.Count > 0)
            {
                CategoryComboBox.SelectedIndex = 0;
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

        private void PathTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Auto-detect category when path changes
            if (!string.IsNullOrWhiteSpace(PathTextBox.Text))
            {
                try
                {
                    var launcherService = new ModernLauncher.Services.LauncherService();
                    var detectedCategory = launcherService.DetectCategory(PathTextBox.Text);
                    
                    // デバッグ用：結果をウィンドウタイトルに表示
                    this.Title = $"Add Item - Detected: {detectedCategory}";
                    
                    // Set category if it matches one of the ComboBox items
                    foreach (var item in CategoryComboBox.Items)
                    {
                        if (item.ToString() == detectedCategory)
                        {
                            CategoryComboBox.SelectedItem = item;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // デバッグ用：エラーをウィンドウタイトルに表示
                    this.Title = $"Add Item - Error: {ex.Message}";
                }
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "実行ファイルを選択",
                Filter = "実行ファイル (*.exe)|*.exe|すべてのファイル (*.*)|*.*",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                PathTextBox.Text = openFileDialog.FileName;
                
                // Auto-populate name if empty
                if (string.IsNullOrWhiteSpace(NameTextBox.Text))
                {
                    NameTextBox.Text = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                }
                
                // Auto-detect category
                try
                {
                    var launcherService = new ModernLauncher.Services.LauncherService();
                    var detectedCategory = launcherService.DetectCategory(openFileDialog.FileName);
                    
                    // Set category if it matches one of the ComboBox items
                    foreach (var item in CategoryComboBox.Items)
                    {
                        if (item.ToString() == detectedCategory)
                        {
                            CategoryComboBox.SelectedItem = item;
                            break;
                        }
                    }
                }
                catch
                {
                    // Ignore detection errors
                }
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
                Id = Guid.NewGuid().ToString(),
                Name = NameTextBox.Text.Trim(),
                Path = PathTextBox.Text.Trim(),
                Description = DescriptionTextBox.Text.Trim(),
                Category = CategoryComboBox.Text?.Trim() ?? "その他",
                GroupIds = selectedGroups,
                OrderIndex = 0,
                LastAccessed = DateTime.MinValue,  // 新しいアイテムなので未アクセス状態
                OpenWithVSCode = VSCodeCheckBox.IsChecked == true,
                OpenWithOffice = OfficeCheckBox?.IsChecked == true
            };

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        public void SetInitialValues(string name, string path, string category, string description, bool openWithVSCode = false, bool openWithOffice = false)
        {
            NameTextBox.Text = name;
            PathTextBox.Text = path;
            DescriptionTextBox.Text = description;
            VSCodeCheckBox.IsChecked = openWithVSCode;
            
            // OfficeCheckBoxが存在する場合のみ設定
            if (OfficeCheckBox != null)
            {
                OfficeCheckBox.IsChecked = openWithOffice;
            }
            
            // パスが設定された後に自動判定を実行
            if (!string.IsNullOrWhiteSpace(path))
            {
                try
                {
                    var launcherService = new ModernLauncher.Services.LauncherService();
                    var detectedCategory = launcherService.DetectCategory(path);
                    
                    // デバッグ用：結果をウィンドウタイトルに表示
                    this.Title = $"Add Item - SetInitialValues Detected: {detectedCategory}";
                    
                    // 自動判定結果を優先する
                    category = detectedCategory;
                }
                catch (Exception ex)
                {
                    // デバッグ用：エラーをウィンドウタイトルに表示
                    this.Title = $"Add Item - SetInitialValues Error: {ex.Message}";
                }
            }
            
            // Set category (select from existing items or set text)
            bool categoryFound = false;
            foreach (var item in CategoryComboBox.Items)
            {
                if (item.ToString() == category)
                {
                    CategoryComboBox.SelectedItem = item;
                    categoryFound = true;
                    break;
                }
            }
            
            if (!categoryFound)
            {
                CategoryComboBox.Text = category;
            }
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