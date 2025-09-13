using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ModernLauncher.Views
{
    /// <summary>
    /// FolderImportDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderImportDialog : Window
    {
        public string SelectedFolderPath { get; private set; } = string.Empty;
        public int MaxDepth { get; private set; } = 3;
        public bool IncludeHidden { get; private set; } = false;

        public FolderImportDialog()
        {
            InitializeComponent();
            FolderPathTextBox.Focus();
        }

        private void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var inputDialog = new TextInputDialog("フォルダ選択", "フォルダパスを入力してください:");
            if (inputDialog.ShowDialog() == true)
            {
                string path = inputDialog.InputText;
                if (Directory.Exists(path))
                {
                    FolderPathTextBox.Text = path;
                    UpdatePreview(path);
                }
                else
                {
                    MessageBox.Show("指定されたフォルダが存在しません。", "エラー", 
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void UpdatePreview(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    PreviewTextBlock.Text = "無効なフォルダパスです。";
                    return;
                }

                int maxDepth = 3;
                if (int.TryParse(MaxDepthTextBox.Text, out int parsedDepth) && parsedDepth > 0)
                {
                    maxDepth = parsedDepth;
                }

                var folderName = Path.GetFileName(folderPath);
                var preview = GeneratePreview(folderPath, 0, maxDepth, IncludeHiddenCheckBox.IsChecked == true);
                
                PreviewTextBlock.Text = $"📂 {folderName}\n{preview}";
            }
            catch (Exception ex)
            {
                PreviewTextBlock.Text = $"プレビューの生成に失敗しました: {ex.Message}";
            }
        }

        private string GeneratePreview(string currentPath, int currentDepth, int maxDepth, bool includeHidden)
        {
            if (currentDepth >= maxDepth)
                return "";

            var result = "";
            var indent = new string(' ', (currentDepth + 1) * 2);

            try
            {
                // サブフォルダを処理
                var directories = Directory.GetDirectories(currentPath);
                var visibleDirs = directories.Where(dir =>
                {
                    var dirInfo = new DirectoryInfo(dir);
                    return includeHidden || (dirInfo.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden;
                }).Take(5); // プレビューは最初の5個まで

                foreach (var dir in visibleDirs)
                {
                    var dirName = Path.GetFileName(dir);
                    result += $"{indent}📁 {dirName}\n";
                    
                    if (currentDepth < maxDepth - 1)
                    {
                        var subPreview = GeneratePreview(dir, currentDepth + 1, maxDepth, includeHidden);
                        if (!string.IsNullOrEmpty(subPreview))
                        {
                            result += subPreview;
                        }
                    }
                }

                // ファイルを処理（最初の3個まで）
                var files = Directory.GetFiles(currentPath);
                var visibleFiles = files.Where(file =>
                {
                    var fileInfo = new FileInfo(file);
                    return includeHidden || (fileInfo.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden;
                }).Take(3);

                foreach (var file in visibleFiles)
                {
                    var fileName = Path.GetFileName(file);
                    result += $"{indent}📄 {fileName}\n";
                }

                var totalFiles = files.Count(file =>
                {
                    var fileInfo = new FileInfo(file);
                    return includeHidden || (fileInfo.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden;
                });

                if (totalFiles > 3)
                {
                    result += $"{indent}... 他 {totalFiles - 3} ファイル\n";
                }
            }
            catch (UnauthorizedAccessException)
            {
                result += $"{indent}(アクセス権限がありません)\n";
            }
            catch (Exception ex)
            {
                result += $"{indent}(エラー: {ex.Message})\n";
            }

            return result;
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FolderPathTextBox.Text))
            {
                ShowValidationError("フォルダパスを入力してください。", FolderPathTextBox);
                return;
            }

            if (!Directory.Exists(FolderPathTextBox.Text))
            {
                ShowValidationError("指定されたフォルダが存在しません。", FolderPathTextBox);
                return;
            }

            if (!int.TryParse(MaxDepthTextBox.Text, out int maxDepth) || maxDepth < 1)
            {
                ShowValidationError("階層数は1以上の数値を入力してください。", MaxDepthTextBox);
                return;
            }

            SelectedFolderPath = FolderPathTextBox.Text.Trim();
            MaxDepth = maxDepth;
            IncludeHidden = IncludeHiddenCheckBox.IsChecked == true;

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ShowValidationError(string message, System.Windows.Controls.Control controlToFocus)
        {
            MessageBox.Show(message, "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
            controlToFocus.Focus();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !e.Handled)
            {
                ImportButton_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (e.Key == Key.Escape && !e.Handled)
            {
                CancelButton_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
            
            base.OnKeyDown(e);
        }

        private void FolderPathTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(FolderPathTextBox.Text))
            {
                UpdatePreview(FolderPathTextBox.Text);
            }
        }

        private void MaxDepthTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(FolderPathTextBox.Text))
            {
                UpdatePreview(FolderPathTextBox.Text);
            }
        }

        private void IncludeHiddenCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(FolderPathTextBox.Text))
            {
                UpdatePreview(FolderPathTextBox.Text);
            }
        }

        private void IncludeHiddenCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(FolderPathTextBox.Text))
            {
                UpdatePreview(FolderPathTextBox.Text);
            }
        }
    }
}