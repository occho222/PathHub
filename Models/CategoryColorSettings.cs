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
            ["�t�H���_"] = "#FFD700",           // ���F
            ["�A�v���P�[�V����"] = "#4169E1",     // ���C�����u���[
            ["�h�L�������g"] = "#32CD32",        // ���C���O���[��
            ["�摜"] = "#FF6347",              // �g�}�g�F
            ["���y"] = "#DA70D6",              // �I�[�L�b�h
            ["����"] = "#FF4500",              // �I�����W���b�h
            ["�A�[�J�C�u"] = "#8B4513",         // �T�h���u���E��
            ["Web�T�C�g"] = "#1E90FF",          // �h�W���[�u���[
            ["GitHubURL"] = "#24292e",         // GitHub �̃u�����h�J���[
            ["GitLabURL"] = "#FC6D26",         // GitLab �̃u�����h�J���[
            ["Google�h���C�u"] = "#4285F4",     // Google �h���C�u�̃u���[
            ["Excel"] = "#217346",             // Excel �̃O���[��
            ["Word"] = "#2B579A",              // Word �̃u���[
            ["PowerPoint"] = "#D24726",        // PowerPoint �̃I�����W
            ["PDF"] = "#FF0000",               // �ԐF
            ["�V���[�g�J�b�g"] = "#808080",      // �O���[
            ["�t�@�C��"] = "#696969",           // �Â��O���[
            ["���̑�"] = "#A0A0A0"             // ���C�g�O���[
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
                    // �f�t�H���g�F�ŏ�����
                    _categoryColors = new Dictionary<string, string>(_defaultColors);
                }
            }
            catch
            {
                // �G���[�̏ꍇ�̓f�t�H���g�F�ŏ�����
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
                // �ۑ��G���[�͖����i����N�����Ƀf�t�H���g�ɖ߂�j
            }
        }

        public string GetColorForCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
                return _defaultColors["���̑�"];

            return _categoryColors.TryGetValue(category, out var color) 
                ? color 
                : _defaultColors.TryGetValue(category, out var defaultColor) 
                    ? defaultColor 
                    : _defaultColors["���̑�"];
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