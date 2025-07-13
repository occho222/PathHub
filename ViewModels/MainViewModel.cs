using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Controls;
using ModernLauncher.Commands;
using ModernLauncher.Interfaces;
using ModernLauncher.Models;
using ModernLauncher.Services;
using ModernLauncher.Views;

namespace ModernLauncher.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IProjectService projectService;
        private readonly ILauncherService launcherService;
        
        private ObservableCollection<Project> projects = new ObservableCollection<Project>();
        private ObservableCollection<ProjectNode> projectNodes = new ObservableCollection<ProjectNode>();
        private Project? currentProject;
        private ProjectNode? selectedProjectNode;
        private ItemGroup? selectedViewGroup;
        private string searchText = string.Empty;
        private LauncherItem? selectedItem;
        private string statusText = string.Empty;
        private readonly string appVersion = "1.4.0";

        // Commands
        public ICommand NewProjectCommand { get; }
        public ICommand NewFolderCommand { get; }
        public ICommand DeleteProjectCommand { get; }
        public ICommand MoveToFolderCommand { get; }
        public ICommand CreateTestFoldersCommand { get; }
        public ICommand AddItemCommand { get; }
        public ICommand AddGroupCommand { get; }
        public ICommand SaveDataCommand { get; }
        public ICommand LaunchItemCommand { get; }
        public ICommand EditItemCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand MoveItemUpCommand { get; }
        public ICommand MoveItemDownCommand { get; }
        public ICommand ExportProjectCommand { get; }
        public ICommand ImportProjectCommand { get; }
        public ICommand ShowHelpCommand { get; }
        public ICommand ShowColorSettingsCommand { get; }
        public ICommand OpenWithVSCodeCommand { get; }
        public ICommand OpenWithOfficeCommand { get; }

        public MainViewModel() : this(
            ServiceLocator.Instance.GetService<IProjectService>(), 
            ServiceLocator.Instance.GetService<ILauncherService>())
        {
        }

        public MainViewModel(IProjectService projectService, ILauncherService launcherService)
        {
            this.projectService = projectService;
            this.launcherService = launcherService;

            // Commands initialization
            NewProjectCommand = new RelayCommand(NewProject);
            NewFolderCommand = new RelayCommand(NewFolder);
            DeleteProjectCommand = new RelayCommand(DeleteProjectOrFolder, CanDeleteProjectOrFolder);
            MoveToFolderCommand = new RelayCommand(MoveToFolder, CanMoveProjectToFolder);
            CreateTestFoldersCommand = new RelayCommand(CreateTestFolders);
            AddItemCommand = new RelayCommand(AddItem);
            AddGroupCommand = new RelayCommand(AddGroup);
            SaveDataCommand = new RelayCommand(_ => SaveData());
            LaunchItemCommand = new RelayCommand(LaunchItem, CanLaunchItem);
            EditItemCommand = new RelayCommand(EditItem, CanEditItem);
            DeleteItemCommand = new RelayCommand(DeleteItem, CanDeleteItem);
            MoveItemUpCommand = new RelayCommand(MoveItemUp, CanMoveItemUp);
            MoveItemDownCommand = new RelayCommand(MoveItemDown, CanMoveItemDown);
            ExportProjectCommand = new RelayCommand(ExportProject, CanExportProject);
            ImportProjectCommand = new RelayCommand(ImportProject);
            ShowHelpCommand = new RelayCommand(ShowHelp);
            ShowColorSettingsCommand = new RelayCommand(ShowColorSettings);
            OpenWithVSCodeCommand = new RelayCommand(OpenWithVSCode, CanOpenWithVSCode);
            OpenWithOfficeCommand = new RelayCommand(OpenWithOffice, CanOpenWithOffice);

            LoadProjects();
            InitializeUI();
        }

        public ObservableCollection<Project> Projects
        {
            get => projects;
            set => SetProperty(ref projects, value);
        }

        public ObservableCollection<ProjectNode> ProjectNodes
        {
            get => projectNodes;
            set => SetProperty(ref projectNodes, value);
        }

        public Project? CurrentProject
        {
            get => currentProject;
            set
            {
                if (SetProperty(ref currentProject, value))
                {
                    UpdateAfterProjectChange();
                }
            }
        }

        public ProjectNode? SelectedProjectNode
        {
            get => selectedProjectNode;
            set
            {
                if (SetProperty(ref selectedProjectNode, value))
                {
                    if (value?.Project != null)
                    {
                        CurrentProject = value.Project;
                    }
                }
            }
        }

        public ItemGroup? SelectedViewGroup
        {
            get => selectedViewGroup;
            set
            {
                if (SetProperty(ref selectedViewGroup, value))
                {
                    ShowGroupItems(value);
                }
            }
        }

        public string SearchText
        {
            get => searchText;
            set
            {
                if (SetProperty(ref searchText, value))
                {
                    ApplySearch();
                }
            }
        }

        public LauncherItem? SelectedItem
        {
            get => selectedItem;
            set => SetProperty(ref selectedItem, value);
        }

        public string StatusText
        {
            get => statusText;
            set => SetProperty(ref statusText, value);
        }

        public string AppVersion => $"v{appVersion}";

        public ObservableCollection<LauncherItem> DisplayedItems { get; } = new ObservableCollection<LauncherItem>();

        private void LoadProjects()
        {
            try
            {
                var projectList = projectService.LoadProjectList();
                
                if (projectList.Any())
                {
                    foreach (var info in projectList.OrderBy(p => p.OrderIndex))
                    {
                        var project = projectService.LoadProject(info.Id);
                        if (project != null)
                        {
                            project.OrderIndex = info.OrderIndex;
                            project.ParentId = info.ParentId;
                            project.IsFolder = info.IsFolder;
                            UpdateProjectCompatibility(project);
                            Projects.Add(project);
                        }
                    }
                }

                if (Projects.Count == 0)
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
            
            Projects.Add(defaultProject);
            
            try
            {
                var projectInfoList = Projects.Select(p => new ProjectInfo
                {
                    Id = p.Id,
                    Name = p.Name,
                    OrderIndex = p.OrderIndex,
                    ParentId = p.ParentId,
                    IsFolder = p.IsFolder
                }).ToList();
                
                projectService.SaveProjectList(projectInfoList);
                projectService.SaveProject(defaultProject);
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

                // ItemType�̐ݒ�
                if (string.IsNullOrEmpty(item.ItemType))
                {
                    item.ItemType = launcherService.GetItemType(item.Path);
                }

                // Icon�̐ݒ�
                if (string.IsNullOrEmpty(item.Icon))
                {
                    item.Icon = launcherService.GetIconForPath(item.Path);
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

                // �O���[�v���̍X�V
                UpdateItemGroupNames(item);
            }

            // �A�C�e����OrderIndex�Ń\�[�g
            var sortedItems = project.Items.OrderBy(i => i.OrderIndex).ToList();
            project.Items.Clear();
            foreach (var item in sortedItems)
            {
                project.Items.Add(item);
            }
        }

        private void UpdateItemGroupNames(LauncherItem item)
        {
            if (CurrentProject != null && item.GroupIds != null)
            {
                var groupNames = CurrentProject.Groups
                    .Where(g => item.GroupIds.Contains(g.Id) && g.Id != "all")
                    .Select(g => g.Name);
                item.GroupNames = string.Join(", ", groupNames);
            }
        }

        private void InitializeUI()
        {
            if (Projects.Count > 0)
            {
                // �ŏ��̔�t�H���_�[�v���W�F�N�g��I��
                var firstProject = Projects.FirstOrDefault(p => !p.IsFolder);
                if (firstProject != null)
                {
                    CurrentProject = firstProject;
                    
                    // �Ή�����ProjectNode��I��
                    var projectNode = FindProjectNode(firstProject.Id);
                    if (projectNode != null)
                    {
                        SelectedProjectNode = projectNode;
                    }
                }
            }
            UpdateStatusText();
        }

        private void UpdateAfterProjectChange()
        {
            UpdateGroupList();
            SelectedViewGroup = null;
            ShowAllItems();
            UpdateStatusText();
            
            // �v���W�F�N�g�m�[�h�̕\�������X�V
            RefreshProjectNodeDisplayNames();
        }

        private void RefreshProjectNodeDisplayNames()
        {
            if (ProjectNodes != null)
            {
                RefreshAllNodeDisplayNames(ProjectNodes);
            }
        }

        private void UpdateGroupList()
        {
            if (CurrentProject != null)
            {
                var sortedGroups = CurrentProject.Groups.OrderBy(g => g.OrderIndex).ToList();
                CurrentProject.Groups.Clear();
                foreach (var group in sortedGroups)
                {
                    if (group.Id == "all")
                    {
                        group.ItemCount = $"{CurrentProject.Items.Count} �A�C�e��";
                    }
                    else
                    {
                        var count = CurrentProject.Items.Count(i => i.GroupIds != null && i.GroupIds.Contains(group.Id));
                        group.ItemCount = $"{count} �A�C�e��";
                    }
                    CurrentProject.Groups.Add(group);
                }
            }
        }

        private void ShowGroupItems(ItemGroup? group)
        {
            if (CurrentProject == null || group == null) return;
            
            IEnumerable<LauncherItem> items;
            if (group.Id == "all")
            {
                items = CurrentProject.Items;
            }
            else
            {
                items = CurrentProject.Items.Where(i =>
                    i.GroupIds != null && i.GroupIds.Contains(group.Id));
            }

            // �����t�B���^��K�p
            if (!string.IsNullOrEmpty(SearchText))
            {
                items = FilterItems(items, SearchText);
            }

            var sortedItems = items.OrderBy(i => i.OrderIndex);
            UpdateDisplayedItems(sortedItems);
            UpdateStatusText();
        }

        private void ShowAllItems()
        {
            if (CurrentProject != null)
            {
                var items = string.IsNullOrEmpty(SearchText)
                    ? CurrentProject.Items
                    : FilterItems(CurrentProject.Items, SearchText);

                var sortedItems = items.OrderBy(i => i.OrderIndex);
                UpdateDisplayedItems(sortedItems);
            }
        }

        private void UpdateDisplayedItems(IEnumerable<LauncherItem> items)
        {
            DisplayedItems.Clear();
            foreach (var item in items)
            {
                DisplayedItems.Add(item);
            }
        }

        private IEnumerable<LauncherItem> FilterItems(IEnumerable<LauncherItem> items, string searchText)
        {
            var lowerSearch = searchText.ToLower();
            return items.Where(item =>
                item.Name.ToLower().Contains(lowerSearch) ||
                item.Path.ToLower().Contains(lowerSearch) ||
                (item.Category?.ToLower().Contains(lowerSearch) ?? false) ||
                (item.Description?.ToLower().Contains(lowerSearch) ?? false) ||
                (item.GroupNames?.ToLower().Contains(lowerSearch) ?? false));
        }

        private void ApplySearch()
        {
            if (SelectedViewGroup != null)
            {
                ShowGroupItems(SelectedViewGroup);
            }
            else
            {
                ShowAllItems();
            }
        }

        private void UpdateStatusText()
        {
            if (CurrentProject != null)
            {
                StatusText = $"{CurrentProject.Items.Count} �̃I�u�W�F�N�g";
            }
        }

        private void SaveData()
        {
            try
            {
                if (CurrentProject != null)
                {
                    projectService.SaveProject(CurrentProject);
                }
                
                // �v���W�F�N�g��񃊃X�g���X�V
                var projectInfoList = Projects.Select(p => new ProjectInfo
                {
                    Id = p.Id,
                    Name = p.Name,
                    OrderIndex = p.OrderIndex,
                    ParentId = p.ParentId,
                    IsFolder = p.IsFolder
                }).ToList();
                
                projectService.SaveProjectList(projectInfoList);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"�ۑ��Ɏ��s���܂���: {ex.Message}", "�G���[",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Command implementations
        private void NewProject(object? parameter)
        {
            // ���p�\�ȃt�H���_�m�[�h���擾
            var availableFolders = new List<ProjectNode>();
            CollectFoldersRecursive(ProjectNodes, availableFolders);

            var dialog = new NewProjectDialog(availableFolders);
            dialog.Owner = Application.Current.MainWindow;
            
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.ProjectName))
            {
                var parentId = dialog.SelectedFolder?.Id; // null�̏ꍇ�̓��[�g���x��
                
                var newProject = new Project
                {
                    Name = dialog.ProjectName,
                    Id = Guid.NewGuid().ToString(),
                    OrderIndex = Projects.Count,
                    ParentId = parentId,
                    IsFolder = false
                };
                newProject.Groups.Add(new ItemGroup { Name = "���ׂ�", Id = "all", OrderIndex = 0 });
                newProject.Groups.Add(new ItemGroup { Name = "�悭�g��", Id = Guid.NewGuid().ToString(), OrderIndex = 1 });
                
                Projects.Add(newProject);
                
                try
                {
                    projectService.SaveProject(newProject);
                    
                    var projectInfoList = Projects.Select(p => new ProjectInfo
                    {
                        Id = p.Id,
                        Name = p.Name,
                        OrderIndex = p.OrderIndex,
                        ParentId = p.ParentId,
                        IsFolder = p.IsFolder
                    }).ToList();
                    
                    projectService.SaveProjectList(projectInfoList);
                    BuildProjectHierarchy();
                    
                    // �V�����쐬���ꂽ�v���W�F�N�g��I��
                    var newNode = FindProjectNode(newProject.Id);
                    if (newNode != null)
                    {
                        SelectedProjectNode = newNode;
                        
                        // �v���W�F�N�g���쐬���ꂽ�t�H���_��W�J
                        if (dialog.SelectedFolder != null)
                        {
                            var parentNode = FindProjectNode(dialog.SelectedFolder.Id);
                            if (parentNode != null)
                            {
                                parentNode.IsExpanded = true;
                            }
                        }
                    }
                    
                    // �������b�Z�[�W
                    var folderName = dialog.SelectedFolder?.Name ?? "���[�g";
                    MessageBox.Show($"�v���W�F�N�g�u{newProject.Name}�v���u{folderName}�v�ɍ쐬���܂����B", "�v���W�F�N�g�쐬����",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"�v���W�F�N�g�̕ۑ��Ɏ��s���܂���: {ex.Message}", "�G���[",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void NewFolder(object? parameter)
        {
            var dialog = new TextInputDialog("New Folder", "Enter folder name:");
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputText))
            {
                var parentId = SelectedProjectNode?.IsFolder == true ? SelectedProjectNode.Id : SelectedProjectNode?.ParentId;
                
                var newFolder = new Project
                {
                    Name = dialog.InputText,
                    Id = Guid.NewGuid().ToString(),
                    OrderIndex = Projects.Count,
                    ParentId = parentId,
                    IsFolder = true
                };
                
                Projects.Add(newFolder);
                
                System.Diagnostics.Debug.WriteLine($"Created new folder: {newFolder.Name} (ID: {newFolder.Id}, IsFolder: {newFolder.IsFolder})");
                
                try
                {
                    projectService.SaveProject(newFolder);
                    
                    var projectInfoList = Projects.Select(p => new ProjectInfo
                    {
                        Id = p.Id,
                        Name = p.Name,
                        OrderIndex = p.OrderIndex,
                        ParentId = p.ParentId,
                        IsFolder = p.IsFolder
                    }).ToList();
                    
                    projectService.SaveProjectList(projectInfoList);
                    BuildProjectHierarchy();
                    
                    // �V�����쐬���ꂽ�t�H���_�[��I��
                    var newNode = FindProjectNode(newFolder.Id);
                    if (newNode != null)
                    {
                        SelectedProjectNode = newNode;
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
        }

        private ProjectNode? FindProjectNode(string projectId)
        {
            return FindProjectNodeRecursive(ProjectNodes, projectId);
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

        private void AddItem(object? parameter)
        {
            if (CurrentProject == null)
            {
                MessageBox.Show("�v���W�F�N�g��I�����Ă�������", "�G���[",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new AddItemDialog(CurrentProject.Groups.ToList());
            if (dialog.ShowDialog() == true && dialog.Result != null)
            {
                var newItem = dialog.Result;
                newItem.OrderIndex = CurrentProject.Items.Count;
                newItem.ItemType = launcherService.GetItemType(newItem.Path);
                newItem.Icon = launcherService.GetIconForPath(newItem.Path);
                
                // �O���[�v�����X�V
                UpdateItemGroupNames(newItem);
                
                CurrentProject.Items.Add(newItem);
                UpdateGroupList();
                
                // �\�����X�V
                if (SelectedViewGroup != null)
                {
                    ShowGroupItems(SelectedViewGroup);
                }
                else
                {
                    ShowAllItems();
                }
                
                // �v���W�F�N�g�m�[�h�̕\�������X�V
                RefreshProjectNodeDisplayNames();
                
                SaveData();
            }
        }

        private void AddGroup(object? parameter)
        {
            if (CurrentProject == null)
            {
                MessageBox.Show("�v���W�F�N�g��I�����Ă�������", "�G���[",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new TextInputDialog("�V�K�O���[�v", "�O���[�v������͂��Ă�������:");
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputText))
            {
                var newGroup = new ItemGroup
                {
                    Name = dialog.InputText,
                    Id = Guid.NewGuid().ToString(),
                    OrderIndex = CurrentProject.Groups.Count
                };
                CurrentProject.Groups.Add(newGroup);
                UpdateGroupList();
                SaveData();
            }
        }

        private void LaunchItem(object? parameter)
        {
            var item = parameter as LauncherItem ?? SelectedItem;
            if (item != null)
            {
                try
                {
                    launcherService.LaunchItem(item);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "�G���[", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanLaunchItem(object? parameter)
        {
            return parameter is LauncherItem || SelectedItem != null;
        }

        private void EditItem(object? parameter)
        {
            var item = parameter as LauncherItem ?? SelectedItem;
            if (item != null && CurrentProject != null)
            {
                var dialog = new EditItemDialog(item, CurrentProject.Groups.ToList());
                if (dialog.ShowDialog() == true && dialog.Result != null)
                {
                    var editedItem = dialog.Result;
                    editedItem.ItemType = launcherService.GetItemType(editedItem.Path);
                    editedItem.Icon = launcherService.GetIconForPath(editedItem.Path);
                    
                    // �O���[�v�����X�V
                    UpdateItemGroupNames(editedItem);
                    
                    // ���̃A�C�e���ƒu������
                    var index = CurrentProject.Items.IndexOf(item);
                    if (index >= 0)
                    {
                        CurrentProject.Items[index] = editedItem;
                    }
                    
                    UpdateGroupList();
                    
                    // �\�����X�V
                    if (SelectedViewGroup != null)
                    {
                        ShowGroupItems(SelectedViewGroup);
                    }
                    else
                    {
                        ShowAllItems();
                    }
                    
                    SaveData();
                }
            }
        }

        private bool CanEditItem(object? parameter)
        {
            return parameter is LauncherItem || SelectedItem != null;
        }

        private void DeleteItem(object? parameter)
        {
            var item = parameter as LauncherItem ?? SelectedItem;
            if (item != null && CurrentProject != null)
            {
                var result = MessageBox.Show($"�u{item.Name}�v���폜���܂����H", "�m�F",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    CurrentProject.Items.Remove(item);
                    UpdateGroupList();
                    
                    if (SelectedViewGroup != null)
                    {
                        ShowGroupItems(SelectedViewGroup);
                    }
                    else
                    {
                        ShowAllItems();
                    }
                    
                    // �v���W�F�N�g�m�[�h�̕\�������X�V
                    RefreshProjectNodeDisplayNames();
                    
                    SaveData();
                }
            }
        }

        private bool CanDeleteItem(object? parameter)
        {
            return parameter is LauncherItem || SelectedItem != null;
        }

        private void MoveItemUp(object? parameter)
        {
            var item = parameter as LauncherItem ?? SelectedItem;
            if (item != null && CurrentProject != null)
            {
                var index = CurrentProject.Items.IndexOf(item);
                if (index > 0)
                {
                    // �\�[�g���N���A���Ă���A�C�e�����ړ�
                    ClearListViewSort();
                    
                    CurrentProject.Items.Move(index, index - 1);
                    UpdateItemOrderIndices();
                    SaveData();
                    RefreshItemsView();
                    
                    // �I����Ԃ��ێ�
                    SelectedItem = item;
                }
            }
        }

        private bool CanMoveItemUp(object? parameter)
        {
            var item = parameter as LauncherItem ?? SelectedItem;
            if (item != null && CurrentProject != null)
            {
                var index = CurrentProject.Items.IndexOf(item);
                return index > 0;
            }
            return false;
        }

        private void MoveItemDown(object? parameter)
        {
            var item = parameter as LauncherItem ?? SelectedItem;
            if (item != null && CurrentProject != null)
            {
                var index = CurrentProject.Items.IndexOf(item);
                if (index < CurrentProject.Items.Count - 1)
                {
                    // �\�[�g���N���A���Ă���A�C�e�����ړ�
                    ClearListViewSort();
                    
                    CurrentProject.Items.Move(index, index + 1);
                    UpdateItemOrderIndices();
                    SaveData();
                    RefreshItemsView();
                    
                    // �I����Ԃ��ێ�
                    SelectedItem = item;
                }
            }
        }

        private bool CanMoveItemDown(object? parameter)
        {
            var item = parameter as LauncherItem ?? SelectedItem;
            if (item != null && CurrentProject != null)
            {
                var index = CurrentProject.Items.IndexOf(item);
                return index < CurrentProject.Items.Count - 1;
            }
            return false;
        }

        private void ClearListViewSort()
        {
            // ���C��ListView�̃\�[�g���N���A
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                var listView = mainWindow.FindName("MainListView") as ListView;
                if (listView?.ItemsSource != null)
                {
                    var view = CollectionViewSource.GetDefaultView(listView.ItemsSource);
                    view?.SortDescriptions.Clear();
                    view?.Refresh();
                }
            }
        }

        private void UpdateItemOrderIndices()
        {
            if (CurrentProject != null)
            {
                for (int i = 0; i < CurrentProject.Items.Count; i++)
                {
                    CurrentProject.Items[i].OrderIndex = i;
                }
            }
        }

        private void RefreshItemsView()
        {
            if (SelectedViewGroup != null)
            {
                ShowGroupItems(SelectedViewGroup);
            }
            else
            {
                ShowAllItems();
            }
        }

        private void ExportProject(object? parameter)
        {
            if (CurrentProject == null)
            {
                MessageBox.Show("�G�N�X�|�[�g����v���W�F�N�g������܂���", "�G���[",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "�v���W�F�N�g���G�N�X�|�[�g",
                    Filter = "JSON�t�@�C�� (*.json)|*.json|���ׂẴt�@�C�� (*.*)|*.*",
                    FileName = $"{CurrentProject.Name}.json"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    projectService.ExportProject(CurrentProject, saveFileDialog.FileName);
                    MessageBox.Show($"�v���W�F�N�g�u{CurrentProject.Name}�v���G�N�X�|�[�g���܂���", "����",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"�G�N�X�|�[�g�Ɏ��s���܂���: {ex.Message}", "�G���[",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExportProject(object? parameter)
        {
            return CurrentProject != null;
        }

        private void ImportProject(object? parameter)
        {
            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "�v���W�F�N�g���C���|�[�g",
                    Filter = "JSON�t�@�C�� (*.json)|*.json|���ׂẴt�@�C�� (*.*)|*.*"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var importedProject = projectService.ImportProject(openFileDialog.FileName);
                    if (importedProject != null)
                    {
                        // ID�̏d���������
                        importedProject.Id = Guid.NewGuid().ToString();
                        importedProject.OrderIndex = Projects.Count;
                        
                        // �݊����`�F�b�N
                        UpdateProjectCompatibility(importedProject);
                        
                        Projects.Add(importedProject);
                        CurrentProject = importedProject;
                        
                        var projectInfoList = Projects.Select(p => new ProjectInfo
                        {
                            Id = p.Id,
                            Name = p.Name,
                            OrderIndex = p.OrderIndex,
                            ParentId = p.ParentId,
                            IsFolder = p.IsFolder
                        }).ToList();
                        
                        projectService.SaveProjectList(projectInfoList);
                        projectService.SaveProject(importedProject);
                        
                        MessageBox.Show($"�v���W�F�N�g�u{importedProject.Name}�v���C���|�[�g���܂���", "����",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"�C���|�[�g�Ɏ��s���܂���: {ex.Message}", "�G���[",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowHelp(object? parameter)
        {
            var helpDialog = new HelpDialog();
            helpDialog.ShowDialog();
        }

        private void ShowColorSettings(object? parameter)
        {
            var colorSettingsDialog = new ColorSettingsDialog();
            colorSettingsDialog.Owner = Application.Current.MainWindow;
            colorSettingsDialog.ShowDialog();
        }

        private void DeleteProjectOrFolder(object? parameter)
        {
            if (SelectedProjectNode == null) return;

            var itemType = SelectedProjectNode.IsFolder ? "�t�H���_�[" : "�v���W�F�N�g";
            var result = MessageBox.Show($"{itemType}�u{SelectedProjectNode.Name}�v���폜���܂����H\n\n���q�v�f���ꏏ�ɍ폜����܂��B", 
                "�m�F", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // �폜�Ώۂ�ID���X�g�����W�i�q�v�f���܂ށj
                    var idsToDelete = new List<string>();
                    CollectNodeIds(SelectedProjectNode, idsToDelete);

                    // �v���W�F�N�g�R���N�V��������폜
                    var projectsToRemove = Projects.Where(p => idsToDelete.Contains(p.Id)).ToList();
                    foreach (var project in projectsToRemove)
                    {
                        Projects.Remove(project);
                        
                        // �t�@�C��������폜
                        try
                        {
                            projectService.DeleteProject(project.Id);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"�v���W�F�N�g�t�@�C���폜�G���[: {ex.Message}");
                        }
                    }

                    // �v���W�F�N�g�ꗗ��ۑ�
                    var projectInfoList = Projects.Select(p => new ProjectInfo
                    {
                        Id = p.Id,
                        Name = p.Name,
                        OrderIndex = p.OrderIndex,
                        ParentId = p.ParentId,
                        IsFolder = p.IsFolder
                    }).ToList();
                    
                    projectService.SaveProjectList(projectInfoList);

                    // �K�w�\�����č\�z
                    BuildProjectHierarchy();

                    // �c��̃v���W�F�N�g������ꍇ�͍ŏ��̃v���W�F�N�g��I��
                    var firstProject = ProjectNodes.FirstOrDefault(n => !n.IsFolder);
                    if (firstProject == null)
                    {
                        // �t�H���_�[���̍ŏ��̃v���W�F�N�g��T��
                        firstProject = FindFirstProjectRecursive(ProjectNodes);
                    }
                    
                    if (firstProject != null)
                    {
                        SelectedProjectNode = firstProject;
                    }
                    else
                    {
                        // �v���W�F�N�g���S���Ȃ��ꍇ�̓f�t�H���g�v���W�F�N�g���쐬
                        CurrentProject = null;
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

        private bool CanDeleteProjectOrFolder(object? parameter)
        {
            return SelectedProjectNode != null;
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

        public void AddItemFromPath(string path)
        {
            if (CurrentProject == null)
            {
                MessageBox.Show("�v���W�F�N�g��I�����Ă�������", "�G���[",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            try
            {
                // �����̃A�C�e���œ����p�X���Ȃ����`�F�b�N
                if (CurrentProject.Items.Any(i => i.Path.Equals(path, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show($"�u{path}�v�͊��ɒǉ�����Ă��܂��B", "���",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // �t�@�C�������疼�O�𐶐�
                string name;
                string description;

                if (IsUrl(path))
                {
                    // URL�̏ꍇ�̓h���C�����𖼑O�Ƃ��Ďg�p
                    try
                    {
                        var uri = new Uri(path);
                        name = uri.Host;
                        if (name.StartsWith("www."))
                        {
                            name = name.Substring(4);
                        }
                        description = $"Web�T�C�g: {path}";
                    }
                    catch
                    {
                        name = "Web�T�C�g";
                        description = $"Web�T�C�g: {path}";
                    }
                }
                else if (System.IO.Directory.Exists(path))
                {
                    name = System.IO.Path.GetFileName(path.TrimEnd('\\', '/'));
                    description = $"�t�H���_: {path}";
                }
                else if (System.IO.File.Exists(path))
                {
                    name = System.IO.Path.GetFileNameWithoutExtension(path);
                    description = $"�t�@�C��: {System.IO.Path.GetFileName(path)}";
                }
                else
                {
                    name = System.IO.Path.GetFileName(path);
                    description = $"�h���b�O&�h���b�v�Œǉ�: {name}";
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    name = path;
                }

                // ���ނ���������
                string category = DetermineCategory(path);

                // �V�����A�C�e�����쐬
                var newItem = new LauncherItem
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = name,
                    Path = path,
                    Description = description,
                    Category = category,
                    GroupIds = new List<string>(), // �f�t�H���g�O���[�v�Ȃ�
                    OrderIndex = CurrentProject.Items.Count,
                    ItemType = launcherService.GetItemType(path),
                    Icon = launcherService.GetIconForPath(path)
                };

                // �O���[�v�����X�V
                UpdateItemGroupNames(newItem);

                // �v���W�F�N�g�ɒǉ�
                CurrentProject.Items.Add(newItem);
                UpdateGroupList();

                // �\�����X�V
                if (SelectedViewGroup != null)
                {
                    ShowGroupItems(SelectedViewGroup);
                }
                else
                {
                    ShowAllItems();
                }

                // �v���W�F�N�g�m�[�h�̕\�������X�V
                RefreshProjectNodeDisplayNames();

                // �f�[�^��ۑ�
                SaveData();

                // �������b�Z�[�W�i�I�v�V�����j
                StatusText = $"�u{name}�v��ǉ����܂��� ({CurrentProject.Items.Count} �̃I�u�W�F�N�g)";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"�A�C�e���̒ǉ��Ɏ��s���܂���: {ex.Message}", ex);
            }
        }

        public void ShowAddItemDialogWithPath(string path)
        {
            if (CurrentProject == null)
            {
                MessageBox.Show("�v���W�F�N�g��I�����Ă�������", "�G���[",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            try
            {
                // �����̃A�C�e���œ����p�X���Ȃ����`�F�b�N
                if (CurrentProject.Items.Any(i => i.Path.Equals(path, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show($"�u{path}�v�͊��ɒǉ�����Ă��܂��B", "���",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // AddItemDialog�Ɏ��O����ݒ肵�ĕ\��
                var dialog = new AddItemDialog(CurrentProject.Groups.ToList());
                
                // �t�@�C�������疼�O�𐶐�
                string name;
                string description;
                
                if (IsUrl(path))
                {
                    // URL�̏ꍇ�̓h���C�����𖼑O�Ƃ��Ďg�p
                    try
                    {
                        var uri = new Uri(path);
                        name = uri.Host;
                        if (name.StartsWith("www."))
                        {
                            name = name.Substring(4);
                        }
                        description = $"Web�T�C�g: {path}";
                    }
                    catch
                    {
                        name = "Web�T�C�g";
                        description = $"Web�T�C�g: {path}";
                    }
                }
                else if (System.IO.Directory.Exists(path))
                {
                    name = System.IO.Path.GetFileName(path.TrimEnd('\\', '/'));
                    description = $"�t�H���_: {path}";
                }
                else if (System.IO.File.Exists(path))
                {
                    name = System.IO.Path.GetFileNameWithoutExtension(path);
                    description = $"�t�@�C��: {System.IO.Path.GetFileName(path)}";
                }
                else
                {
                    name = System.IO.Path.GetFileName(path);
                    description = $"�h���b�O&�h���b�v�Œǉ�: {name}";
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    name = path;
                }

                // ���ނ���������
                string category = DetermineCategory(path);

                // �_�C�A���O�ɏ����l��ݒ�
                dialog.SetInitialValues(name, path, category, description);

                if (dialog.ShowDialog() == true && dialog.Result != null)
                {
                    var newItem = dialog.Result;
                    newItem.OrderIndex = CurrentProject.Items.Count;
                    newItem.ItemType = launcherService.GetItemType(newItem.Path);
                    newItem.Icon = launcherService.GetIconForPath(newItem.Path);
                    
                    // �O���[�v�����X�V
                    UpdateItemGroupNames(newItem);
                    
                    CurrentProject.Items.Add(newItem);
                    UpdateGroupList();
                    
                    // �\�����X�V
                    if (SelectedViewGroup != null)
                    {
                        ShowGroupItems(SelectedViewGroup);
                    }
                    else
                    {
                        ShowAllItems();
                    }
                    
                    SaveData();
                    
                    // �������b�Z�[�W
                    StatusText = $"�u{newItem.Name}�v��ǉ����܂��� ({CurrentProject.Items.Count} �̃I�u�W�F�N�g)";
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"�A�C�e���̒ǉ��Ɏ��s���܂���: {ex.Message}", ex);
            }
        }

        private bool IsUrl(string path)
        {
            return !string.IsNullOrEmpty(path) && 
                   (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                    path.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
        }

        private string DetermineCategory(string path)
        {
            try
            {
                // URL�̏ꍇ
                if (IsUrl(path))
                {
                    var uri = new Uri(path);
                    var host = uri.Host.ToLower();
                    
                    if (host.Contains("github.com"))
                        return "GitHubURL";
                    else if (host.Contains("gitlab.com"))
                        return "GitLabURL";
                    else if (host.Contains("drive.google.com") || host.Contains("docs.google.com"))
                        return "Google�h���C�u";
                    else if (host.Contains("teams.microsoft.com") || host.Contains("teams.live.com"))
                        return "MicrosoftTeams";
                    else if (host.Contains("sharepoint.com") || host.Contains(".sharepoint.com") || 
                             host.EndsWith("sharepoint.com") || host.Contains("office365.sharepoint.com"))
                        return "SharePoint";
                    else if (host.Contains("outlook.office365.com") || host.Contains("outlook.office.com") ||
                             host.Contains("onedrive.live.com") || host.Contains("1drv.ms"))
                        return "SharePoint"; // OneDrive��SharePoint�J�e�S���Ɋ܂߂�
                    else
                        return "Web�T�C�g";
                }

                // ���[�J���p�X�̏ꍇ
                if (System.IO.Directory.Exists(path))
                {
                    // G:�h���C�u�̏ꍇ��Google�h���C�u�Ɣ���
                    if (path.StartsWith("G:", StringComparison.OrdinalIgnoreCase) || 
                        path.StartsWith("G\\", StringComparison.OrdinalIgnoreCase))
                    {
                        return "Google�h���C�u";
                    }
                    return "�t�H���_";
                }

                if (System.IO.File.Exists(path))
                {
                    // G:�h���C�u��̃t�@�C����Google�h���C�u�Ɣ���
                    if (path.StartsWith("G:", StringComparison.OrdinalIgnoreCase) || 
                        path.StartsWith("G\\", StringComparison.OrdinalIgnoreCase))
                    {
                        return "Google�h���C�u";
                    }

                    var extension = System.IO.Path.GetExtension(path).ToLower();
                    var fileName = System.IO.Path.GetFileName(path).ToLower();
                    
                    return extension switch
                    {
                        ".exe" or ".msi" or ".bat" or ".cmd" => "�A�v���P�[�V����",
                        ".txt" or ".rtf" => "�h�L�������g",
                        ".doc" or ".docx" => "Word",
                        ".xls" or ".xlsx" => "Excel",
                        ".ppt" or ".pptx" => "PowerPoint",
                        ".pdf" => "PDF",
                        ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".svg" or ".webp" => "�摜",
                        ".mp3" or ".wav" or ".wma" or ".flac" or ".aac" or ".ogg" => "���y",
                        ".mp4" or ".avi" or ".mkv" or ".wmv" or ".mov" or ".flv" or ".webm" => "����",
                        ".zip" or ".rar" or ".7z" or ".tar" or ".gz" or ".bz2" => "�A�[�J�C�u",
                        ".lnk" => "�V���[�g�J�b�g",
                        ".py" or ".js" or ".html" or ".css" or ".cpp" or ".c" or ".cs" or ".java" or ".php" => "�h�L�������g",
                        _ => "�t�@�C��"
                    };
                }

                // �p�X�����݂��Ȃ��ꍇ��G:�Ŏn�܂��Ă����Google�h���C�u�Ɣ���
                if (path.StartsWith("G:", StringComparison.OrdinalIgnoreCase) || 
                    path.StartsWith("G\\", StringComparison.OrdinalIgnoreCase))
                {
                    return "Google�h���C�u";
                }

                return "���̑�";
            }
            catch
            {
                return "���̑�";
            }
        }

        private void BuildProjectHierarchy()
        {
            ProjectNodes.Clear();
            var nodeMap = new Dictionary<string, ProjectNode>();

            // ���ׂẴv���W�F�N�g�ƃt�H���_�[����m�[�h���쐬
            foreach (var project in Projects.OrderBy(p => p.OrderIndex))
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
                    // �e���Ȃ��ꍇ�̓��[�g���x���ɒǉ�
                    ProjectNodes.Add(node);
                }
            }

            // �\�[�g
            SortProjectNodes(ProjectNodes);
            
            // ���ׂẴm�[�h��DisplayName���X�V
            RefreshAllNodeDisplayNames(ProjectNodes);
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

        private void MoveToFolder(object? parameter)
        {
            if (SelectedProjectNode == null) return;

            // �f�o�b�O�F���݂̃v���W�F�N�g�󋵂��m�F
            System.Diagnostics.Debug.WriteLine("=== Current Projects Status ===");
            foreach (var project in Projects)
            {
                System.Diagnostics.Debug.WriteLine($"Project: {project.Name} | IsFolder: {project.IsFolder} | ParentId: {project.ParentId ?? "null"}");
            }
            System.Diagnostics.Debug.WriteLine("=== ProjectNodes Status ===");
            PrintProjectNodes(ProjectNodes, 0);

            // ���p�\�ȃt�H���_�[���擾�i�ړ��Ώێ��g�Ǝq�t�H���_�[�͏��O�j
            var availableFolders = GetAvailableFolders(SelectedProjectNode);
            
            // �f�o�b�O: ���p�\�ȃt�H���_�[�����m�F
            System.Diagnostics.Debug.WriteLine($"Available folders for move: {availableFolders.Count}");
            foreach (var folder in availableFolders)
            {
                System.Diagnostics.Debug.WriteLine($"  - {folder.Name} (ID: {folder.Id}, IsFolder: {folder.IsFolder})");
            }
            
            // �e�X�g�p�̉��z�t�H���_�[��ǉ��i�f�o�b�O�ړI�j
            if (availableFolders.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("No folders found - this is the issue!");
                
                // �e�X�g�ړI�ňꎞ�I�ȃt�H���_�[���쐬
                MessageBox.Show("No folders available for moving. Please create a folder first using the [F] button.", 
                    "No Folders", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            var dialog = new FolderSelectionDialog(availableFolders, 
                $"Select destination folder for '{SelectedProjectNode.Name}'");
            dialog.Owner = Application.Current.MainWindow;
            
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var targetFolder = dialog.SelectedFolder;
                    var targetParentId = targetFolder?.Id; // null�̏ꍇ�̓��[�g���x��

                    System.Diagnostics.Debug.WriteLine($"Selected target folder: {targetFolder?.Name ?? "[Root]"}");

                    // �v���W�F�N�g�̐eID���X�V
                    var project = Projects.FirstOrDefault(p => p.Id == SelectedProjectNode.Id);
                    if (project != null)
                    {
                        project.ParentId = targetParentId;
                        
                        // �f�[�^��ۑ�
                        SaveData();
                        
                        // �K�w�\�����č\�z
                        BuildProjectHierarchy();
                        
                        // �ړ������v���W�F�N�g���đI��
                        var movedNode = FindProjectNode(SelectedProjectNode.Id);
                        if (movedNode != null)
                        {
                            SelectedProjectNode = movedNode;
                            
                            // �ړ���t�H���_�[��W�J
                            if (targetFolder != null)
                            {
                                var targetNode = FindProjectNode(targetFolder.Id);
                                if (targetNode != null)
                                {
                                    targetNode.IsExpanded = true;
                                }
                            }
                        }
                        
                        MessageBox.Show("Move completed successfully.", "Move Complete", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Move failed: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void PrintProjectNodes(ObservableCollection<ProjectNode> nodes, int level)
        {
            var indent = new string(' ', level * 2);
            foreach (var node in nodes)
            {
                System.Diagnostics.Debug.WriteLine($"{indent}{node.Name} | IsFolder: {node.IsFolder} | Children: {node.Children.Count}");
                if (node.Children.Count > 0)
                {
                    PrintProjectNodes(node.Children, level + 1);
                }
            }
        }

        private List<ProjectNode> GetAvailableFolders(ProjectNode excludeNode)
        {
            var allFolders = new List<ProjectNode>();
            CollectFoldersRecursive(ProjectNodes, allFolders);
            
            System.Diagnostics.Debug.WriteLine($"Total folders found: {allFolders.Count}");
            
            // �ړ��Ώێ��g�Ǝq�t�H���_�[�����O
            var excludeIds = new HashSet<string>();
            CollectNodeIdsRecursive(excludeNode, excludeIds);
            
            System.Diagnostics.Debug.WriteLine($"Excluding {excludeIds.Count} items");
            
            var availableFolders = allFolders.Where(f => !excludeIds.Contains(f.Id)).ToList();
            System.Diagnostics.Debug.WriteLine($"Available folders after exclusion: {availableFolders.Count}");
            
            return availableFolders;
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

        private void CreateTestFolders(object? parameter)
        {
            var result = MessageBox.Show("Create test folders for debugging?\n\nThis will create:\n- Work Folder\n- Personal Folder\n- Development Folder", 
                "Create Test Folders", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var testFolders = new[]
                    {
                        new { Name = "Work", ParentId = (string?)null },
                        new { Name = "Personal", ParentId = (string?)null },
                        new { Name = "Development", ParentId = (string?)null }
                    };

                    foreach (var folderData in testFolders)
                    {
                        var newFolder = new Project
                        {
                            Name = folderData.Name,
                            Id = Guid.NewGuid().ToString(),
                            OrderIndex = Projects.Count,
                            ParentId = folderData.ParentId,
                            IsFolder = true
                        };
                        
                        Projects.Add(newFolder);
                        System.Diagnostics.Debug.WriteLine($"Created test folder: {newFolder.Name} (ID: {newFolder.Id}, IsFolder: {newFolder.IsFolder})");
                    }
                    
                    var projectInfoList = Projects.Select(p => new ProjectInfo
                    {
                        Id = p.Id,
                        Name = p.Name,
                        OrderIndex = p.OrderIndex,
                        ParentId = p.ParentId,
                        IsFolder = p.IsFolder
                    }).ToList();
                    
                    projectService.SaveProjectList(projectInfoList);
                    
                    // �t�H���_�[���ۑ�
                    foreach (var project in Projects.Where(p => p.IsFolder))
                    {
                        projectService.SaveProject(project);
                    }
                    
                    BuildProjectHierarchy();
                    
                    MessageBox.Show("Test folders created successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to create test folders: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanMoveProjectToFolder(object? parameter)
        {
            return SelectedProjectNode != null;
        }

        private void OpenWithVSCode(object? parameter)
        {
            var item = parameter as LauncherItem ?? SelectedItem;
            if (item != null)
            {
                try
                {
                    string path = item.Path;
                    
                    // VSCode�̃p�X���m�F
                    string vscodeCommand = FindVSCodePath();
                    if (string.IsNullOrEmpty(vscodeCommand))
                    {
                        MessageBox.Show("Visual Studio Code ��������܂���B\nVS Code���C���X�g�[������Ă��邩�m�F���Ă��������B", 
                            "�G���[", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // URL�̏ꍇ�̓p�X�w��ł��Ȃ����߁A�G���[���b�Z�[�W��\��
                    if (IsUrl(path))
                    {
                        MessageBox.Show("Web�T�C�g��URL��VS Code�ŊJ�����Ƃ��ł��܂���B", 
                            "�G���[", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    // �t�@�C���܂��̓t�H���_�[�����݂��邩�m�F
                    if (!System.IO.File.Exists(path) && !System.IO.Directory.Exists(path))
                    {
                        MessageBox.Show($"�w�肳�ꂽ�p�X��������܂���: {path}", 
                            "�G���[", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // VS Code�ŊJ��
                    var startInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = vscodeCommand,
                        Arguments = $"\"{path}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    System.Diagnostics.Process.Start(startInfo);
                    StatusText = $"�u{item.Name}�v��VS Code�ŊJ���܂���";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"VS Code�ŊJ���ۂɃG���[���������܂���: {ex.Message}", 
                        "�G���[", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanOpenWithVSCode(object? parameter)
        {
            var item = parameter as LauncherItem ?? SelectedItem;
            if (item == null) return false;

            // URL�̏ꍇ�͖���
            if (IsUrl(item.Path)) return false;

            // �t�@�C���܂��̓t�H���_�[�����݂���ꍇ�̂ݗL��
            return System.IO.File.Exists(item.Path) || System.IO.Directory.Exists(item.Path);
        }

        private void OpenWithOffice(object? parameter)
        {
            var item = parameter as LauncherItem ?? SelectedItem;
            if (item != null)
            {
                try
                {
                    string path = item.Path;

                    // SharePoint��Office Online URL�̏ꍇ�͒��ڃu���E�U�ŊJ��
                    if (IsOfficeUrl(path))
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = path,
                            UseShellExecute = true
                        });
                        StatusText = $"�u{item.Name}�v��Office�ŊJ���܂���";
                        return;
                    }

                    // ���[�J���t�@�C���̏ꍇ
                    if (System.IO.File.Exists(path))
                    {
                        string extension = System.IO.Path.GetExtension(path).ToLower();
                        
                        // Office�t�@�C�����ǂ����m�F
                        if (IsOfficeFile(extension))
                        {
                            // �֘A�t�����ꂽ�A�v���P�[�V�����ŊJ���i�ʏ��Office�j
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = path,
                                UseShellExecute = true
                            });
                            StatusText = $"�u{item.Name}�v��Office�ŊJ���܂���";
                        }
                        else
                        {
                            MessageBox.Show("���̃t�@�C����Office�h�L�������g�ł͂���܂���B", 
                                "���", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"�t�@�C����������܂���: {path}", 
                            "�G���[", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Office�ŊJ���ۂɃG���[���������܂���: {ex.Message}", 
                        "�G���[", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanOpenWithOffice(object? parameter)
        {
            var item = parameter as LauncherItem ?? SelectedItem;
            if (item == null) return false;

            string path = item.Path;

            // SharePoint��Office Online URL�̏ꍇ�͗L��
            if (IsOfficeUrl(path)) return true;

            // ���[�J���t�@�C���̏ꍇ��Office�t�@�C�����ǂ����m�F
            if (System.IO.File.Exists(path))
            {
                string extension = System.IO.Path.GetExtension(path).ToLower();
                return IsOfficeFile(extension);
            }

            return false;
        }

        private string FindVSCodePath()
        {
            // ��ʓI��VS Code�̃C���X�g�[���p�X���m�F
            var possiblePaths = new[]
            {
                @"C:\Users\" + Environment.UserName + @"\AppData\Local\Programs\Microsoft VS Code\Code.exe",
                @"C:\Program Files\Microsoft VS Code\Code.exe",
                @"C:\Program Files (x86)\Microsoft VS Code\Code.exe",
                "code" // PATH���ϐ��ɓo�^����Ă���ꍇ
            };

            foreach (var path in possiblePaths)
            {
                if (path == "code")
                {
                    // PATH���ϐ��ł̊m�F
                    try
                    {
                        var startInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "where",
                            Arguments = "code",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        };
                        var process = System.Diagnostics.Process.Start(startInfo);
                        var output = process?.StandardOutput.ReadToEnd();
                        process?.WaitForExit();
                        
                        if (process?.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                        {
                            return "code";
                        }
                    }
                    catch
                    {
                        // where�R�}���h�����s�����ꍇ�͎���
                    }
                }
                else if (System.IO.File.Exists(path))
                {
                    return path;
                }
            }

            return string.Empty;
        }

        private bool IsOfficeUrl(string path)
        {
            if (!IsUrl(path)) return false;

            try
            {
                var uri = new Uri(path);
                var host = uri.Host.ToLower();

                return host.Contains("sharepoint.com") ||
                       host.Contains("office365.sharepoint.com") ||
                       host.Contains("onedrive.live.com") ||
                       host.Contains("1drv.ms") ||
                       host.Contains("office.com") ||
                       host.Contains("outlook.office365.com") ||
                       path.Contains("/_layouts/") ||
                       path.Contains("/workbook/") ||
                       path.Contains("/document/") ||
                       path.Contains("/presentation/");
            }
            catch
            {
                return false;
            }
        }

        private bool IsOfficeFile(string extension)
        {
            var officeExtensions = new[]
            {
                ".doc", ".docx", ".docm",           // Word
                ".xls", ".xlsx", ".xlsm", ".xlsb",  // Excel  
                ".ppt", ".pptx", ".pptm",           // PowerPoint
                ".vsd", ".vsdx",                    // Visio
                ".mpp",                             // Project
                ".pub",                             // Publisher
                ".one"                              // OneNote
            };

            return officeExtensions.Contains(extension);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}