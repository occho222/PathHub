using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using ModernLauncher.Interfaces;
using ModernLauncher.Services;

namespace ModernLauncher.Models
{
    public class CategoryColorSettings : INotifyPropertyChanged
    {
        private static CategoryColorSettings _instance = new CategoryColorSettings();
        public static CategoryColorSettings Instance => _instance;

        private Dictionary<string, string> _categoryColors = new Dictionary<string, string>();
        private Dictionary<string, string> _defaultColors = new Dictionary<string, string>
        {
            ["フォルダ"] = "#FFD700",           // 金色
            ["アプリケーション"] = "#4169E1",     // ロイヤルブルー
            ["ドキュメント"] = "#32CD32",        // ライムグリーン
            ["画像"] = "#FF6347",              // トマト色
            ["音楽"] = "#DA70D6",              // オーキッド
            ["動画"] = "#FF4500",              // オレンジレッド
            ["アーカイブ"] = "#8B4513",         // サドルブラウン
            ["Webサイト"] = "#1E90FF",          // ドジャーブルー
            ["GitHubURL"] = "#24292e",         // GitHub のブランドカラー
            ["GitLabURL"] = "#FC6D26",         // GitLab のブランドカラー
            ["Googleドライブ"] = "#4285F4",     // Google ドライブのブルー
            ["Excel"] = "#217346",             // Excel のグリーン
            ["Word"] = "#2B579A",              // Word のブルー
            ["PowerPoint"] = "#D24726",        // PowerPoint のオレンジ
            ["PDF"] = "#FF0000",               // 赤色
            ["ショートカット"] = "#808080",      // グレー
            ["ファイル"] = "#696969",           // 暗いグレー
            ["その他"] = "#A0A0A0"             // ライトグレー
        };

        private readonly IProjectService _projectService;

        public CategoryColorSettings()
        {
            _projectService = ServiceLocator.Instance.GetService<IProjectService>();
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                var savedColors = _projectService.LoadColorSettings();
                if (savedColors != null)
                {
                    _categoryColors = savedColors;
                }
                else
                {
                    // デフォルト色で初期化
                    _categoryColors = new Dictionary<string, string>(_defaultColors);
                }
            }
            catch
            {
                // エラーの場合はデフォルト色で初期化
                _categoryColors = new Dictionary<string, string>(_defaultColors);
            }
        }

        private void SaveSettings()
        {
            try
            {
                _projectService.SaveColorSettings(_categoryColors);
            }
            catch
            {
                // 保存エラーは無視（次回起動時にデフォルトに戻る）
            }
        }

        public string GetColorForCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
                return _defaultColors["その他"];

            return _categoryColors.TryGetValue(category, out var color) 
                ? color 
                : _defaultColors.TryGetValue(category, out var defaultColor) 
                    ? defaultColor 
                    : _defaultColors["その他"];
        }

        public void SetColorForCategory(string category, string color)
        {
            if (string.IsNullOrEmpty(category))
                return;

            _categoryColors[category] = color;
            SaveSettings();
            OnPropertyChanged();
        }

        public SolidColorBrush GetBrushForCategory(string category)
        {
            var colorHex = GetColorForCategory(category);
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(colorHex);
                return new SolidColorBrush(color);
            }
            catch
            {
                return new SolidColorBrush(Colors.LightGray);
            }
        }

        public Dictionary<string, string> GetAllCategoryColors()
        {
            return new Dictionary<string, string>(_categoryColors);
        }

        public void ResetToDefaults()
        {
            _categoryColors = new Dictionary<string, string>(_defaultColors);
            SaveSettings();
            OnPropertyChanged();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}