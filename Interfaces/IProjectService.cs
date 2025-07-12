using ModernLauncher.Models;
using System.Collections.Generic;

namespace ModernLauncher.Interfaces
{
    public interface IProjectService
    {
        void SaveProject(Project project);
        void SaveProjectList(IEnumerable<ProjectInfo> projectInfos);
        Project? LoadProject(string id);
        List<ProjectInfo> LoadProjectList();
        void DeleteProject(string id);
        void ExportProject(Project project, string filePath);
        Project? ImportProject(string filePath);
    }
}