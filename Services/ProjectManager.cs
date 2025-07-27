using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ModernLauncher.Interfaces;
using ModernLauncher.Models;
using ModernLauncher.Views;

namespace ModernLauncher.Services
{
    public class ProjectManager
    {
        private readonly IProjectService _projectService;
        private ObservableCollection<Project> _projects = new ObservableCollection<Project>();
        private ObservableCollection<ProjectNode> _projectNodes = new ObservableCollection<ProjectNode>();

        public ObservableCollection<Project> Projects => _projects;
        public ObservableCollection<ProjectNode> ProjectNodes => _projectNodes;

        public event EventHandler<ProjectNode?>? ProjectSelectionChanged;

        public ProjectManager(IProjectService projectService)
        {
            _projectService = projectService;
        }

        public void LoadProjects()
        {
            try
            {
                var projectList = _projectService.LoadProjectList();
                
                if (projectList.Any())
                {
                    foreach (var info in projectList.OrderBy(p => p.OrderIndex))
                    {
                        var project = _projectService.LoadProject(info.Id);
                        if (project != null)
                        {
                            project.OrderIndex = info.OrderIndex;
                            project.ParentId = info.ParentId;
                            project.IsFolder = info.IsFolder;
                            UpdateProjectCompatibility(project);
                            _projects.Add(project);
                        }
                    }
                }

                if (_projects.Count == 0)
                {
                    CreateDefaultProject();
                }

                BuildProjectHierarchy();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"�v���W�F�N�g�̓ǂݍ��݂Ɏ��s���܂���: {ex.Message}", "�G���[",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                CreateDefaultProject();
                BuildProjectHierarchy();
            }
        }

        private void CreateDefaultProject()
        {
            var defaultProject = new Project
            {
                Name = "�f�t�H���g",
                Id = Guid.NewGuid().ToString(),
                OrderIndex = 0
            };
            defaultProject.Groups.Add(new ItemGroup { Name = "���ׂ�", Id = "all", OrderIndex = 0 });
            defaultProject.Groups.Add(new ItemGroup { Name = "�悭�g��", Id = Guid.NewGuid().ToString(), OrderIndex = 1 });
            
            _projects.Add(defaultProject);
            
            try
            {
                var projectInfoList = _projects.Select(p => new ProjectInfo
                {
                    Id = p.Id,
                    Name = p.Name,
                    OrderIndex = p.OrderIndex,
                    ParentId = p.ParentId,
                    IsFolder = p.IsFolder
                }).ToList();
                
                _projectService.SaveProjectList(projectInfoList);
                _projectService.SaveProject(defaultProject);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"�f�t�H���g�v���W�F�N�g�ۑ��G���[: {ex.Message}");
            }
        }

        private void UpdateProjectCompatibility(Project project)
        {
            // �O���[�v��OrderIndex������
            if (project.Groups != null)
            {
                for (int i = 0; i < project.Groups.Count; i++)
                {
                    if (project.Groups[i].OrderIndex == 0 && project.Groups[i].Id != "all")
                    {
                        project.Groups[i].OrderIndex = i;
                    }
                }
            }

            foreach (var item in project.Items)
            {
                // ���`������̕ϊ�
                if (item.GroupIds == null || item.GroupIds.Count == 0)
                {
                    item.GroupIds = new List<string>();
                    if (!string.IsNullOrEmpty(item.GroupId))
                    {
                        item.GroupIds.Add(item.GroupId);
                    }
                }

                // ItemType��Icon�̐ݒ�
                if (string.IsNullOrEmpty(item.ItemType) || string.IsNullOrEmpty(item.Icon))
                {
                    item.RefreshIconAndType();
                }

                // OrderIndex�̏�����
                if (item.OrderIndex == 0)
                {
                    item.OrderIndex = project.Items.IndexOf(item);
                }

                // Category�̏�����
                if (string.IsNullOrEmpty(item.Category))
                {
                    item.Category = "���̑�";
                }

                // Description�̏�����
                if (item.Description == null)
                {
                    item.Description = string.Empty;
                }
            }

            // �A�C�e����OrderIndex�Ń\�[�g
            var sortedItems = project.Items.OrderBy(i => i.OrderIndex).ToList();
            project.Items.Clear();
            foreach (var item in sortedItems)
            {
                project.Items.Add(item);
            }
        }

        public void BuildProjectHierarchy()
        {
            _projectNodes.Clear();
            var nodeMap = new Dictionary<string, ProjectNode>();

            // ���ׂẴv���W�F�N�g�ƃt�H���_�[����m�[�h���쐬
            foreach (var project in _projects.OrderBy(p => p.OrderIndex))
            {
                var node = new ProjectNode
                {
                    Id = project.Id,
                    Name = project.Name,
                    IsFolder = project.IsFolder,
                    OrderIndex = project.OrderIndex,
                    ParentId = project.ParentId,
                    Project = project.IsFolder ? null : project
                };
                nodeMap[project.Id] = node;
            }

            // �e�q�֌W���\�z
            foreach (var node in nodeMap.Values)
            {
                if (!string.IsNullOrEmpty(node.ParentId) && nodeMap.ContainsKey(node.ParentId))
                {
                    nodeMap[node.ParentId].AddChild(node);
                }
                else
                {
                    _projectNodes.Add(node);
                }
            }

            // �\�[�g
            SortProjectNodes(_projectNodes);
            
            // ���ׂẴm�[�h��DisplayName���X�V
            RefreshAllNodeDisplayNames(_projectNodes);
        }

        private void RefreshAllNodeDisplayNames(ObservableCollection<ProjectNode> nodes)
        {
            foreach (var node in nodes)
            {
                node.RefreshDisplayName();
                if (node.Children.Count > 0)
                {
                    RefreshAllNodeDisplayNames(node.Children);
                }
            }
        }

        private void SortProjectNodes(ObservableCollection<ProjectNode> nodes)
        {
            var sortedNodes = nodes.OrderBy(n => n.OrderIndex).ToList();
            nodes.Clear();
            foreach (var node in sortedNodes)
            {
                nodes.Add(node);
                if (node.Children.Count > 0)
                {
                    SortProjectNodes(node.Children);
                }
            }
        }

        public ProjectNode? FindProjectNode(string projectId)
        {
            return FindProjectNodeRecursive(_projectNodes, projectId);
        }

        private ProjectNode? FindProjectNodeRecursive(ObservableCollection<ProjectNode> nodes, string projectId)
        {
            foreach (var node in nodes)
            {
                if (node.Id == projectId)
                    return node;
                
                var found = FindProjectNodeRecursive(node.Children, projectId);
                if (found != null)
                    return found;
            }
            return null;
        }

        public void CreateNewProject(string projectName, ProjectNode? parentFolder)
        {
            try
            {
                var parentId = parentFolder?.Id;
                
                var newProject = new Project
                {
                    Name = projectName,
                    Id = Guid.NewGuid().ToString(),
                    OrderIndex = _projects.Count,
                    ParentId = parentId,
                    IsFolder = false
                };
                newProject.Groups.Add(new ItemGroup { Name = "���ׂ�", Id = "all", OrderIndex = 0 });
                newProject.Groups.Add(new ItemGroup { Name = "�悭�g��", Id = Guid.NewGuid().ToString(), OrderIndex = 1 });
                
                _projects.Add(newProject);
                
                _projectService.SaveProject(newProject);
                
                var projectInfoList = _projects.Select(p => new ProjectInfo
                {
                    Id = p.Id,
                    Name = p.Name,
                    OrderIndex = p.OrderIndex,
                    ParentId = p.ParentId,
                    IsFolder = p.IsFolder
                }).ToList();
                
                _projectService.SaveProjectList(projectInfoList);
                BuildProjectHierarchy();
                
                // �V�����쐬���ꂽ�v���W�F�N�g��I��
                var newNode = FindProjectNode(newProject.Id);
                if (newNode != null)
                {
                    ProjectSelectionChanged?.Invoke(this, newNode);
                    
                    // �v���W�F�N�g���쐬���ꂽ�t�H���_��W�J
                    if (parentFolder != null)
                    {
                        var parentNode = FindProjectNode(parentFolder.Id);
                        if (parentNode != null)
                        {
                            parentNode.IsExpanded = true;
                        }
                    }
                }
                
                var folderName = parentFolder?.Name ?? "Root";
                MessageBox.Show($"Project '{newProject.Name}' created successfully in '{folderName}'.", 
                    "Project Created", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Project save error: {ex.Message}");
                MessageBox.Show($"Failed to save project: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CreateNewFolder(string folderName, string? parentId = null)
        {
            var newFolder = new Project
            {
                Name = folderName,
                Id = Guid.NewGuid().ToString(),
                OrderIndex = _projects.Count,
                ParentId = parentId,
                IsFolder = true
            };
            
            _projects.Add(newFolder);
            
            try
            {
                _projectService.SaveProject(newFolder);
                
                var projectInfoList = _projects.Select(p => new ProjectInfo
                {
                    Id = p.Id,
                    Name = p.Name,
                    OrderIndex = p.OrderIndex,
                    ParentId = p.ParentId,
                    IsFolder = p.IsFolder
                }).ToList();
                
                _projectService.SaveProjectList(projectInfoList);
                BuildProjectHierarchy();
                
                // �V�����쐬���ꂽ�t�H���_�[��I��
                var newNode = FindProjectNode(newFolder.Id);
                if (newNode != null)
                {
                    ProjectSelectionChanged?.Invoke(this, newNode);
                    newNode.IsExpanded = true;
                }
                
                MessageBox.Show($"Folder '{newFolder.Name}' created successfully.", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save folder: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void DeleteProjectOrFolder(ProjectNode nodeToDelete)
        {
            var itemType = nodeToDelete.IsFolder ? "�t�H���_�[" : "�v���W�F�N�g";
            var result = MessageBox.Show($"{itemType}�u{nodeToDelete.Name}�v���폜���܂����H\n\n���q�v�f���ꏏ�ɍ폜����܂��B", 
                "�m�F", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // �폜�Ώۂ�ID���X�g�����W�i�q�v�f���܂ށj
                    var idsToDelete = new List<string>();
                    CollectNodeIds(nodeToDelete, idsToDelete);

                    // �v���W�F�N�g�R���N�V��������폜
                    var projectsToRemove = _projects.Where(p => idsToDelete.Contains(p.Id)).ToList();
                    foreach (var project in projectsToRemove)
                    {
                        _projects.Remove(project);
                        
                        // �t�@�C��������폜
                        try
                        {
                            _projectService.DeleteProject(project.Id);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"�v���W�F�N�g�t�@�C���폜�G���[: {ex.Message}");
                        }
                    }

                    // �v���W�F�N�g��񃊃X�g���X�V
                    var projectInfoList = _projects.Select(p => new ProjectInfo
                    {
                        Id = p.Id,
                        Name = p.Name,
                        OrderIndex = p.OrderIndex,
                        ParentId = p.ParentId,
                        IsFolder = p.IsFolder
                    }).ToList();
                    
                    _projectService.SaveProjectList(projectInfoList);

                    // �K�w�\�����č\�z
                    BuildProjectHierarchy();

                    // �c��̃v���W�F�N�g������ꍇ�͍ŏ��̃v���W�F�N�g��I��
                    var firstProject = _projectNodes.FirstOrDefault(n => !n.IsFolder);
                    if (firstProject == null)
                    {
                        firstProject = FindFirstProjectRecursive(_projectNodes);
                    }
                    
                    if (firstProject != null)
                    {
                        ProjectSelectionChanged?.Invoke(this, firstProject);
                    }
                    else
                    {
                        // �v���W�F�N�g���S���Ȃ��ꍇ�̓f�t�H���g�v���W�F�N�g���쐬
                        CreateDefaultProject();
                        BuildProjectHierarchy();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"�폜�Ɏ��s���܂���: {ex.Message}", "�G���[",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CollectNodeIds(ProjectNode node, List<string> ids)
        {
            ids.Add(node.Id);
            foreach (var child in node.Children)
            {
                CollectNodeIds(child, ids);
            }
        }

        private ProjectNode? FindFirstProjectRecursive(ObservableCollection<ProjectNode> nodes)
        {
            foreach (var node in nodes)
            {
                if (!node.IsFolder)
                {
                    return node;
                }
                
                var found = FindFirstProjectRecursive(node.Children);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        public void SaveAllProjects()
        {
            try
            {
                foreach (var project in _projects)
                {
                    _projectService.SaveProject(project);
                }
                
                var projectInfoList = _projects.Select(p => new ProjectInfo
                {
                    Id = p.Id,
                    Name = p.Name,
                    OrderIndex = p.OrderIndex,
                    ParentId = p.ParentId,
                    IsFolder = p.IsFolder
                }).ToList();
                
                _projectService.SaveProjectList(projectInfoList);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"�ۑ��Ɏ��s���܂���: {ex.Message}", "�G���[",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void MoveProjectToPosition(string projectId, int newIndex)
        {
            var project = _projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null) return;

            var siblings = GetSiblingProjects(project);
            var currentIndex = siblings.IndexOf(project);
            
            if (currentIndex == newIndex) return;

            // �V�����ʒu�Ƀv���W�F�N�g���ړ�
            siblings.RemoveAt(currentIndex);
            siblings.Insert(newIndex, project);

            // OrderIndex���X�V
            for (int i = 0; i < siblings.Count; i++)
            {
                siblings[i].OrderIndex = i;
            }

            SaveAllProjects();
            BuildProjectHierarchy();
        }

        private List<Project> GetSiblingProjects(Project project)
        {
            return _projects.Where(p => p.ParentId == project.ParentId)
                          .OrderBy(p => p.OrderIndex)
                          .ToList();
        }

        public List<ProjectNode> GetAvailableFolders(ProjectNode? excludeNode = null)
        {
            var allFolders = new List<ProjectNode>();
            CollectFoldersRecursive(_projectNodes, allFolders);
            
            if (excludeNode != null)
            {
                // �ړ��Ώێ��g�Ǝq�t�H���_�[�����O
                var excludeIds = new HashSet<string>();
                CollectNodeIdsRecursive(excludeNode, excludeIds);
                allFolders = allFolders.Where(f => !excludeIds.Contains(f.Id)).ToList();
            }
            
            return allFolders;
        }

        private void CollectFoldersRecursive(ObservableCollection<ProjectNode> nodes, List<ProjectNode> folders)
        {
            foreach (var node in nodes)
            {
                if (node.IsFolder)
                {
                    folders.Add(node);
                }
                CollectFoldersRecursive(node.Children, folders);
            }
        }

        private void CollectNodeIdsRecursive(ProjectNode node, HashSet<string> ids)
        {
            ids.Add(node.Id);
            foreach (var child in node.Children)
            {
                CollectNodeIdsRecursive(child, ids);
            }
        }
    }
}