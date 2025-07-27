using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ModernLauncher.Services;

namespace ModernLauncher.Views
{
    /// <summary>
    /// CategoryManagementDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class CategoryManagementDialog : Window
    {
        private CategoryService _categoryService;
        private List<string> _categories;

        public CategoryManagementDialog()
        {
            InitializeComponent();
            _categoryService = new CategoryService();
            LoadCategories();
        }

        private void LoadCategories()
        {
            _categories = _categoryService.GetCategories();
            CategoriesListBox.ItemsSource = _categories;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new TextInputDialog("Add Category", "Enter new category name:", "");
            if (dialog.ShowDialog() == true)
            {
                var newCategory = dialog.InputText?.Trim();
                if (!string.IsNullOrWhiteSpace(newCategory))
                {
                    if (_categories.Contains(newCategory))
                    {
                        MessageBox.Show("This category already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    _categoryService.AddCategory(newCategory);
                    LoadCategories();
                }
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedCategory = CategoriesListBox.SelectedItem as string;
            if (selectedCategory == null)
            {
                MessageBox.Show("Please select a category to edit.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new TextInputDialog("Edit Category", "Edit category name:", selectedCategory);
            if (dialog.ShowDialog() == true)
            {
                var newCategory = dialog.InputText?.Trim();
                if (!string.IsNullOrWhiteSpace(newCategory) && newCategory != selectedCategory)
                {
                    if (_categories.Contains(newCategory))
                    {
                        MessageBox.Show("This category already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    _categoryService.UpdateCategory(selectedCategory, newCategory);
                    LoadCategories();
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedCategory = CategoriesListBox.SelectedItem as string;
            if (selectedCategory == null)
            {
                MessageBox.Show("Please select a category to delete.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Delete category '{selectedCategory}'?", "Confirm", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                _categoryService.RemoveCategory(selectedCategory);
                LoadCategories();
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Reset all categories to default?", "Confirm", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                _categoryService.ResetToDefaults();
                LoadCategories();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !e.Handled)
            {
                SaveButton_Click(this, new RoutedEventArgs());
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