using System.Windows;
using PathHub.Services;

namespace PathHub.Views
{
    /// <summary>
    /// BackupManagerWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class BackupManagerWindow : Window
    {
        private readonly BackupService _backupService;

        public BackupManagerWindow()
        {
            InitializeComponent();
            _backupService = new BackupService();
            LoadBackupList();
        }

        /// <summary>
        /// バックアップ一覧を読み込む
        /// </summary>
        private void LoadBackupList()
        {
            var backupList = _backupService.GetBackupList();
            BackupListBox.ItemsSource = backupList;

            if (backupList.Count > 0)
            {
                BackupListBox.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// バックアップ作成ボタン
        /// </summary>
        private void CreateBackup_Click(object sender, RoutedEventArgs e)
        {
            string backupName = BackupNameTextBox.Text.Trim();

            // バックアップ名が空の場合はタイムスタンプを使用
            if (string.IsNullOrWhiteSpace(backupName))
            {
                backupName = null; // サービスが自動生成
            }

            if (_backupService.CreateBackup(backupName))
            {
                BackupNameTextBox.Clear();
                LoadBackupList();
            }
        }

        /// <summary>
        /// 復元ボタン
        /// </summary>
        private void RestoreBackup_Click(object sender, RoutedEventArgs e)
        {
            if (BackupListBox.SelectedItem is BackupInfo selectedBackup)
            {
                if (_backupService.RestoreBackup(selectedBackup.Name))
                {
                    // 復元成功後、ウィンドウを閉じる
                    DialogResult = true;
                    Close();
                }
            }
            else
            {
                MessageBox.Show(
                    "復元するバックアップを選択してください。",
                    "選択エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        /// <summary>
        /// 削除ボタン
        /// </summary>
        private void DeleteBackup_Click(object sender, RoutedEventArgs e)
        {
            if (BackupListBox.SelectedItem is BackupInfo selectedBackup)
            {
                if (_backupService.DeleteBackup(selectedBackup.Name))
                {
                    LoadBackupList();
                }
            }
            else
            {
                MessageBox.Show(
                    "削除するバックアップを選択してください。",
                    "選択エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        /// <summary>
        /// バックアップフォルダを開くボタン
        /// </summary>
        private void OpenBackupFolder_Click(object sender, RoutedEventArgs e)
        {
            _backupService.OpenBackupFolder();
        }

        /// <summary>
        /// 更新ボタン
        /// </summary>
        private void RefreshList_Click(object sender, RoutedEventArgs e)
        {
            LoadBackupList();
        }

        /// <summary>
        /// 閉じるボタン
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
