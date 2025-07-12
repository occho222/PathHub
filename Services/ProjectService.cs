using ModernLauncher.Interfaces;
using ModernLauncher.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModernLauncher.Services
{
    public class ProjectService : IProjectService
    {
        private readonly string projectsFolder = "Projects";
        private readonly string projectListFile = "projects.json";
        private readonly string colorSettingsFile = "colorSettings.json";

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
                File.WriteAllText(projectFile, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"�v���W�F�N�g�̕ۑ��Ɏ��s���܂���: {ex.Message}", ex);
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
                File.WriteAllText(projectListFile, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"�v���W�F�N�g�ꗗ�̕ۑ��Ɏ��s���܂���: {ex.Message}", ex);
            }
        }

        public void SaveColorSettings(Dictionary<string, string> colorSettings)
        {
            try
            {
                string json = JsonConvert.SerializeObject(colorSettings, Formatting.Indented);
                File.WriteAllText(colorSettingsFile, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"�F�ݒ�̕ۑ��Ɏ��s���܂���: {ex.Message}", ex);
            }
        }

        public Dictionary<string, string>? LoadColorSettings()
        {
            try
            {
                if (File.Exists(colorSettingsFile))
                {
                    string json = File.ReadAllText(colorSettingsFile);
                    return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"�F�ݒ�̓ǂݍ��݂Ɏ��s���܂���: {ex.Message}", ex);
            }
        }

        public Project? LoadProject(string id)
        {
            try
            {
                var projectFile = Path.Combine(projectsFolder, $"{id}.json");
                if (File.Exists(projectFile))
                {
                    var projectJson = File.ReadAllText(projectFile);
                    var project = JsonConvert.DeserializeObject<Project>(projectJson);
                    return project;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"�v���W�F�N�g�̓ǂݍ��݂Ɏ��s���܂���: {ex.Message}", ex);
            }
        }

        public List<ProjectInfo> LoadProjectList()
        {
            try
            {
                if (File.Exists(projectListFile))
                {
                    string json = File.ReadAllText(projectListFile);
                    var projectList = JsonConvert.DeserializeObject<List<ProjectInfo>>(json);
                    return projectList ?? new List<ProjectInfo>();
                }
                return new List<ProjectInfo>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"�v���W�F�N�g���X�g�̓ǂݍ��݂Ɏ��s���܂���: {ex.Message}", ex);
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
                throw new InvalidOperationException($"�v���W�F�N�g�̍폜�Ɏ��s���܂���: {ex.Message}", ex);
            }
        }

        public void ExportProject(Project project, string filePath)
        {
            try
            {
                string json = JsonConvert.SerializeObject(project, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"�G�N�X�|�[�g�Ɏ��s���܂���: {ex.Message}", ex);
            }
        }

        public Project? ImportProject(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException("�w�肳�ꂽ�t�@�C����������܂���");
                }

                string json = File.ReadAllText(filePath);
                var project = JsonConvert.DeserializeObject<Project>(json);
                
                if (project == null)
                {
                    throw new InvalidOperationException("�����ȃv���W�F�N�g�t�@�C���ł�");
                }

                return project;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"�C���|�[�g�Ɏ��s���܂���: {ex.Message}", ex);
            }
        }
    }
}