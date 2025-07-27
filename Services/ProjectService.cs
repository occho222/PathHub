using ModernLauncher.Interfaces;
using ModernLauncher.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ModernLauncher.Services
{
    public class ProjectService : IProjectService
    {
        private readonly string projectsFolder = "Projects";
        private readonly string projectListFile = "projects.json";
        private readonly string colorSettingsFile = "colorSettings.json";
        private readonly string windowLayoutFile = "windowLayout.json";

        public ProjectService()
        {
            if (!Directory.Exists(projectsFolder))
            {
                Directory.CreateDirectory(projectsFolder);
            }
        }

        public void SaveProject(Project project)
        {
            try
            {
                string projectFile = Path.Combine(projectsFolder, $"{project.Id}.json");
                string json = JsonConvert.SerializeObject(project, Formatting.Indented);
                File.WriteAllText(projectFile, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"プロジェクトの保存に失敗しました: {ex.Message}", ex);
            }
        }

        public void SaveProjectList(IEnumerable<ProjectInfo> projectInfos)
        {
            try
            {
                var projectList = projectInfos.Select(p => new ProjectInfo
                {
                    Id = p.Id,
                    Name = p.Name,
                    OrderIndex = p.OrderIndex,
                    ParentId = p.ParentId,
                    IsFolder = p.IsFolder
                }).ToList();

                string json = JsonConvert.SerializeObject(projectList, Formatting.Indented);
                File.WriteAllText(projectListFile, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"プロジェクト一覧の保存に失敗しました: {ex.Message}", ex);
            }
        }

        public void SaveColorSettings(Dictionary<string, string> colorSettings)
        {
            try
            {
                string json = JsonConvert.SerializeObject(colorSettings, Formatting.Indented);
                File.WriteAllText(colorSettingsFile, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"色設定の保存に失敗しました: {ex.Message}", ex);
            }
        }

        public void SaveWindowLayout(WindowLayoutSettings layoutSettings)
        {
            try
            {
                string json = JsonConvert.SerializeObject(layoutSettings, Formatting.Indented);
                File.WriteAllText(windowLayoutFile, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"ウィンドウレイアウト設定の保存に失敗しました: {ex.Message}", ex);
            }
        }

        public Dictionary<string, string>? LoadColorSettings()
        {
            try
            {
                if (File.Exists(colorSettingsFile))
                {
                    string json = File.ReadAllText(colorSettingsFile, Encoding.UTF8);
                    return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"色設定の読み込みに失敗しました: {ex.Message}", ex);
            }
        }

        public WindowLayoutSettings? LoadWindowLayout()
        {
            try
            {
                if (File.Exists(windowLayoutFile))
                {
                    string json = File.ReadAllText(windowLayoutFile, Encoding.UTF8);
                    return JsonConvert.DeserializeObject<WindowLayoutSettings>(json);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"ウィンドウレイアウト設定の読み込みに失敗しました: {ex.Message}", ex);
            }
        }

        public Project? LoadProject(string id)
        {
            try
            {
                var projectFile = Path.Combine(projectsFolder, $"{id}.json");
                if (File.Exists(projectFile))
                {
                    var projectJson = File.ReadAllText(projectFile, Encoding.UTF8);
                    var project = JsonConvert.DeserializeObject<Project>(projectJson);
                    return project;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"プロジェクトの読み込みに失敗しました: {ex.Message}", ex);
            }
        }

        public List<ProjectInfo> LoadProjectList()
        {
            try
            {
                if (File.Exists(projectListFile))
                {
                    string json = File.ReadAllText(projectListFile, Encoding.UTF8);
                    var projectList = JsonConvert.DeserializeObject<List<ProjectInfo>>(json);
                    return projectList ?? new List<ProjectInfo>();
                }
                return new List<ProjectInfo>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"プロジェクトリストの読み込みに失敗しました: {ex.Message}", ex);
            }
        }

        public void DeleteProject(string id)
        {
            try
            {
                string projectFile = Path.Combine(projectsFolder, $"{id}.json");
                if (File.Exists(projectFile))
                {
                    File.Delete(projectFile);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"プロジェクトの削除に失敗しました: {ex.Message}", ex);
            }
        }

        public void ExportProject(Project project, string filePath)
        {
            try
            {
                string json = JsonConvert.SerializeObject(project, Formatting.Indented);
                File.WriteAllText(filePath, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"エクスポートに失敗しました: {ex.Message}", ex);
            }
        }

        public Project? ImportProject(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException("指定されたファイルが見つかりません");
                }

                string json = File.ReadAllText(filePath, Encoding.UTF8);
                var project = JsonConvert.DeserializeObject<Project>(json);
                
                if (project == null)
                {
                    throw new InvalidOperationException("無効なプロジェクトファイルです");
                }

                return project;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"インポートに失敗しました: {ex.Message}", ex);
            }
        }
    }
}