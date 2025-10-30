using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace PathHub.Services
{
    /// <summary>
    /// バックアップと復元機能を提供するサービスクラス
    /// </summary>
    public class BackupService
    {
        private readonly string _dataFolder;
        private readonly string _backupRootFolder;

        public BackupService()
        {
            _dataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "NicoPath"
            );

            _backupRootFolder = Path.Combine(_dataFolder, "Backups");

            // バックアップフォルダが存在しない場合は作成
            if (!Directory.Exists(_backupRootFolder))
            {
                Directory.CreateDirectory(_backupRootFolder);
            }
        }

        /// <summary>
        /// 現在のすべてのJSONデータをバックアップする
        /// </summary>
        /// <param name="backupName">バックアップ名（省略時はタイムスタンプ）</param>
        /// <param name="showMessage">メッセージを表示するかどうか</param>
        /// <returns>成功した場合はtrue</returns>
        public bool CreateBackup(string? backupName = null, bool showMessage = true)
        {
            try
            {
                // バックアップ名を生成（タイムスタンプ）
                if (string.IsNullOrWhiteSpace(backupName))
                {
                    backupName = $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}";
                }

                // バックアップフォルダを作成
                string backupFolder = Path.Combine(_backupRootFolder, backupName);
                if (Directory.Exists(backupFolder))
                {
                    if (showMessage)
                    {
                        MessageBox.Show(
                            $"バックアップ '{backupName}' は既に存在します。",
                            "エラー",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                    }
                    return false;
                }

                Directory.CreateDirectory(backupFolder);

                // Projectsサブフォルダも作成
                string projectsBackupFolder = Path.Combine(backupFolder, "Projects");
                Directory.CreateDirectory(projectsBackupFolder);

                int fileCount = 0;

                // ルートフォルダのJSONファイルをコピー
                var rootJsonFiles = new[]
                {
                    "projects.json",
                    "windowLayout.json",
                    "colorSettings.json",
                    "categories.json",
                    "pathAccessHistory.json"
                };

                foreach (var fileName in rootJsonFiles)
                {
                    string sourcePath = Path.Combine(_dataFolder, fileName);
                    if (File.Exists(sourcePath))
                    {
                        string destPath = Path.Combine(backupFolder, fileName);
                        File.Copy(sourcePath, destPath, true);
                        fileCount++;
                    }
                }

                // Projectsフォルダ内のすべてのJSONファイルをコピー
                string projectsFolder = Path.Combine(_dataFolder, "Projects");
                if (Directory.Exists(projectsFolder))
                {
                    var projectFiles = Directory.GetFiles(projectsFolder, "*.json");
                    foreach (var sourceFile in projectFiles)
                    {
                        string fileName = Path.GetFileName(sourceFile);
                        string destPath = Path.Combine(projectsBackupFolder, fileName);
                        File.Copy(sourceFile, destPath, true);
                        fileCount++;
                    }
                }

                // バックアップ情報ファイルを作成
                var backupInfo = new
                {
                    BackupName = backupName,
                    CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    FileCount = fileCount,
                    DataFolder = _dataFolder
                };

                string infoPath = Path.Combine(backupFolder, "backup_info.json");
                File.WriteAllText(
                    infoPath,
                    Newtonsoft.Json.JsonConvert.SerializeObject(backupInfo, Newtonsoft.Json.Formatting.Indented)
                );

                if (showMessage)
                {
                    MessageBox.Show(
                        $"バックアップが正常に作成されました。\n\nバックアップ名: {backupName}\nファイル数: {fileCount}",
                        "バックアップ完了",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }

                return true;
            }
            catch (Exception ex)
            {
                if (showMessage)
                {
                    MessageBox.Show(
                        $"バックアップの作成中にエラーが発生しました。\n\n{ex.Message}",
                        "エラー",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
                return false;
            }
        }

        /// <summary>
        /// アプリ起動時に自動バックアップを作成する
        /// </summary>
        /// <param name="maxBackups">保持する最大バックアップ数（デフォルト: 10）</param>
        public void CreateAutoBackupOnStartup(int maxBackups = 10)
        {
            try
            {
                // 自動バックアップを作成（メッセージは表示しない）
                string backupName = $"AutoBackup_{DateTime.Now:yyyyMMdd_HHmmss}";
                CreateBackup(backupName, showMessage: false);

                // 古いバックアップを削除
                CleanupOldBackups(maxBackups);
            }
            catch (Exception ex)
            {
                // 自動バックアップのエラーは無視（ログのみ）
                System.Diagnostics.Debug.WriteLine($"自動バックアップエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// 古いバックアップを削除する
        /// </summary>
        /// <param name="maxBackups">保持する最大バックアップ数</param>
        private void CleanupOldBackups(int maxBackups)
        {
            try
            {
                if (!Directory.Exists(_backupRootFolder))
                {
                    return;
                }

                var backupFolders = Directory.GetDirectories(_backupRootFolder)
                    .Select(d => new
                    {
                        Path = d,
                        Name = Path.GetFileName(d),
                        CreatedTime = Directory.GetCreationTime(d)
                    })
                    .Where(b => b.Name.StartsWith("AutoBackup_")) // 自動バックアップのみ削除
                    .OrderByDescending(b => b.CreatedTime)
                    .ToList();

                // 最大数を超えたバックアップを削除
                if (backupFolders.Count > maxBackups)
                {
                    var backupsToDelete = backupFolders.Skip(maxBackups);
                    foreach (var backup in backupsToDelete)
                    {
                        try
                        {
                            Directory.Delete(backup.Path, true);
                            System.Diagnostics.Debug.WriteLine($"古いバックアップを削除: {backup.Name}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"バックアップ削除エラー: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"バックアップクリーンアップエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// バックアップ一覧を取得する
        /// </summary>
        /// <returns>バックアップ情報のリスト</returns>
        public List<BackupInfo> GetBackupList()
        {
            var backupList = new List<BackupInfo>();

            try
            {
                if (!Directory.Exists(_backupRootFolder))
                {
                    return backupList;
                }

                var backupFolders = Directory.GetDirectories(_backupRootFolder)
                    .OrderByDescending(d => Directory.GetCreationTime(d));

                foreach (var folder in backupFolders)
                {
                    string backupName = Path.GetFileName(folder);
                    string infoPath = Path.Combine(folder, "backup_info.json");

                    var info = new BackupInfo
                    {
                        Name = backupName,
                        FolderPath = folder,
                        CreatedAt = Directory.GetCreationTime(folder)
                    };

                    // backup_info.jsonが存在する場合は詳細情報を読み込む
                    if (File.Exists(infoPath))
                    {
                        try
                        {
                            var jsonContent = File.ReadAllText(infoPath);
                            var backupInfoData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(jsonContent);
                            if (backupInfoData != null)
                            {
                                info.FileCount = backupInfoData.FileCount ?? 0;
                            }
                        }
                        catch
                        {
                            // エラーが発生してもスキップ
                        }
                    }

                    // ファイル数が不明な場合はカウント
                    if (info.FileCount == 0)
                    {
                        info.FileCount = Directory.GetFiles(folder, "*.json", SearchOption.AllDirectories).Length - 1; // backup_info.jsonを除外
                    }

                    backupList.Add(info);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"バックアップ一覧の取得中にエラーが発生しました。\n\n{ex.Message}",
                    "エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

            return backupList;
        }

        /// <summary>
        /// 指定したバックアップからデータを復元する
        /// </summary>
        /// <param name="backupName">復元するバックアップ名</param>
        /// <returns>成功した場合はtrue</returns>
        public bool RestoreBackup(string backupName)
        {
            try
            {
                string backupFolder = Path.Combine(_backupRootFolder, backupName);

                if (!Directory.Exists(backupFolder))
                {
                    MessageBox.Show(
                        $"バックアップ '{backupName}' が見つかりません。",
                        "エラー",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    return false;
                }

                // 確認メッセージ
                var result = MessageBox.Show(
                    $"バックアップ '{backupName}' から復元します。\n\n現在のデータは上書きされます。続行しますか？",
                    "復元の確認",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result != MessageBoxResult.Yes)
                {
                    return false;
                }

                int fileCount = 0;

                // ルートフォルダのJSONファイルを復元
                var jsonFiles = Directory.GetFiles(backupFolder, "*.json")
                    .Where(f => Path.GetFileName(f) != "backup_info.json");

                foreach (var sourceFile in jsonFiles)
                {
                    string fileName = Path.GetFileName(sourceFile);
                    string destPath = Path.Combine(_dataFolder, fileName);
                    File.Copy(sourceFile, destPath, true);
                    fileCount++;
                }

                // Projectsフォルダ内のJSONファイルを復元
                string projectsBackupFolder = Path.Combine(backupFolder, "Projects");
                if (Directory.Exists(projectsBackupFolder))
                {
                    string projectsFolder = Path.Combine(_dataFolder, "Projects");

                    // Projectsフォルダが存在しない場合は作成
                    if (!Directory.Exists(projectsFolder))
                    {
                        Directory.CreateDirectory(projectsFolder);
                    }

                    // 既存のプロジェクトファイルを削除（復元前にクリーンアップ）
                    var existingFiles = Directory.GetFiles(projectsFolder, "*.json");
                    foreach (var file in existingFiles)
                    {
                        File.Delete(file);
                    }

                    // バックアップからプロジェクトファイルを復元
                    var projectFiles = Directory.GetFiles(projectsBackupFolder, "*.json");
                    foreach (var sourceFile in projectFiles)
                    {
                        string fileName = Path.GetFileName(sourceFile);
                        string destPath = Path.Combine(projectsFolder, fileName);
                        File.Copy(sourceFile, destPath, true);
                        fileCount++;
                    }
                }

                MessageBox.Show(
                    $"バックアップから復元が完了しました。\n\nファイル数: {fileCount}\n\nアプリケーションを再起動してください。",
                    "復元完了",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"バックアップの復元中にエラーが発生しました。\n\n{ex.Message}",
                    "エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return false;
            }
        }

        /// <summary>
        /// 指定したバックアップを削除する
        /// </summary>
        /// <param name="backupName">削除するバックアップ名</param>
        /// <returns>成功した場合はtrue</returns>
        public bool DeleteBackup(string backupName)
        {
            try
            {
                string backupFolder = Path.Combine(_backupRootFolder, backupName);

                if (!Directory.Exists(backupFolder))
                {
                    MessageBox.Show(
                        $"バックアップ '{backupName}' が見つかりません。",
                        "エラー",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    return false;
                }

                // 確認メッセージ
                var result = MessageBox.Show(
                    $"バックアップ '{backupName}' を削除しますか？\n\nこの操作は元に戻せません。",
                    "削除の確認",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result != MessageBoxResult.Yes)
                {
                    return false;
                }

                Directory.Delete(backupFolder, true);

                MessageBox.Show(
                    $"バックアップ '{backupName}' を削除しました。",
                    "削除完了",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"バックアップの削除中にエラーが発生しました。\n\n{ex.Message}",
                    "エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return false;
            }
        }

        /// <summary>
        /// バックアップフォルダを開く
        /// </summary>
        public void OpenBackupFolder()
        {
            try
            {
                if (!Directory.Exists(_backupRootFolder))
                {
                    Directory.CreateDirectory(_backupRootFolder);
                }

                System.Diagnostics.Process.Start("explorer.exe", _backupRootFolder);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"バックアップフォルダを開けませんでした。\n\n{ex.Message}",
                    "エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }

    /// <summary>
    /// バックアップ情報を保持するクラス
    /// </summary>
    public class BackupInfo
    {
        public string Name { get; set; } = string.Empty;
        public string FolderPath { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int FileCount { get; set; }

        public string DisplayText => $"{Name} ({CreatedAt:yyyy/MM/dd HH:mm}) - {FileCount}ファイル";
    }
}
