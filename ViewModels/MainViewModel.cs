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
using PathHub.Utils;

namespace ModernLauncher.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IProjectService projectService;
        private readonly ILauncherService launcherService;
        private readonly ISmartLauncherService smartLauncherService;
        
        private ObservableCollection<Project> projects = new ObservableCollection<Project>();
        private ObservableCollection<ProjectNode> projectNodes = new ObservableCollection<ProjectNode>();
        private Project? currentProject;
        private ProjectNode? selectedProjectNode;
        private ItemGroup? selectedViewGroup;
        private string searchText = string.Empty;
        private LauncherItem? selectedItem;
        private string statusText = string.Empty;

        // SmartLauncher properties
        private ObservableCollection<SmartLauncherItem> smartLauncherItems = new ObservableCollection<SmartLauncherItem>();
        private SmartLauncherItem? selectedSmartLauncherItem;
        private bool isSmartLauncherMode = false;

        // 全プロジェクト表示用のフラグを追加
        private bool isShowingAllProjects = false;
        public bool IsShowingAllProjects
        {
            get => isShowingAllProjects;
            set => SetProperty(ref isShowingAllProjects, value);
        }

        // 現在表示中のプロジェクトのリスト（フォルダ展開時用）
        private List<Project> currentDisplayedProjects = new List<Project>();

        // SmartLauncher properties
        public ObservableCollection<SmartLauncherItem> SmartLauncherItems
        {
            get => smartLauncherItems;
            set => SetProperty(ref smartLauncherItems, value);
        }

        public SmartLauncherItem? SelectedSmartLauncherItem
        {
            get => selectedSmartLauncherItem;
            set
            {
                if (SetProperty(ref selectedSmartLauncherItem, value))
                {
                    HandleSmartLauncherItemSelectionChange(value);
                }
            }
        }

        public bool IsSmartLauncherMode
        {
            get => isSmartLauncherMode;
            set => SetProperty(ref isSmartLauncherMode, value);
        }

        // Commands
        public ICommand NewProjectCommand { get; }
        public ICommand NewFolderCommand { get; }
        public ICommand DeleteProjectCommand { get; }
        public ICommand EditProjectCommand { get; }
        public ICommand MoveToFolderCommand { get; }
        public ICommand CreateTestFoldersCommand { get; }
        public ICommand AddItemCommand { get; }
        public ICommand AddGroupCommand { get; }
        public ICommand EditGroupCommand { get; }
        public ICommand DeleteGroupCommand { get; }
        public ICommand SaveDataCommand { get; }
        public ICommand LaunchItemCommand { get; }
        public ICommand LaunchGroupCommand { get; }
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
        public ICommand OpenInExplorerCommand { get; }
        
        // プロジェクトの並び替えコマンド
        public ICommand MoveProjectUpCommand { get; }
        public ICommand MoveProjectDownCommand { get; }
        
        // グループの並び替えコマンド
        public ICommand MoveGroupUpCommand { get; }
        public ICommand MoveGroupDownCommand { get; }

        // SmartLauncher commands
        public ICommand RefreshSmartLauncherCommand { get; }
        
        // ショートカットコマンド
        public ICommand FocusSearchCommand { get; }
        public ICommand FocusProjectCommand { get; }
        public ICommand FocusGroupCommand { get; }
        public ICommand FocusMainListCommand { get; }
        public ICommand FocusSmartLauncherCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand SearchAllProjectsCommand { get; }

        public MainViewModel() : this(
            ServiceLocator.Instance.GetService<IProjectService>(), 
            ServiceLocator.Instance.GetService<ILauncherService>(),
            ServiceLocator.Instance.GetService<ISmartLauncherService>())
        {
        }

        public MainViewModel(IProjectService projectService, ILauncherService launcherService, ISmartLauncherService smartLauncherService)
        {
            // デバッグログをクリア
            DebugLogger.Clear();
            DebugLogger.Log("MainViewModel constructor started");

            this.projectService = projectService;
            this.launcherService = launcherService;
            this.smartLauncherService = smartLauncherService;

            // Commands initialization
            NewProjectCommand = new RelayCommand(NewProject, CanNewProject);
            NewFolderCommand = new RelayCommand(NewFolder, CanNewFolder);
            DeleteProjectCommand = new RelayCommand(DeleteProjectOrFolder, CanDeleteProjectOrFolder);
            EditProjectCommand = new RelayCommand(EditProjectOrFolder, CanEditProjectOrFolder);
            MoveToFolderCommand = new RelayCommand(MoveToFolder, CanMoveProjectToFolder);
            CreateTestFoldersCommand = new RelayCommand(CreateTestFolders);
            AddItemCommand = new RelayCommand(AddItem);
            AddGroupCommand = new RelayCommand(AddGroup);
            EditGroupCommand = new RelayCommand(EditGroup, CanEditGroup);
            DeleteGroupCommand = new RelayCommand(DeleteGroup, CanDeleteGroup);
            SaveDataCommand = new RelayCommand(_ => SaveData());
            LaunchItemCommand = new RelayCommand(LaunchItem, CanLaunchItem);
            LaunchGroupCommand = new RelayCommand(LaunchGroup, CanLaunchGroup);
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
            OpenInExplorerCommand = new RelayCommand(OpenInExplorer, CanOpenInExplorer);
            
            // プロジェクトの並び替えコマンド
            MoveProjectUpCommand = new RelayCommand(MoveProjectUp, CanMoveProjectUp);
            MoveProjectDownCommand = new RelayCommand(MoveProjectDown, CanMoveProjectDown);
            
            // グループの並び替えコマンド
            MoveGroupUpCommand = new RelayCommand(MoveGroupUp, CanMoveGroupUp);
            MoveGroupDownCommand = new RelayCommand(MoveGroupDown, CanMoveGroupDown);

            // SmartLauncher commands
            RefreshSmartLauncherCommand = new RelayCommand(RefreshSmartLauncher);
            
            // ショートカットコマンド
            FocusSearchCommand = new RelayCommand(FocusSearch);
            FocusProjectCommand = new RelayCommand(FocusProject);
            FocusGroupCommand = new RelayCommand(FocusGroup);
            FocusMainListCommand = new RelayCommand(FocusMainList);
            FocusSmartLauncherCommand = new RelayCommand(FocusSmartLauncher);
            ClearSearchCommand = new RelayCommand(ClearSearch);
            SearchAllProjectsCommand = new RelayCommand(SearchAllProjects);

            LoadProjects();
            LoadSmartLauncherItems();
            InitializeUI();
            
            // アプリケーション終了時のイベントハンドラーを設定
            if (Application.Current != null)
            {
                Application.Current.Exit += Application_Exit;
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // アプリケーション終了時にデータを保存
            SaveData();
        }

        private void LoadSmartLauncherItems()
        {
            try
            {
                var smartItems = smartLauncherService.GetSmartLauncherItems(Projects);
                SmartLauncherItems.Clear();
                foreach (var item in smartItems)
                {
                    SmartLauncherItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading smart launcher items: {ex.Message}");
            }
        }

        private void RefreshSmartLauncher(object? parameter)
        {
            LoadSmartLauncherItems();
        }

        private void HandleSmartLauncherItemSelectionChange(SmartLauncherItem? item)
        {
            if (item != null)
            {
                IsSmartLauncherMode = true;
                IsShowingAllProjects = false;
                CurrentProject = null;
                SelectedProjectNode = null;
                SelectedViewGroup = null;
                
                // 検索フィルタを考慮してアイテムを表示
                ApplySmartLauncherSearch();
                
                System.Diagnostics.Debug.WriteLine($"Smart launcher item selected: {item.DisplayName}");
            }
        }

        private void RecordItemAccess(LauncherItem item)
        {
            try
            {
                var projectName = item.ProjectName ?? CurrentProject?.Name ?? "Unknown";
                smartLauncherService.RecordPathAccess(item.Path, item.Name, item.Category, projectName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error recording item access: {ex.Message}");
            }
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
                var previousNode = selectedProjectNode;
                var nodeChanged = SetProperty(ref selectedProjectNode, value);
                
                // プロジェクトノードが変更された場合、または同じノードでも強制的に更新を行う
                if (nodeChanged || (value != null && previousNode == value))
                {
                    System.Diagnostics.Debug.WriteLine($"Handling project node selection: {value?.Name ?? "null"}");
                    HandleProjectNodeSelectionChange(value);
                }
            }
        }

        private void HandleProjectNodeSelectionChange(ProjectNode? value)
        {
            if (value?.Project != null)
            {
                // 単一プロジェクトが選択された場合
                System.Diagnostics.Debug.WriteLine($"Switching to project: {value.Project.Name}");

                // 状態をリセット
                IsSmartLauncherMode = false;
                SelectedSmartLauncherItem = null;
                IsShowingAllProjects = false;
                currentDisplayedProjects.Clear();
                currentDisplayedProjects.Add(value.Project);

                // CurrentProjectを設定して、UIの更新をトリガー
                CurrentProject = value.Project;

                // グループ選択をリセット
                SelectedViewGroup = null;

                // 検索文字をクリア
                if (!string.IsNullOrEmpty(SearchText))
                {
                    SearchText = string.Empty;
                }
                
                // アイテム表示を即座に更新
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    ShowAllItems();
                    UpdateStatusText();
                }), System.Windows.Threading.DispatcherPriority.Render);
            }
            else if (value?.IsFolder == true)
            {
                // フォルダが選択された場合、子プロジェクトを集めて表示
                System.Diagnostics.Debug.WriteLine($"Switching to folder: {value.Name}");

                // SmartLauncher mode を無効にする
                IsSmartLauncherMode = false;
                SelectedSmartLauncherItem = null;

                // 検索文字をクリア
                if (!string.IsNullOrEmpty(SearchText))
                {
                    SearchText = string.Empty;
                }

                var childProjects = GetChildProjects(value);
                if (childProjects.Any())
                {
                    CurrentProject = null; // フォルダ選択時CurrentProjectはnull
                    IsShowingAllProjects = true;
                    currentDisplayedProjects.Clear();
                    currentDisplayedProjects.AddRange(childProjects);
                    
                    // フォルダ表示を即座に更新
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
                    {
                        ShowAllProjectsItems();
                    }), System.Windows.Threading.DispatcherPriority.Render);
                }
            }
            else if (value == null)
            {
                // 何も選択されていない場合は全プロジェクト表示
                System.Diagnostics.Debug.WriteLine("Switching to all projects");
                
                // SmartLauncher mode を無効にする
                IsSmartLauncherMode = false;
                SelectedSmartLauncherItem = null;
                
                ShowAllProjectsGlobal();
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

        public string AppVersion => VersionHelper.GetDisplayVersion();

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
                MessageBox.Show($"プロジェクトの読み込みに失敗しました: {ex.Message}", "エラー",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                CreateDefaultProject();
                BuildProjectHierarchy();
            }
        }

        private void CreateDefaultProject()
        {
            var defaultProject = new Project
            {
                Name = "デフォルト",
                Id = Guid.NewGuid().ToString(),
                OrderIndex = 0
            };
            defaultProject.Groups.Add(new ItemGroup { Name = "すべて", Id = "all", OrderIndex = 0 });
            defaultProject.Groups.Add(new ItemGroup { Name = "よく使う", Id = Guid.NewGuid().ToString(), OrderIndex = 1 });
            
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
                System.Diagnostics.Debug.WriteLine($"デフォルトプロジェクト保存エラー: {ex.Message}");
            }
        }

        private void UpdateProjectCompatibility(Project project)
        {
            // グループのOrderIndex初期化
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
                // 旧形式からの変換
                if (item.GroupIds == null || item.GroupIds.Count == 0)
                {
                    item.GroupIds = new List<string>();
                    if (!string.IsNullOrEmpty(item.GroupId))
                    {
                        item.GroupIds.Add(item.GroupId);
                    }
                }

                // ItemTypeとIconの設定 - LauncherItemの新しいメソッドを使用
                if (string.IsNullOrEmpty(item.ItemType) || string.IsNullOrEmpty(item.Icon))
                {
                    item.RefreshIconAndType();
                }

                // OrderIndexの初期化
                if (item.OrderIndex == 0)
                {
                    item.OrderIndex = project.Items.IndexOf(item);
                }

                // Categoryの初期化
                if (string.IsNullOrEmpty(item.Category))
                {
                    item.Category = "その他";
                }

                // Descriptionの初期化
                if (item.Description == null)
                {
                    item.Description = string.Empty;
                }

                // グループ名の更新
                UpdateItemGroupNames(item);
            }

            // アイテムをOrderIndexでソート
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

        private void UpdateItemGroupNamesForProject(LauncherItem item, Project project)
        {
            if (project != null && item.GroupIds != null)
            {
                var groupNames = project.Groups
                    .Where(g => item.GroupIds.Contains(g.Id) && g.Id != "all")
                    .Select(g => g.Name);
                item.GroupNames = string.Join(", ", groupNames);
            }
        }

        private void UpdateGroupListForProject(Project project)
        {
            if (project != null)
            {
                var sortedGroups = project.Groups.OrderBy(g => g.OrderIndex).ToList();
                project.Groups.Clear();
                foreach (var group in sortedGroups)
                {
                    if (group.Id == "all")
                    {
                        group.ItemCount = $"{project.Items.Count} アイテム";
                    }
                    else
                    {
                        var count = project.Items.Count(i => i.GroupIds != null && i.GroupIds.Contains(group.Id));
                        group.ItemCount = $"{count} アイテム";
                    }
                    project.Groups.Add(group);
                }
            }
        }

        private List<Project> GetAllProjectsFlattened()
        {
            var result = new List<Project>();
            foreach (var node in ProjectNodes)
            {
                CollectProjects(node, result);
            }
            return result;
        }

        private void CollectProjects(ProjectNode node, List<Project> result)
        {
            if (!node.IsFolder && node.Project != null)
            {
                result.Add(node.Project);
            }

            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    CollectProjects(child, result);
                }
            }
        }

        private void InitializeUI()
        {
            if (Projects.Count > 0)
            {
                // 最初の非フォルダープロジェクトを選択
                var firstProject = Projects.FirstOrDefault(p => !p.IsFolder);
                if (firstProject != null)
                {
                    CurrentProject = firstProject;
                    
                    // 対応するProjectNodeを選択
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
            System.Diagnostics.Debug.WriteLine($"UpdateAfterProjectChange: CurrentProject = {CurrentProject?.Name ?? "null"}, IsShowingAllProjects = {IsShowingAllProjects}");
            
            if (!IsShowingAllProjects && CurrentProject != null)
            {
                UpdateGroupList();
                SelectedViewGroup = null;
                
                // UI更新を確実に実行
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    ShowAllItems();
                    UpdateStatusText();
                }), System.Windows.Threading.DispatcherPriority.Render);
            }
            
            // プロジェクトノードの表示名を更新
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
                        group.ItemCount = $"{CurrentProject.Items.Count} アイテム";
                    }
                    else
                    {
                        var count = CurrentProject.Items.Count(i => i.GroupIds != null && i.GroupIds.Contains(group.Id));
                        group.ItemCount = $"{count} アイテム";
                    }
                    CurrentProject.Groups.Add(group);
                }
            }
        }

        private void ShowGroupItems(ItemGroup? group)
        {
            if (CurrentProject == null && !IsShowingAllProjects) return;
            
            IEnumerable<LauncherItem> items;
            
            if (IsShowingAllProjects)
            {
                // 全プロジェクト表示時はグループフィルタリングを行わない
                items = GetAllItemsFromDisplayedProjects();
            }
            else if (CurrentProject != null && group != null)
            {
                if (group.Id == "all")
                {
                    items = CurrentProject.Items;
                }
                else
                {
                    items = CurrentProject.Items.Where(i =>
                        i.GroupIds != null && i.GroupIds.Contains(group.Id));
                }
            }
            else
            {
                items = new List<LauncherItem>();
            }

            // 検索フィルタを適用
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
            if (IsShowingAllProjects)
            {
                ShowAllProjectsItems();
            }
            else if (CurrentProject != null)
            {
                // 各アイテムにプロジェクト情報を設定
                foreach (var item in CurrentProject.Items)
                {
                    item.ProjectName = CurrentProject.Name;
                    item.FolderPath = GetProjectFolderPath(CurrentProject);
                }

                var items = string.IsNullOrEmpty(SearchText)
                    ? CurrentProject.Items
                    : FilterItems(CurrentProject.Items, SearchText);

                var sortedItems = items.OrderBy(i => i.OrderIndex);
                UpdateDisplayedItems(sortedItems);
            }
        }

        private void ShowAllProjectsGlobal()
        {
            IsSmartLauncherMode = false;
            SelectedSmartLauncherItem = null;
            IsShowingAllProjects = true;
            CurrentProject = null;
            currentDisplayedProjects.Clear();
            currentDisplayedProjects.AddRange(Projects.Where(p => !p.IsFolder));
            ShowAllProjectsItems();
        }

        private void ShowAllProjectsItems()
        {
            var allItems = GetAllItemsFromDisplayedProjects();

            // 検索フィルタを適用
            if (!string.IsNullOrEmpty(SearchText))
            {
                allItems = FilterItems(allItems, SearchText);
            }

            var sortedItems = allItems.OrderBy(item => 
            {
                // プロジェクト名でソート、その後OrderIndexでソート
                var project = currentDisplayedProjects.FirstOrDefault(p => p.Items.Contains(item));
                return project?.Name ?? "";
            })
            .ThenBy(i => i.OrderIndex);

            UpdateDisplayedItems(sortedItems);
            UpdateStatusText();
        }

        private IEnumerable<LauncherItem> GetAllItemsFromDisplayedProjects()
        {
            var allItems = new List<LauncherItem>();
            foreach (var project in currentDisplayedProjects)
            {
                foreach (var item in project.Items)
                {
                    // アイテムにプロジェクト情報を設定（フォルダパス表示用）
                    item.ProjectName = project.Name;
                    item.FolderPath = GetProjectFolderPath(project);
                    allItems.Add(item);
                }
            }
            return allItems;
        }

        private string GetProjectFolderPath(Project project)
        {
            var pathParts = new List<string>();
            var currentProject = project;
            
            while (!string.IsNullOrEmpty(currentProject.ParentId))
            {
                var parentProject = Projects.FirstOrDefault(p => p.Id == currentProject.ParentId);
                if (parentProject != null)
                {
                    pathParts.Insert(0, parentProject.Name);
                    currentProject = parentProject;
                }
                else
                {
                    break;
                }
            }
            
            return pathParts.Count > 0 ? string.Join(" > ", pathParts) : "ルート";
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
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return items;
            }

            // スペースで分割してAND検索を実行
            var searchTerms = searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                       .Select(term => term.Trim().ToLower())
                                       .Where(term => !string.IsNullOrEmpty(term))
                                       .ToArray();

            if (searchTerms.Length == 0)
            {
                return items;
            }

            return items.Where(item =>
            {
                // 検索対象のテキストを結合
                var searchableText = $"{item.Name} {item.Path} {item.Category ?? ""} {item.Description ?? ""} {item.GroupNames ?? ""}".ToLower();
                
                // すべての検索語がテキストに含まれているかチェック（AND検索）
                return searchTerms.All(term => searchableText.Contains(term));
            });
        }

        private void ApplySearch()
        {
            if (IsSmartLauncherMode && SelectedSmartLauncherItem != null)
            {
                // スマートランチャーモード時の検索処理
                ApplySmartLauncherSearch();
            }
            else if (IsShowingAllProjects)
            {
                ShowAllProjectsItems();
            }
            else if (SelectedViewGroup != null)
            {
                ShowGroupItems(SelectedViewGroup);
            }
            else
            {
                ShowAllItems();
            }
        }

        private void ApplySmartLauncherSearch()
        {
            if (SelectedSmartLauncherItem == null) return;

            var items = SelectedSmartLauncherItem.Items;

            // 検索フィルタを適用
            if (!string.IsNullOrEmpty(SearchText))
            {
                items = FilterItems(items, SearchText).ToList();
            }

            UpdateDisplayedItems(items);
            
            // ステータステキストを更新（フィルタ後のアイテム数を表示）
            StatusText = $"{SelectedSmartLauncherItem.DisplayName}: {items.Count} items" +
                        (items.Count != SelectedSmartLauncherItem.ItemCount && !string.IsNullOrEmpty(SearchText) 
                         ? $" (全{SelectedSmartLauncherItem.ItemCount}項目から検索)" : "");
        }

        private void UpdateStatusText()
        {
            if (IsSmartLauncherMode && SelectedSmartLauncherItem != null)
            {
                StatusText = $"{SelectedSmartLauncherItem.DisplayName}: {SelectedSmartLauncherItem.ItemCount} items";
            }
            else if (IsShowingAllProjects)
            {
                var totalItems = currentDisplayedProjects.Sum(p => p.Items.Count);
                var projectCount = currentDisplayedProjects.Count;
                StatusText = $"{totalItems} 個のオブジェクト ({projectCount} プロジェクト)";
            }
            else if (CurrentProject != null)
            {
                StatusText = $"{CurrentProject.Items.Count} 個のオブジェクト";
            }
            else
            {
                StatusText = "プロジェクトを選択してください";
            }
        }

        private void SaveData()
        {
            try
            {
                // 現在のプロジェクトが存在する場合は保存
                if (CurrentProject != null)
                {
                    projectService.SaveProject(CurrentProject);
                }
                
                // 全プロジェクト表示時は、表示中のプロジェクトをすべて保存
                if (IsShowingAllProjects)
                {
                    foreach (var project in currentDisplayedProjects)
                    {
                        projectService.SaveProject(project);
                    }
                }
                
                // プロジェクト情報リストを更新
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
                MessageBox.Show($"保存に失敗しました: {ex.Message}", "エラー",
                    MessageBoxButton.OK, MessageBoxImage.Error);
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

        private List<Project> GetChildProjects(ProjectNode folderNode)
        {
            var childProjects = new List<Project>();
            CollectChildProjectsRecursive(folderNode, childProjects);
            return childProjects;
        }

        private void CollectChildProjectsRecursive(ProjectNode node, List<Project> projects)
        {
            foreach (var child in node.Children)
            {
                if (child.IsFolder)
                {
                    // 子フォルダの場合は再帰的に探索
                    CollectChildProjectsRecursive(child, projects);
                }
                else if (child.Project != null)
                {
                    // プロジェクトの場合はリストに追加
                    projects.Add(child.Project);
                }
            }
        }

        private void BuildProjectHierarchy(bool suppressAutoSelection = false)
        {
            // 現在選択されているプロジェクトのIDを保存
            var currentSelectedProjectId = SelectedProjectNode?.Id;

            ProjectNodes.Clear();
            var nodeMap = new Dictionary<string, ProjectNode>();

            // すべてのプロジェクトとフォルダーからノードを作成
            foreach (var project in Projects.OrderBy(p => p.OrderIndex))
            {
                var node = new ProjectNode
                {
                    Id = project.Id,
                    Name = project.Name,
                    IsFolder = project.IsFolder,
                    OrderIndex = project.OrderIndex,
                    ParentId = project.ParentId,
                    Project = project.IsFolder ? null : project // フォルダーの場合はnull、プロジェクトの場合は実際のProjectオブジェクトを設定
                };
                nodeMap[project.Id] = node;

                // デバッグ用ログを追加
                System.Diagnostics.Debug.WriteLine($"BuildProjectHierarchy: Created node {project.Name} (IsFolder: {project.IsFolder}, ItemCount: {(project.IsFolder ? 0 : project.Items?.Count ?? 0)})");
            }

            // 親子関係を構築
            foreach (var node in nodeMap.Values)
            {
                if (!string.IsNullOrEmpty(node.ParentId) && nodeMap.ContainsKey(node.ParentId))
                {
                    nodeMap[node.ParentId].AddChild(node);
                }
                else
                {
                    // 親がない場合はルートレベルに追加
                    ProjectNodes.Add(node);
                }
            }

            // ソート
            SortProjectNodes(ProjectNodes);

            // すべてのノードのDisplayNameを更新
            RefreshAllNodeDisplayNames(ProjectNodes);

            // 以前に選択されていたプロジェクトがあれば再選択(抑制フラグがfalseの場合のみ)
            if (!suppressAutoSelection && !string.IsNullOrEmpty(currentSelectedProjectId))
            {
                var nodeToSelect = FindProjectNode(currentSelectedProjectId);
                if (nodeToSelect != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Re-selecting project node: {nodeToSelect.Name}");
                    SelectedProjectNode = nodeToSelect;
                }
            }
        }

        private void RefreshAllNodeDisplayNames(ObservableCollection<ProjectNode> nodes)
        {
            foreach (var node in nodes)
            {
                // ノードの状態をデバッグ出力
                System.Diagnostics.Debug.WriteLine($"RefreshAllNodeDisplayNames: Node '{node.Name}', IsFolder: {node.IsFolder}, Project is null: {node.Project == null}");
                
                // DisplayNameとItemCountTextの更新を強制実行
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

        public void AddItemFromPath(string path)
        {
            if (CurrentProject == null && !IsShowingAllProjects)
            {
                MessageBox.Show("プロジェクトを選択してください", "エラー",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 全プロジェクト表示時は最初のプロジェクトに追加
            var targetProject = CurrentProject ?? currentDisplayedProjects.FirstOrDefault();
            if (targetProject == null)
            {
                MessageBox.Show("アイテムを追加するプロジェクトがありません", "エラー",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            try
            {
                // 既存のアイテムで同じパスがないかチェック
                if (targetProject.Items.Any(i => i.Path.Equals(path, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show($"「{path}」は既に追加されています。", "情報",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // ファイル名から名前を生成
                string name;
                string description;

                if (IsUrl(path))
                {
                    // URLの場合はドメイン名を名前として使用
                    try
                    {
                        var uri = new Uri(path);
                        name = uri.Host;
                        if (name.StartsWith("www."))
                        {
                            name = name.Substring(4);
                        }
                        description = $"Webサイト: {path}";
                    }
                    catch
                    {
                        name = "Webサイト";
                        description = $"Webサイト: {path}";
                    }
                }
                else if (System.IO.Directory.Exists(path))
                {
                    name = System.IO.Path.GetFileName(path.TrimEnd('\\', '/'));
                    description = $"フォルダ: {path}";
                }
                else if (System.IO.File.Exists(path))
                {
                    name = System.IO.Path.GetFileNameWithoutExtension(path);
                    description = $"ファイル: {System.IO.Path.GetFileName(path)}";
                }
                else
                {
                    name = System.IO.Path.GetFileName(path);
                    description = $"ドラッグ&ドロップで追加: {name}";
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    name = path;
                }

                // 分類を自動判定
                string category = DetermineCategory(path);

                // 新しいアイテムを作成
                var newItem = new LauncherItem
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = name,
                    Path = path,
                    Description = description,
                    Category = category,
                    GroupIds = new List<string>(), // デフォルトグループなし
                    OrderIndex = targetProject.Items.Count,
                    LastAccessed = DateTime.MinValue, // 新しいアイテムなので未アクセス状態
                    ProjectName = targetProject.Name,
                    FolderPath = GetProjectFolderPath(targetProject)
                };

                // アイコンとタイプを設定
                newItem.RefreshIconAndType();

                // グループ名を更新
                if (CurrentProject != null)
                {
                    UpdateItemGroupNames(newItem);
                }

                // プロジェクトに追加
                targetProject.Items.Add(newItem);
                
                if (CurrentProject != null)
                {
                    UpdateGroupList();
                }

                // 表示を更新
                if (IsShowingAllProjects)
                {
                    ShowAllProjectsItems();
                }
                else if (SelectedViewGroup != null)
                {
                    ShowGroupItems(SelectedViewGroup);
                }
                else
                {
                    ShowAllItems();
                }

                // プロジェクトノードの表示名を更新
                RefreshProjectNodeDisplayNames();

                // データを保存
                SaveData();

                // SmartLauncher項目を更新
                LoadSmartLauncherItems();

                // 成功メッセージ（オプション）
                if (IsShowingAllProjects)
                {
                    var totalItems = currentDisplayedProjects.Sum(p => p.Items.Count);
                    StatusText = $"「{name}」を{targetProject.Name}に追加しました ({totalItems} 個のオブジェクト)";
                }
                else
                {
                    StatusText = $"「{name}」を追加しました ({targetProject.Items.Count} 個のオブジェクト)";
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"アイテムの追加に失敗しました: {ex.Message}", ex);
            }
        }

        public void ShowAddItemDialogWithPath(string path)
        {
            if (CurrentProject == null && !IsShowingAllProjects)
            {
                MessageBox.Show("プロジェクトを選択してください", "エラー",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 全プロジェクト表示時は最初のプロジェクトに追加
            var targetProject = CurrentProject ?? currentDisplayedProjects.FirstOrDefault();
            if (targetProject == null)
            {
                MessageBox.Show("アイテムを追加するプロジェクトがありません", "エラー",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            try
            {
                // 既存のアイテムで同じパスがないかチェック
                if (targetProject.Items.Any(i => i.Path.Equals(path, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show($"「{path}」は既に追加されています。", "情報",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // AddItemDialogに事前情報を設定して表示
                var dialog = new AddItemDialog(targetProject.Groups.ToList());
                
                // ファイル名から名前を生成
                string name;
                string description;
                
                if (IsUrl(path))
                {
                    // URLの場合はドメイン名を名前として使用
                    try
                    {
                        var uri = new Uri(path);
                        name = uri.Host;
                        if (name.StartsWith("www."))
                        {
                            name = name.Substring(4);
                        }
                        description = $"Webサイト: {path}";
                    }
                    catch
                    {
                        name = "Webサイト";
                        description = $"Webサイト: {path}";
                    }
                }
                else if (System.IO.Directory.Exists(path))
                {
                    name = System.IO.Path.GetFileName(path.TrimEnd('\\', '/'));
                    description = $"フォルダ: {path}";
                }
                else if (System.IO.File.Exists(path))
                {
                    name = System.IO.Path.GetFileNameWithoutExtension(path);
                    description = $"ファイル: {System.IO.Path.GetFileName(path)}";
                }
                else
                {
                    name = System.IO.Path.GetFileName(path);
                    description = $"ドラッグ&ドロップで追加: {name}";
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    name = path;
                }

                // 分類を自動判定
                string category = DetermineCategory(path);

                // ダイアログに初期値を設定
                dialog.SetInitialValues(name, path, category, description, false);

                if (dialog.ShowDialog() == true && dialog.Result != null)
                {
                    var newItem = dialog.Result;
                    newItem.OrderIndex = targetProject.Items.Count;
                    newItem.ProjectName = targetProject.Name;
                    newItem.FolderPath = GetProjectFolderPath(targetProject);
                    
                    // アイコンとタイプを設定
                    newItem.RefreshIconAndType();
                    
                    // グループ名を更新
                    if (CurrentProject != null)
                    {
                        UpdateItemGroupNames(newItem);
                    }
                    
                    targetProject.Items.Add(newItem);
                    
                    if (CurrentProject != null)
                    {
                        UpdateGroupList();
                    }
                    
                    // 表示を更新
                    if (IsShowingAllProjects)
                    {
                        ShowAllProjectsItems();
                    }
                    else if (SelectedViewGroup != null)
                    {
                        ShowGroupItems(SelectedViewGroup);
                    }
                    else
                    {
                        ShowAllItems();
                    }
                    
                    SaveData();
                    
                    // 成功メッセージ
                    if (IsShowingAllProjects)
                    {
                        var totalItems = currentDisplayedProjects.Sum(p => p.Items.Count);
                        StatusText = $"「{newItem.Name}」を{targetProject.Name}に追加しました ({totalItems} 個のオブジェクト)";
                    }
                    else
                    {
                        StatusText = $"「{newItem.Name}」を追加しました ({targetProject.Items.Count} 個のオブジェクト)";
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"アイテムの追加に失敗しました: {ex.Message}", ex);
            }
        }

        private void AddItem(object? parameter)
        {
            if (CurrentProject == null && !IsShowingAllProjects)
            {
                MessageBox.Show("プロジェクトを選択してください", "エラー",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 全プロジェクト表示時は最初のプロジェクトに追加
            var targetProject = CurrentProject ?? currentDisplayedProjects.FirstOrDefault();
            if (targetProject == null)
            {
                MessageBox.Show("アイテムを追加するプロジェクトがありません", "エラー",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new AddItemDialog(targetProject.Groups.ToList());

            // パラメータとしてクリップボードの値が渡された場合は、パスに設定
            if (parameter is string clipboardPath && !string.IsNullOrWhiteSpace(clipboardPath))
            {
                dialog.SetInitialValues("", clipboardPath, "", "");
            }

            if (dialog.ShowDialog() == true && dialog.Result != null)
            {
                var newItem = dialog.Result;
                newItem.OrderIndex = targetProject.Items.Count;
                newItem.ProjectName = targetProject.Name;
                newItem.FolderPath = GetProjectFolderPath(targetProject);
                
                // 新しいメソッドを使用してアイコンとタイプを設定
                newItem.RefreshIconAndType();
                
                // グループ名を更新
                if (CurrentProject != null)
                {
                    UpdateItemGroupNames(newItem);
                }
                
                targetProject.Items.Add(newItem);
                
                if (CurrentProject != null)
                {
                    UpdateGroupList();
                }
                
                // 表示を更新
                if (IsShowingAllProjects)
                {
                    ShowAllProjectsItems();
                }
                else if (SelectedViewGroup != null)
                {
                    ShowGroupItems(SelectedViewGroup);
                }
                else
                {
                    ShowAllItems();
                }
                
                // プロジェクトノードの表示名を更新
                RefreshProjectNodeDisplayNames();
                
                SaveData();
                
                // SmartLauncher項目を更新
                LoadSmartLauncherItems();
            }
        }

        private void AddGroup(object? parameter)
        {
            if (CurrentProject == null)
            {
                MessageBox.Show("プロジェクトを選択してください", "エラー",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new TextInputDialog("新規グループ", "グループ名を入力してください:");
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

        private void EditGroup(object? parameter)
        {
            // パラメータからグループを取得するか、現在選択されているグループを使用
            var groupToEdit = parameter as ItemGroup ?? SelectedViewGroup;

            if (CurrentProject == null || groupToEdit == null)
            {
                MessageBox.Show("編集するグループを選択してください", "エラー",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 「すべて」グループは編集不可
            if (groupToEdit.Id == "all")
            {
                MessageBox.Show("「すべて」グループは編集できません", "エラー",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new TextInputDialog("グループ編集", "グループ名を入力してください:", groupToEdit.Name);
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputText))
            {
                var currentGroup = CurrentProject.Groups.FirstOrDefault(g => g.Id == groupToEdit.Id);
                if (currentGroup != null)
                {
                    currentGroup.Name = dialog.InputText;
                    UpdateGroupList();
                    
                    // 関連するアイテムのGroupNamesを更新
                    foreach (var item in CurrentProject.Items.Where(i => i.GroupIds?.Contains(currentGroup.Id) == true))
                    {
                        UpdateItemGroupNames(item);
                    }
                    
                    // 表示を更新
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

        private bool CanEditGroup(object? parameter)
        {
            var groupToCheck = parameter as ItemGroup ?? SelectedViewGroup;
            return CurrentProject != null && groupToCheck != null && groupToCheck.Id != "all";
        }

        private void DeleteGroup(object? parameter)
        {
            // パラメータからグループを取得するか、現在選択されているグループを使用
            var groupToDelete = parameter as ItemGroup ?? SelectedViewGroup;

            if (CurrentProject == null || groupToDelete == null)
            {
                MessageBox.Show("削除するグループを選択してください", "エラー",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 「すべて」グループは削除不可
            if (groupToDelete.Id == "all")
            {
                MessageBox.Show("「すべて」グループは削除できません", "エラー",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var currentGroup = CurrentProject.Groups.FirstOrDefault(g => g.Id == groupToDelete.Id);
            if (currentGroup != null)
            {
                var result = MessageBox.Show($"グループ「{currentGroup.Name}」を削除しますか？\n\n※グループに属するアイテムは削除されませんが、このグループへの所属が解除されます。", 
                    "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    // このグループに属するアイテムからグループIDを削除
                    foreach (var item in CurrentProject.Items.Where(i => i.GroupIds?.Contains(currentGroup.Id) == true))
                    {
                        item.GroupIds?.Remove(currentGroup.Id);
                        UpdateItemGroupNames(item);
                    }
                    
                    // グループを削除
                    CurrentProject.Groups.Remove(currentGroup);
                    UpdateGroupList();
                    
                    // 選択をクリア
                    SelectedViewGroup = null;
                    ShowAllItems();
                    
                    SaveData();
                }
            }
        }

        private bool CanDeleteGroup(object? parameter)
        {
            var groupToCheck = parameter as ItemGroup ?? SelectedViewGroup;
            return CurrentProject != null && groupToCheck != null && groupToCheck.Id != "all";
        }

        // Command implementations
        private void NewProject(object? parameter)
        {
            DebugLogger.Log("=== NewProject command triggered ===");
            DebugLogger.Log($"Parameter: {parameter}");
            DebugLogger.Log($"Parameter type: {parameter?.GetType().Name}");
            DebugLogger.Log($"SelectedProjectNode: {SelectedProjectNode?.Name ?? "null"}");

            try
            {
                // パラメータから ProjectNode を取得、なければ SelectedProjectNode を使用
                var targetNode = parameter as ProjectNode ?? SelectedProjectNode;
                DebugLogger.Log($"Target node: {targetNode?.Name ?? "null"}");

                // 利用可能なフォルダノードを取得
                var availableFolders = new List<ProjectNode>();
                CollectFoldersRecursive(ProjectNodes, availableFolders);

                var dialog = new NewProjectDialog(availableFolders);
                
                // Owner設定を安全に実行
                try
                {
                    dialog.Owner = Application.Current.MainWindow;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to set dialog owner: {ex.Message}");
                }
                
                if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.ProjectName))
                {
                    var parentId = dialog.SelectedFolder?.Id; // nullの場合はルートレベル
                    
                    var newProject = new Project
                    {
                        Name = dialog.ProjectName,
                        Id = Guid.NewGuid().ToString(),
                        OrderIndex = Projects.Count,
                        ParentId = parentId,
                        IsFolder = false
                    };
                    newProject.Groups.Add(new ItemGroup { Name = "すべて", Id = "all", OrderIndex = 0 });
                    newProject.Groups.Add(new ItemGroup { Name = "よく使う", Id = Guid.NewGuid().ToString(), OrderIndex = 1 });
                    
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
                        
                        // 階層構造を再構築
                        BuildProjectHierarchy();
                        
                        // 新しく作成されたプロジェクトを選択
                        var newNode = FindProjectNode(newProject.Id);
                        if (newNode != null)
                        {
                            // プロジェクトノードを選択
                            SelectedProjectNode = newNode;
                            
                            // プロジェクトが作成されたフォルダを展開
                            if (dialog.SelectedFolder != null)
                            {
                                var parentNode = FindProjectNode(dialog.SelectedFolder.Id);
                                if (parentNode != null)
                                {
                                    parentNode.IsExpanded = true;
                                }
                            }
                            
                            // UI表示を強制的に更新
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
                            {
                                CurrentProject = newProject;
                                UpdateGroupList();
                                SelectedViewGroup = null;
                                ShowAllItems();
                                UpdateStatusText();
                                RefreshProjectNodeDisplayNames();
                            }), System.Windows.Threading.DispatcherPriority.Render);
                        }
                        
                        // SmartLauncher項目を更新
                        LoadSmartLauncherItems();
                        
                        // 成功メッセージ
                        var folderName = dialog.SelectedFolder?.Name ?? "Root";
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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NewProject error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Failed to create new project: {ex.Message}\n\nPlease try again.", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NewFolder(object? parameter)
        {
            DebugLogger.Log("=== NewFolder command triggered ===");
            DebugLogger.Log($"Parameter: {parameter}");
            DebugLogger.Log($"Parameter type: {parameter?.GetType().Name}");
            DebugLogger.Log($"SelectedProjectNode: {SelectedProjectNode?.Name ?? "null"}");

            // パラメータから ProjectNode を取得、なければ SelectedProjectNode を使用
            var targetNode = parameter as ProjectNode ?? SelectedProjectNode;
            DebugLogger.Log($"Target node: {targetNode?.Name ?? "null"}");

            var dialog = new TextInputDialog("New Folder", "Enter folder name:");
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputText))
            {
                var parentId = targetNode?.IsFolder == true ? targetNode.Id : targetNode?.ParentId;
                
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
                    
                    // 階層構造を再構築
                    BuildProjectHierarchy();
                    
                    // 新しく作成されたフォルダーを選択
                    var newNode = FindProjectNode(newFolder.Id);
                    if (newNode != null)
                    {
                        SelectedProjectNode = newNode;
                        newNode.IsExpanded = true;
                        
                        // UI表示を強制的に更新
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
                        {
                            RefreshProjectNodeDisplayNames();
                            UpdateStatusText();
                        }), System.Windows.Threading.DispatcherPriority.Render);
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

        private void LaunchItem(object? parameter)
        {
            var item = parameter as LauncherItem ?? SelectedItem;
            if (item != null)
            {
                try
                {
                    // アイテムの所属プロジェクトを特定
                    var itemProject = FindProjectContainingItem(item);
                    
                    // Record the access before launching
                    RecordItemAccess(item);
                    
                    launcherService.LaunchItem(item);
                    
                    // 最終アクセス時刻を更新した後、該当プロジェクトを保存
                    if (itemProject != null)
                    {
                        projectService.SaveProject(itemProject);
                    }
                    else
                    {
                        // フォールバック：通常の保存メソッドを使用
                        SaveData();
                    }
                    
                    // Refresh smart launcher items after launching
                    LoadSmartLauncherItems();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanLaunchItem(object? parameter)
        {
            return parameter is LauncherItem || SelectedItem != null;
        }

        private void LaunchGroup(object? parameter)
        {
            var group = parameter as ItemGroup ?? SelectedViewGroup;
            if (group == null || CurrentProject == null)
            {
                MessageBox.Show("グループを選択してください", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 「すべて」グループが選択された場合
            IEnumerable<LauncherItem> itemsToLaunch;
            if (group.Id == "all")
            {
                itemsToLaunch = CurrentProject.Items;
            }
            else
            {
                itemsToLaunch = CurrentProject.Items.Where(i => i.GroupIds != null && i.GroupIds.Contains(group.Id));
            }

            var itemList = itemsToLaunch.ToList();
            
            if (itemList.Count == 0)
            {
                MessageBox.Show($"グループ「{group.Name}」には起動可能なアイテムがありません", "情報", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 確認メッセージを表示
            var result = MessageBox.Show($"グループ「{group.Name}」の{itemList.Count}個のアイテムを一括起動しますか？", 
                "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                int successCount = 0;
                int errorCount = 0;
                var errors = new List<string>();

                foreach (var item in itemList)
                {
                    try
                    {
                        // アイテムの所属プロジェクトを特定
                        var itemProject = FindProjectContainingItem(item);
                        
                        // Record the access before launching
                        RecordItemAccess(item);
                        
                        launcherService.LaunchItem(item);
                        
                        // 最終アクセス時刻を更新した後、該当プロジェクトを保存
                        if (itemProject != null)
                        {
                            projectService.SaveProject(itemProject);
                        }
                        
                        successCount++;
                        
                        // 各アイテムの起動間隔を設ける（システムに負荷をかけすぎないため）
                        System.Threading.Thread.Sleep(100);
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        errors.Add($"「{item.Name}」: {ex.Message}");
                    }
                }

                // 結果を表示
                if (errorCount == 0)
                {
                    StatusText = $"グループ「{group.Name}」の{successCount}個のアイテムを一括起動しました";
                }
                else
                {
                    var message = $"グループ一括起動完了\n成功: {successCount}個\n失敗: {errorCount}個";
                    if (errors.Count > 0 && errors.Count <= 5)
                    {
                        message += "\n\nエラー詳細:\n" + string.Join("\n", errors);
                    }
                    else if (errors.Count > 5)
                    {
                        message += "\n\nエラー詳細:\n" + string.Join("\n", errors.Take(5)) + $"\n...他{errors.Count - 5}件";
                    }
                    
                    MessageBox.Show(message, "一括起動結果", MessageBoxButton.OK, MessageBoxImage.Information);
                    StatusText = $"グループ「{group.Name}」の一括起動完了（成功: {successCount}個、失敗: {errorCount}個）";
                }

                // SmartLauncher項目を更新
                LoadSmartLauncherItems();
            }
        }

        private bool CanLaunchGroup(object? parameter)
        {
            var group = parameter as ItemGroup ?? SelectedViewGroup;
            return group != null && CurrentProject != null;
        }

        private Project? FindProjectContainingItem(LauncherItem item)
        {
            foreach (var project in Projects)
            {
                if (project.Items.Contains(item))
                {
                    return project;
                }
            }
            return null;
        }

        private void EditItem(object? parameter)
        {
            var item = parameter as LauncherItem ?? SelectedItem;
            if (item != null && CurrentProject != null)
            {
                // Get all projects for the dialog
                var allProjects = GetAllProjectsFlattened();
                var dialog = new EditItemDialog(item, CurrentProject.Groups.ToList(), allProjects, CurrentProject);
                if (dialog.ShowDialog() == true && dialog.Result != null)
                {
                    var editedItem = dialog.Result;

                    // 新しいメソッドを使用してアイコンとタイプを設定
                    editedItem.RefreshIconAndType();

                    // Check if project was changed
                    if (dialog.SelectedProject != null && dialog.SelectedProject.Id != CurrentProject.Id)
                    {
                        // Remove from current project
                        CurrentProject.Items.Remove(item);

                        // Add to new project
                        dialog.SelectedProject.Items.Add(editedItem);

                        // Update group names for the new project
                        UpdateItemGroupNamesForProject(editedItem, dialog.SelectedProject);

                        // Update group list for new project
                        UpdateGroupListForProject(dialog.SelectedProject);
                    }
                    else
                    {
                        // Same project - just update the item
                        // グループ名を更新
                        UpdateItemGroupNames(editedItem);

                        // 元のアイテムと置き換え
                        var index = CurrentProject.Items.IndexOf(item);
                        if (index >= 0)
                        {
                            CurrentProject.Items[index] = editedItem;
                        }

                        UpdateGroupList();
                    }

                    // 表示を更新
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
                var result = MessageBox.Show($"「{item.Name}」を削除しますか？", "確認",
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
                    
                    // プロジェクトノードの表示名を更新
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
                    // ソートをクリアしてからアイテムを移動
                    ClearListViewSort();
                    
                    CurrentProject.Items.Move(index, index - 1);
                    UpdateItemOrderIndices();
                    SaveData();
                    RefreshItemsView();
                    
                    // 選択状態を維持
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
                    // ソートをクリアしてからアイテムを移動
                    ClearListViewSort();
                    
                    CurrentProject.Items.Move(index, index + 1);
                    UpdateItemOrderIndices();
                    SaveData();
                    RefreshItemsView();
                    
                    // 選択状態を維持
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
            // メインListViewのソートをクリア
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
                MessageBox.Show("エクスポートするプロジェクトがありません", "エラー",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "プロジェクトをエクスポート",
                    Filter = "JSONファイル (*.json)|*.json|すべてのファイル (*.*)|*.*",
                    FileName = $"{CurrentProject.Name}.json"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    projectService.ExportProject(CurrentProject, saveFileDialog.FileName);
                    MessageBox.Show($"プロジェクト「{CurrentProject.Name}」をエクスポートしました", "完了",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"エクスポートに失敗しました: {ex.Message}", "エラー",
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
                    Title = "プロジェクトをインポート",
                    Filter = "JSONファイル (*.json)|*.json|すべてのファイル (*.*)|*.*"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var importedProject = projectService.ImportProject(openFileDialog.FileName);
                    if (importedProject != null)
                    {
                        // デバッグ情報を出力
                        System.Diagnostics.Debug.WriteLine($"ImportProject: Imported project '{importedProject.Name}' with {importedProject.Items?.Count ?? 0} items");
                        
                        // IDの重複を避ける
                        importedProject.Id = Guid.NewGuid().ToString();
                        importedProject.OrderIndex = Projects.Count;
                        
                        // 互換性チェック
                        UpdateProjectCompatibility(importedProject);
                        
                        // デバッグ: 互換性チェック後のアイテム数を確認
                        System.Diagnostics.Debug.WriteLine($"ImportProject: After compatibility check - project '{importedProject.Name}' has {importedProject.Items?.Count ?? 0} items");
                        
                        // プロジェクトコレクションに追加
                        Projects.Add(importedProject);
                        
                        // プロジェクト情報リストを保存
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
                        
                        // 階層構造を再構築
                        BuildProjectHierarchy();
                        
                        // インポートしたプロジェクトを即座に選択表示
                        var importedNode = FindProjectNode(importedProject.Id);
                        if (importedNode != null)
                        {
                            // デバッグ: ノードの状態を確認
                            System.Diagnostics.Debug.WriteLine($"ImportProject: Found imported node '{importedNode.Name}', Project is null: {importedNode.Project == null}");
                            if (importedNode.Project != null)
                            {
                                System.Diagnostics.Debug.WriteLine($"ImportProject: Node's project has {importedNode.Project.Items?.Count ?? 0} items");
                            }
                            
                            // プロジェクトノードを選択
                            SelectedProjectNode = importedNode;
                            
                            // プロジェクトを現在のプロジェクトとして設定
                            CurrentProject = importedProject;
                
                            // UI表示を強制的に更新
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
                            {
                                // グループリストを更新
                                UpdateGroupList();
                                
                                // アイテム表示を更新  
                                SelectedViewGroup = null;
                                ShowAllItems();
                                UpdateStatusText();
                                
                                // プロジェクトノードの表示名を更新
                                RefreshProjectNodeDisplayNames();
                                
                            }), System.Windows.Threading.DispatcherPriority.Render);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"ImportProject: Could not find imported node for project '{importedProject.Name}'");
                        }
                        
                        // SmartLauncher項目を更新
                        LoadSmartLauncherItems();
                        
                        MessageBox.Show($"プロジェクト「{importedProject.Name}」をインポートしました", "完了",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"インポートに失敗しました: {ex.Message}", "エラー",
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

            var itemType = SelectedProjectNode.IsFolder ? "フォルダー" : "プロジェクト";
            var result = MessageBox.Show($"{itemType}「{SelectedProjectNode.Name}」を削除しますか？\n\n※子要素も一緒に削除されます。", 
                "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // 削除対象のIDリストを収集（子要素を含む）
                    var idsToDelete = new List<string>();
                    CollectNodeIds(SelectedProjectNode, idsToDelete);

                    // プロジェクトコレクションから削除
                    var projectsToRemove = Projects.Where(p => idsToDelete.Contains(p.Id)).ToList();
                    foreach (var project in projectsToRemove)
                    {
                        Projects.Remove(project);
                        
                        // ファイルからも削除
                        try
                        {
                            projectService.DeleteProject(project.Id);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"プロジェクトファイル削除エラー: {ex.Message}");
                        }
                    }

                    // プロジェクト情報リストを更新
                    var projectInfoList = Projects.Select(p => new ProjectInfo
                    {
                        Id = p.Id,
                        Name = p.Name,
                        OrderIndex = p.OrderIndex,
                        ParentId = p.ParentId,
                        IsFolder = p.IsFolder
                    }).ToList();
                    
                    projectService.SaveProjectList(projectInfoList);

                    // 階層構造を再構築
                    BuildProjectHierarchy();

                    // 残りのプロジェクトがある場合は最初のプロジェクトを選択
                    var firstProject = ProjectNodes.FirstOrDefault(n => !n.IsFolder);
                    if (firstProject == null)
                    {
                        // フォルダー内の最初のプロジェクトを探す
                        firstProject = FindFirstProjectRecursive(ProjectNodes);
                    }
                    
                    if (firstProject != null)
                    {
                        SelectedProjectNode = firstProject;
                    }
                    else
                    {
                        // プロジェクトが全くない場合はデフォルトプロジェクトを作成
                        CurrentProject = null;
                        CreateDefaultProject();
                        BuildProjectHierarchy();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"削除に失敗しました: {ex.Message}", "エラー",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanNewProject(object? parameter)
        {
            DebugLogger.Log($"CanNewProject called - Parameter: {parameter}, Result: true");
            // 新規プロジェクトは常に作成可能
            return true;
        }

        private bool CanNewFolder(object? parameter)
        {
            DebugLogger.Log($"CanNewFolder called - Parameter: {parameter}, Result: true");
            // 新規フォルダは常に作成可能
            return true;
        }

        private bool CanDeleteProjectOrFolder(object? parameter)
        {
            return SelectedProjectNode != null;
        }

        private void EditProjectOrFolder(object? parameter)
        {
            if (SelectedProjectNode == null) return;

            try
            {
                // 利用可能なフォルダノードを取得
                var availableFolders = new List<ProjectNode>();
                CollectFoldersRecursive(ProjectNodes, availableFolders);

                var dialog = new EditProjectDialog(SelectedProjectNode, availableFolders);
                dialog.Owner = Application.Current.MainWindow;
                
                if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.ProjectName))
                {
                    var selectedParentId = dialog.SelectedFolder?.Id;
                    
                    // プロジェクトまたはフォルダーを更新
                    var project = Projects.FirstOrDefault(p => p.Id == SelectedProjectNode.Id);
                    if (project != null)
                    {
                        var oldName = project.Name;
                        project.Name = dialog.ProjectName;
                        project.ParentId = selectedParentId;

                        System.Diagnostics.Debug.WriteLine($"EditProject: Before save - Name='{project.Name}', ParentId='{project.ParentId}', IsFolder={project.IsFolder}");
                        projectService.SaveProject(project);
                        System.Diagnostics.Debug.WriteLine($"EditProject: After save - Name='{project.Name}', ParentId='{project.ParentId}', IsFolder={project.IsFolder}");
                        
                        var projectInfoList = Projects.Select(p => new ProjectInfo
                        {
                            Id = p.Id,
                            Name = p.Name,
                            OrderIndex = p.OrderIndex,
                            ParentId = p.ParentId,
                            IsFolder = p.IsFolder
                        }).ToList();
                        
                        projectService.SaveProjectList(projectInfoList);

                        // 階層構造を再構築(再選択は手動で行うため、自動再選択を抑制)
                        BuildProjectHierarchy(suppressAutoSelection: true);

                        // 編集されたプロジェクトを再選択
                        var editedNode = FindProjectNode(project.Id);
                        if (editedNode != null)
                        {
                            // 移動先フォルダーを展開
                            if (selectedParentId != null)
                            {
                                var parentNode = FindProjectNode(selectedParentId);
                                if (parentNode != null)
                                {
                                    parentNode.IsExpanded = true;
                                }
                            }

                            // SelectedProjectNodeを設定することで、HandleProjectNodeSelectionChangeが呼ばれ、
                            // CurrentProjectが自動的に設定され、UIが更新される
                            SelectedProjectNode = editedNode;

                            // UI表示を強制的に更新
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
                            {
                                RefreshProjectNodeDisplayNames();
                                UpdateStatusText();
                            }), System.Windows.Threading.DispatcherPriority.Render);
                        }
                        
                        var itemType = project.IsFolder ? "フォルダー" : "プロジェクト";
                        StatusText = $"項目「{oldName}」が「{project.Name}」に更新されました";
                        MessageBox.Show($"{itemType}が正常に更新されました。", "更新完了", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"編集に失敗しました: {ex.Message}", "エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanEditProjectOrFolder(object? parameter)
        {
            return SelectedProjectNode != null;
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

        // 不足しているメソッドを追加
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

        private void MoveToFolder(object? parameter)
        {
            if (SelectedProjectNode == null) return;

            // 利用可能なフォルダーを取得（移動対象自身と子フォルダーは除外）
            var availableFolders = GetAvailableFolders(SelectedProjectNode);
            
            if (availableFolders.Count == 0)
            {
                MessageBox.Show("移動先として利用可能なフォルダーがありません。先にフォルダーを作成してください。", 
                    "フォルダーなし", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            var dialog = new FolderSelectionDialog(availableFolders, 
                $"'{SelectedProjectNode.Name}'の移動先フォルダーを選択してください");
            dialog.Owner = Application.Current.MainWindow;
            
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var targetFolder = dialog.SelectedFolder;
                    var targetParentId = targetFolder?.Id; // nullの場合はルートレベル

                    // プロジェクトの親IDを更新
                    var project = Projects.FirstOrDefault(p => p.Id == SelectedProjectNode.Id);
                    if (project != null)
                    {
                        project.ParentId = targetParentId;
                        
                        // データを保存
                        SaveData();
                        
                        // 階層構造を再構築
                        BuildProjectHierarchy();
                        
                        // 移動したプロジェクトを再選択
                        var movedNode = FindProjectNode(SelectedProjectNode.Id);
                        if (movedNode != null)
                        {
                            SelectedProjectNode = movedNode;
                            
                            // 移動先フォルダーを展開
                            if (targetFolder != null)
                            {
                                var targetNode = FindProjectNode(targetFolder.Id);
                                if (targetNode != null)
                                {
                                    targetNode.IsExpanded = true;
                                }
                            }
                        }
                        
                        MessageBox.Show("移動が完了しました。", "移動完了", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"移動に失敗しました: {ex.Message}", "エラー",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanMoveProjectToFolder(object? parameter)
        {
            return SelectedProjectNode != null;
        }

        private List<ProjectNode> GetAvailableFolders(ProjectNode excludeNode)
        {
            var allFolders = new List<ProjectNode>();
            CollectFoldersRecursive(ProjectNodes, allFolders);
            
            // 移動対象自身と子フォルダーを除外
            var excludeIds = new HashSet<string>();
            CollectNodeIdsRecursive(excludeNode, excludeIds);
            
            var availableFolders = allFolders.Where(f => !excludeIds.Contains(f.Id)).ToList();
            
            return availableFolders;
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
            var result = MessageBox.Show("デバッグ用のテストフォルダーを作成しますか？\n\n以下が作成されます：\n- Work Folder\n- Personal Folder\n- Development Folder", 
                "テストフォルダー作成", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
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
                    
                    // フォルダーも保存
                    foreach (var project in Projects.Where(p => p.IsFolder))
                    {
                        projectService.SaveProject(project);
                    }
                    
                    BuildProjectHierarchy();
                    
                    MessageBox.Show("テストフォルダーが正常に作成されました！", "成功",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"テストフォルダーの作成に失敗しました: {ex.Message}", "エラー",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
                if (string.IsNullOrEmpty(path))
                    return "その他";

                if (path.StartsWith("http://") || path.StartsWith("https://") || path.StartsWith("www."))
                {
                    var uri = new Uri(path.StartsWith("www.") ? "http://" + path : path);
                    var host = uri.Host.ToLower();
                    var lowerPath = path.ToLower();
                    
                    if (host.Contains("github.com"))
                        return "GitHubURL";
                    else if (host.Contains("gitlab.com") || lowerPath.Contains("gitlab"))
                        return "GitLabURL";
                    else if (lowerPath.Contains("redmine"))
                        return "RedmineURL";
                    else if (host.Contains("drive.google.com") || host.Contains("docs.google.com"))
                        return "Googleドライブ";
                    else if (host.Contains("teams.microsoft.com") || host.Contains("teams.live.com"))
                        return "MicrosoftTeams";
                    else if (host.Contains("sharepoint.com") || host.Contains(".sharepoint.com") || 
                             host.EndsWith("sharepoint.com") || host.Contains("office365.sharepoint.com"))
                        return "SharePoint";
                    else if (host.Contains("outlook.office365.com") || host.Contains("outlook.office.com") ||
                             host.Contains("onedrive.live.com") || host.Contains("1drv.ms"))
                        return "OneDrive";
                    else
                        return "Webサイト";
                }

                if (System.IO.Directory.Exists(path))
                {
                    return "フォルダ";
                }

                if (System.IO.File.Exists(path))
                {
                    var ext = System.IO.Path.GetExtension(path).ToLower();
                    return ext switch
                    {
                        ".exe" or ".msi" or ".bat" or ".cmd" => "アプリケーション",
                        ".txt" or ".rtf" => "ドキュメント",
                        ".doc" or ".docx" => "Word",
                        ".xls" or ".xlsx" => "Excel",
                        ".ppt" or ".pptx" => "PowerPoint",
                        ".pdf" => "PDF",
                        ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".svg" or ".webp" => "画像",
                        ".mp3" or ".wav" or ".wma" or ".flac" or ".aac" or ".ogg" => "音楽",
                        ".mp4" or ".avi" or ".mkv" or ".wmv" or ".mov" or ".flv" or ".webm" => "動画",
                        ".zip" or ".rar" or ".7z" or ".tar" or ".gz" or ".bz2" => "アーカイブ",
                        ".lnk" => "ショートカット",
                        ".py" or ".js" or ".html" or ".css" or ".cpp" or ".c" or ".cs" or ".java" or ".php" => "プログラム",
                        _ => "ファイル"
                    };
                }

                return "コマンド";
            }
            catch (Exception)
            {
                return "その他";
            }
        }

        private void OpenWithVSCode(object? parameter)
        {
            var item = parameter as LauncherItem ?? SelectedItem;
            if (item != null)
            {
                try
                {
                    string path = item.Path;
                    
                    // VSCodeのパスを確認
                    string vscodeCommand = FindVSCodePath();
                    if (string.IsNullOrEmpty(vscodeCommand))
                    {
                        MessageBox.Show("Visual Studio Code が見つかりません。\nVS Codeがインストールされているか確認してください。", 
                            "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // URLの場合はパス指定できないため、エラーメッセージを表示
                    if (IsUrl(path))
                    {
                        MessageBox.Show("WebサイトのURLはVS Codeで開くことができません。", 
                            "エラー", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    // ファイルまたはフォルダーが存在するか確認
                    if (!System.IO.File.Exists(path) && !System.IO.Directory.Exists(path))
                    {
                        MessageBox.Show($"指定されたパスが見つかりません: {path}", 
                            "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // VS Codeで開く
                    var startInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = vscodeCommand,
                        Arguments = $"\"{path}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    System.Diagnostics.Process.Start(startInfo);
                    StatusText = $"「{item.Name}」をVS Codeで開きました";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"VS Codeで開く際にエラーが発生しました: {ex.Message}", 
                        "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanOpenWithVSCode(object? parameter)
        {
            var item = parameter as LauncherItem ?? SelectedItem;
            if (item == null) return false;

            // URLの場合は無効
            if (IsUrl(item.Path)) return false;

            // ファイルまたはフォルダーが存在する場合のみ有効
            return System.IO.File.Exists(item.Path) || System.IO.Directory.Exists(item.Path);
        }

        private string FindVSCodePath()
        {
            // 一般的なVS Codeのインストールパスを確認
            var possiblePaths = new[]
            {
                @"C:\Users\" + Environment.UserName + @"\AppData\Local\Programs\Microsoft VS Code\Code.exe",
                @"C:\Program Files\Microsoft VS Code\Code.exe",
                @"C:\Program Files (x86)\Microsoft VS Code\Code.exe",
                "code" // PATH環境変数に登録されている場合
            };

            foreach (var path in possiblePaths)
            {
                if (path == "code")
                {
                    // PATH環境変数での確認
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
                        // whereコマンドが失敗した場合は次へ
                    }
                }
                else if (System.IO.File.Exists(path))
                {
                    return path;
                }
            }

            return string.Empty;
        }

        private void OpenWithOffice(object? parameter)
        {
            var item = parameter as LauncherItem ?? SelectedItem;
            if (item != null)
            {
                try
                {
                    // アイテムの所属プロジェクトを特定
                    var itemProject = FindProjectContainingItem(item);
                    
                    // Record the access before launching
                    RecordItemAccess(item);
                    
                    // LauncherServiceのOffice機能を使用
                    launcherService.LaunchItemWithOffice(item);
                    
                    // 最終アクセス時刻を更新した後、該当プロジェクトを保存
                    if (itemProject != null)
                    {
                        projectService.SaveProject(itemProject);
                    }
                    else
                    {
                        // フォールバック：通常の保存メソッドを使用
                        SaveData();
                    }
                    
                    // Refresh smart launcher items after launching
                    LoadSmartLauncherItems();
                    
                    StatusText = $"「{item.Name}」をOfficeアプリで開きました";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanOpenWithOffice(object? parameter)
        {
            var item = parameter as LauncherItem ?? SelectedItem;
            if (item == null) return false;

            string path = item.Path;

            // SharePointやOffice Online URLの場合は有効
            if (IsOfficeUrl(path)) return true;

            // ローカルファイルの場合はOfficeファイルかどうか確認
            if (System.IO.File.Exists(path))
            {
                string extension = System.IO.Path.GetExtension(path).ToLower();
                return IsOfficeFile(extension);
            }

            // Google DocsのURLも対応
            if (IsUrl(path) && path.Contains("docs.google.com"))
            {
                return path.Contains("/spreadsheets/") || 
                       path.Contains("/document/") || 
                       path.Contains("/presentation/");
            }

            return false;
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
                       host.Contains("-my.sharepoint.com") ||
                       host.Contains("onedrive.live.com") ||
                       host.Contains("1drv.ms") ||
                       host.Contains("office.com") ||
                       host.Contains("outlook.office365.com") ||
                       host.Contains("outlook.office.com") ||
                       path.Contains("/_layouts/") ||
                       path.Contains("/workbook/") ||
                       path.Contains("/document/") ||
                       path.Contains("/presentation/") ||
                       path.Contains(":x:") ||
                       path.Contains(":w:") ||
                       path.Contains(":p:");
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

        private void OpenInExplorer(object? parameter)
        {
            var item = parameter as LauncherItem ?? SelectedItem;
            if (item != null)
            {
                try
                {
                    string path = item.Path;

                    // URLの場合はブラウザで開く
                    if (IsUrl(path))
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = path,
                            UseShellExecute = true
                        });
                        StatusText = $"「{item.Name}」をブラウザーで開きました";
                        return;
                    }

                    // フォルダーの場合はエクスプローラーで開く
                    if (System.IO.Directory.Exists(path))
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "explorer.exe",
                            Arguments = $"\"{path}\"",
                            UseShellExecute = false
                        });
                        StatusText = $"「{item.Name}」をエクスプローラーで開きました";
                        return;
                    }

                    // ファイルの場合は親フォルダーをエクスプローラーで開いてファイルを選択
                    if (System.IO.File.Exists(path))
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "explorer.exe",
                            Arguments = $"/select,\"{path}\"",
                            UseShellExecute = false
                        });
                        StatusText = $"「{item.Name}」の場所をエクスプローラーで開きました";
                        return;
                    }

                    // パスが存在しない場合は親フォルダーを開く
                    var parentDir = System.IO.Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(parentDir) && System.IO.Directory.Exists(parentDir))
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "explorer.exe",
                            Arguments = $"\"{parentDir}\"",
                            UseShellExecute = false
                        });
                        StatusText = $"「{item.Name}」の親フォルダーをエクスプローラーで開きました";
                        return;
                    }

                    MessageBox.Show($"パスが見つかりません: {path}", "エラー",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"エクスプローラーで開く際にエラーが発生しました: {ex.Message}", 
                        "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanOpenInExplorer(object? parameter)
        {
            var item = parameter as LauncherItem ?? SelectedItem;
            return item != null && !string.IsNullOrEmpty(item.Path);
        }

        // プロジェクト並び替えメソッド
        private void MoveProjectUp(object? parameter)
        {
            if (SelectedProjectNode == null) return;
            
            var projectId = SelectedProjectNode.Id;
            var project = Projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null) return;

            var siblings = GetSiblingProjects(project);
            var currentIndex = siblings.IndexOf(project);
            
            if (currentIndex > 0)
            {
                var targetProject = siblings[currentIndex - 1];
                SwapProjectOrder(project, targetProject);
                UpdateProjectOrderIndices();
                SaveData();
                BuildProjectHierarchy();
                
                // 選択状態を維持
                var movedNode = FindProjectNode(projectId);
                if (movedNode != null)
                {
                    SelectedProjectNode = movedNode;
                }
            }
        }

        private bool CanMoveProjectUp(object? parameter)
        {
            if (SelectedProjectNode == null) return false;
            
            var project = Projects.FirstOrDefault(p => p.Id == SelectedProjectNode.Id);
            if (project == null) return false;

            var siblings = GetSiblingProjects(project);
            var currentIndex = siblings.IndexOf(project);
            
            return currentIndex > 0;
        }

        private void MoveProjectDown(object? parameter)
        {
            if (SelectedProjectNode == null) return;
            
            var projectId = SelectedProjectNode.Id;
            var project = Projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null) return;

            var siblings = GetSiblingProjects(project);
            var currentIndex = siblings.IndexOf(project);
            
            if (currentIndex < siblings.Count - 1)
            {
                var targetProject = siblings[currentIndex + 1];
                SwapProjectOrder(project, targetProject);
                UpdateProjectOrderIndices();
                SaveData();
                BuildProjectHierarchy();
                
                // 選択状態を維持
                var movedNode = FindProjectNode(projectId);
                if (movedNode != null)
                {
                    SelectedProjectNode = movedNode;
                }
            }
        }

        private bool CanMoveProjectDown(object? parameter)
        {
            if (SelectedProjectNode == null) return false;
            
            var project = Projects.FirstOrDefault(p => p.Id == SelectedProjectNode.Id);
            if (project == null) return false;

            var siblings = GetSiblingProjects(project);
            var currentIndex = siblings.IndexOf(project);
            
            return currentIndex < siblings.Count - 1;
        }

        private void SwapProjectOrder(Project project1, Project project2)
        {
            var temp = project1.OrderIndex;
            project1.OrderIndex = project2.OrderIndex;
            project2.OrderIndex = temp;
        }

        private void UpdateProjectOrderIndices()
        {
            // 全てのプロジェクトレベルでOrderIndexを再計算
            var allProjects = Projects.ToList();
            var groupedByParent = allProjects.GroupBy(p => p.ParentId);
            
            foreach (var group in groupedByParent)
            {
                var orderedProjects = group.OrderBy(p => p.OrderIndex).ToList();
                for (int i = 0; i < orderedProjects.Count; i++)
                {
                    orderedProjects[i].OrderIndex = i;
                }
            }
        }

        // グループ並び替えメソッド
        private void MoveGroupUp(object? parameter)
        {
            if (CurrentProject == null || SelectedViewGroup == null) return;

            var groupId = SelectedViewGroup.Id;
            var groups = CurrentProject.Groups.OrderBy(g => g.OrderIndex).ToList();
            var currentIndex = groups.IndexOf(SelectedViewGroup);
            
            if (currentIndex > 0)
            {
                var targetGroup = groups[currentIndex - 1];
                SwapGroupOrder(SelectedViewGroup, targetGroup);
                UpdateGroupOrderIndices();
                UpdateGroupList();
                SaveData();
                
                // 選択状態を維持
                var movedGroup = CurrentProject.Groups.FirstOrDefault(g => g.Id == groupId);
                if (movedGroup != null)
                {
                    SelectedViewGroup = movedGroup;
                }
            }
        }

        private bool CanMoveGroupUp(object? parameter)
        {
            if (CurrentProject == null || SelectedViewGroup == null) return false;
            if (SelectedViewGroup.Id == "all") return false; // 「すべて」グループは移動不可

            var groups = CurrentProject.Groups.OrderBy(g => g.OrderIndex).ToList();
            var currentIndex = groups.IndexOf(SelectedViewGroup);
            
            return currentIndex > 0;
        }

        private void MoveGroupDown(object? parameter)
        {
            if (CurrentProject == null || SelectedViewGroup == null) return;

            var groupId = SelectedViewGroup.Id;
            var groups = CurrentProject.Groups.OrderBy(g => g.OrderIndex).ToList();
            var currentIndex = groups.IndexOf(SelectedViewGroup);
            
            if (currentIndex < groups.Count - 1)
            {
                var targetGroup = groups[currentIndex + 1];
                SwapGroupOrder(SelectedViewGroup, targetGroup);
                UpdateGroupOrderIndices();
                UpdateGroupList();
                SaveData();
                
                // 選択状態を維持
                var movedGroup = CurrentProject.Groups.FirstOrDefault(g => g.Id == groupId);
                if (movedGroup != null)
                {
                    SelectedViewGroup = movedGroup;
                }
            }
        }

        private bool CanMoveGroupDown(object? parameter)
        {
            if (CurrentProject == null || SelectedViewGroup == null) return false;
            if (SelectedViewGroup.Id == "all") return false; // «すべて» グループは移動不可

            var groups = CurrentProject.Groups.OrderBy(g => g.OrderIndex).ToList();
            var currentIndex = groups.IndexOf(SelectedViewGroup);
            
            return currentIndex < groups.Count - 1;
        }

        private void SwapGroupOrder(ItemGroup group1, ItemGroup group2)
        {
            var temp = group1.OrderIndex;
            group1.OrderIndex = group2.OrderIndex;
            group2.OrderIndex = temp;
        }

        private void UpdateGroupOrderIndices()
        {
            if (CurrentProject == null) return;
            
            var orderedGroups = CurrentProject.Groups.OrderBy(g => g.OrderIndex).ToList();
            for (int i = 0; i < orderedGroups.Count; i++)
            {
                orderedGroups[i].OrderIndex = i;
            }
        }

        // ショートカットコマンドの実装
        private void FocusSearch(object? parameter)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                var searchTextBox = mainWindow.FindName("SearchTextBox") as TextBox;
                if (searchTextBox != null)
                {
                    searchTextBox.Focus();
                    searchTextBox.SelectAll();
                    StatusText = "🔍 検索ボックスにフォーカスしました";
                }
            }
        }

        private void FocusProject(object? parameter)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                var projectTreeView = mainWindow.FindName("ProjectTreeView") as TreeView;
                if (projectTreeView != null)
                {
                    projectTreeView.Focus();
                    StatusText = "📁 プロジェクト一覧にフォーカスしました";
                }
            }
        }

        private void FocusGroup(object? parameter)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                var groupTreeView = mainWindow.FindName("GroupTreeView") as TreeView;
                if (groupTreeView != null)
                {
                    groupTreeView.Focus();
                    StatusText = "🗂️ グループ一覧にフォーカスしました";
                }
            }
        }

        private void FocusMainList(object? parameter)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                var mainListView = mainWindow.FindName("MainListView") as ListView;
                if (mainListView != null)
                {
                    mainListView.Focus();
                    StatusText = "📋 メインリストにフォーカスしました";
                }
            }
        }

        private void FocusSmartLauncher(object? parameter)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                var smartLauncherListView = mainWindow.FindName("SmartLauncherListView") as ListView;
                if (smartLauncherListView != null)
                {
                    smartLauncherListView.Focus();
                    
                    // 何も選択されていない場合は最初のアイテムを選択
                    if (smartLauncherListView.SelectedItem == null && SmartLauncherItems.Count > 0)
                    {
                        smartLauncherListView.SelectedIndex = 0;
                    }
                    
                    StatusText = "🚀 SmartLauncherにフォーカスしました";
                }
            }
        }

        // ドラッグ&ドロップ並び替えメソッド
        public void MoveProjectToPosition(string projectId, int newIndex)
        {
            var project = Projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null) return;

            var siblings = GetSiblingProjects(project);
            var currentIndex = siblings.IndexOf(project);
            
            if (currentIndex == newIndex) return;

            // 新しい位置にプロジェクトを移動
            siblings.RemoveAt(currentIndex);
            siblings.Insert(newIndex, project);

            // OrderIndexを更新
            for (int i = 0; i < siblings.Count; i++)
            {
                siblings[i].OrderIndex = i;
            }

            SaveData();
            BuildProjectHierarchy();
        }

        public void MoveGroupToPosition(string groupId, int newIndex)
        {
            if (CurrentProject == null) return;

            var group = CurrentProject.Groups.FirstOrDefault(g => g.Id == groupId);
            if (group == null) return;

            var groups = CurrentProject.Groups.OrderBy(g => g.OrderIndex).ToList();
            var currentIndex = groups.IndexOf(group);
            
            if (currentIndex == newIndex) return;

            // 新しい位置にグループを移動
            groups.RemoveAt(currentIndex);
            groups.Insert(newIndex, group);

            // OrderIndexを更新
            for (int i = 0; i < groups.Count; i++)
            {
                groups[i].OrderIndex = i;
            }

            UpdateGroupOrderIndices();
            SaveData();
        }

        // ショートカットコマンド
        private void ClearSearch(object? parameter)
        {
            if (!string.IsNullOrEmpty(SearchText))
            {
                SearchText = string.Empty;
                StatusText = "🔍 検索をクリアしました";
            }
        }

        private void SearchAllProjects(object? parameter)
        {
            // スマートランチャーの「すべてのプロジェクト」を選択
            var allProjectsItem = SmartLauncherItems.FirstOrDefault(item => 
                item.ItemType == SmartLauncherItemType.AllProjects);
            
            if (allProjectsItem != null)
            {
                SelectedSmartLauncherItem = allProjectsItem;
                
                // 検索テキストボックスにフォーカス
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    var searchTextBox = mainWindow.FindName("SearchTextBox") as TextBox;
                    if (searchTextBox != null)
                    {
                        searchTextBox.Focus();
                        searchTextBox.SelectAll();
                    }
                }
                
                StatusText = "🔍 すべてのプロジェクトから検索モードに切り替えました";
            }
        }

        private List<Project> GetSiblingProjects(Project project)
        {
            return Projects.Where(p => p.ParentId == project.ParentId)
                          .OrderBy(p => p.OrderIndex)
                          .ToList();
        }
    }
}