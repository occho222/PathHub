using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ModernLauncher.Models;

namespace ModernLauncher.Services
{
    public class CategoryService
    {
        private static readonly string CategoriesFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NicoPath", "categories.json");

        private List<string> _categories = new List<string>();
        private List<string> _defaultCategories = new List<string>
        {
            "アプリケーション",
            "ドキュメント",
            "画像",
            "音楽",
            "動画",
            "アーカイブ",
            "Webサイト",
            "GitHubURL",
            "GitLabURL",
            "RedmineURL",
            "Googleドライブ",
            "MicrosoftTeams",
            "SharePoint",
            "OneDrive",
            "OneNote",
            "Excel",
            "Word",
            "PowerPoint",
            "PDF",
            "ショートカット",
            "ファイル",
            "フォルダ",
            "プログラム",
            "コマンド",
            "その他"
        };

        public CategoryService()
        {
            LoadCategories();
        }

        public List<string> GetCategories()
        {
            return _categories.ToList();
        }

        public void AddCategory(string categoryName)
        {
            if (!string.IsNullOrWhiteSpace(categoryName) && !_categories.Contains(categoryName))
            {
                _categories.Add(categoryName);
                SaveCategories();
            }
        }

        public void RemoveCategory(string categoryName)
        {
            if (_categories.Contains(categoryName))
            {
                _categories.Remove(categoryName);
                SaveCategories();
            }
        }

        public void UpdateCategory(string oldName, string newName)
        {
            if (!string.IsNullOrWhiteSpace(newName) && _categories.Contains(oldName))
            {
                var index = _categories.IndexOf(oldName);
                _categories[index] = newName;
                SaveCategories();
            }
        }

        public void ResetToDefaults()
        {
            _categories = _defaultCategories.ToList();
            SaveCategories();
        }

        private void LoadCategories()
        {
            try
            {
                if (File.Exists(CategoriesFilePath))
                {
                    var json = File.ReadAllText(CategoriesFilePath);
                    _categories = JsonSerializer.Deserialize<List<string>>(json) ?? _defaultCategories.ToList();
                }
                else
                {
                    _categories = _defaultCategories.ToList();
                }
            }
            catch
            {
                _categories = _defaultCategories.ToList();
            }
        }

        private void SaveCategories()
        {
            try
            {
                var directory = Path.GetDirectoryName(CategoriesFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(_categories, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(CategoriesFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save categories: {ex.Message}");
            }
        }
    }
}