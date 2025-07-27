using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ModernLauncher.Models;

namespace ModernLauncher.Views
{
    /// <summary>
    /// ColorSettingsDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class ColorSettingsDialog : Window, INotifyPropertyChanged
    {
        private readonly ObservableCollection<ColorSettingItem> _colorSettings;
        private readonly List<PresetColor> _presetColors;

        public ColorSettingsDialog()
        {
            InitializeComponent();
            
            _presetColors = CreatePresetColors();
            _colorSettings = new ObservableCollection<ColorSettingItem>();
            
            LoadColorSettings();
            DataContext = this;
        }

        public ObservableCollection<ColorSettingItem> ColorSettings => _colorSettings;

        private void LoadColorSettings()
        {
            var categoryColors = CategoryColorSettings.Instance.GetAllCategoryColors();
            var categories = categoryColors.Keys.OrderBy(k => k).ToList();

            foreach (var category in categories)
            {
                var colorItem = new ColorSettingItem
                {
                    Category = category,
                    ColorCode = categoryColors[category],
                    PresetColors = _presetColors
                };
                _colorSettings.Add(colorItem);
            }

            ColorSettingsItemsControl.ItemsSource = _colorSettings;
        }

        private List<PresetColor> CreatePresetColors()
        {
            return new List<PresetColor>
            {
                new PresetColor { Name = "赤", Value = "#FF0000" },
                new PresetColor { Name = "緑", Value = "#008000" },
                new PresetColor { Name = "青", Value = "#0000FF" },
                new PresetColor { Name = "黄", Value = "#FFFF00" },
                new PresetColor { Name = "オレンジ", Value = "#FFA500" },
                new PresetColor { Name = "紫", Value = "#800080" },
                new PresetColor { Name = "ピンク", Value = "#FFC0CB" },
                new PresetColor { Name = "茶", Value = "#A52A2A" },
                new PresetColor { Name = "グレー", Value = "#808080" },
                new PresetColor { Name = "黒", Value = "#000000" },
                new PresetColor { Name = "Excel緑", Value = "#217346" },
                new PresetColor { Name = "Word青", Value = "#2B579A" },
                new PresetColor { Name = "PowerPoint橙", Value = "#D24726" },
                new PresetColor { Name = "GitHub黒", Value = "#24292e" },
                new PresetColor { Name = "GitLab橙", Value = "#FC6D26" },
                new PresetColor { Name = "Google青", Value = "#4285F4" },
                new PresetColor { Name = "Teams紫", Value = "#6264A7" },
                new PresetColor { Name = "SharePoint青", Value = "#0078D4" }
            };
        }

        private void PresetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is PresetColor selectedColor)
            {
                // Find the corresponding color setting item
                var parentBorder = FindParent<Border>(comboBox);
                if (parentBorder?.DataContext is ColorSettingItem colorItem)
                {
                    colorItem.ColorCode = selectedColor.Value;
                }
            }
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
                return null;

            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("すべての色設定をデフォルトに戻しますか？", "確認",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                CategoryColorSettings.Instance.ResetToDefaults();
                
                // Reload the color settings
                _colorSettings.Clear();
                LoadColorSettings();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Save all color settings
            foreach (var colorItem in _colorSettings)
            {
                CategoryColorSettings.Instance.SetColorForCategory(colorItem.Category, colorItem.ColorCode);
            }

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ColorSettingItem : INotifyPropertyChanged
    {
        private string _category;
        private string _colorCode;
        private Brush _colorBrush;

        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged();
            }
        }

        public string ColorCode
        {
            get => _colorCode;
            set
            {
                _colorCode = value;
                UpdateColorBrush();
                OnPropertyChanged();
            }
        }

        public Brush ColorBrush
        {
            get => _colorBrush;
            private set
            {
                _colorBrush = value;
                OnPropertyChanged();
            }
        }

        public List<PresetColor> PresetColors { get; set; }

        private void UpdateColorBrush()
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(_colorCode);
                ColorBrush = new SolidColorBrush(color);
            }
            catch
            {
                ColorBrush = new SolidColorBrush(Colors.LightGray);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PresetColor
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}