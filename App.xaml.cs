using System.Configuration;
using System.Data;
using System.Windows;
using System.Threading;
using System.Runtime.InteropServices;
using System;

namespace ModernLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex? _mutex = null;
        private const string MutexName = "NicoPath_SingleInstance_Mutex";

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        private const int SW_RESTORE = 9;

        protected override void OnStartup(StartupEventArgs e)
        {
            // 多重起動チェック
            _mutex = new Mutex(true, MutexName, out bool createdNew);

            if (!createdNew)
            {
                // 既に起動している場合は、既存のウィンドウをアクティブ化
                ActivateExistingWindow();
                Shutdown();
                return;
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
            base.OnExit(e);
        }

        private void ActivateExistingWindow()
        {
            var current = System.Diagnostics.Process.GetCurrentProcess();
            var processes = System.Diagnostics.Process.GetProcessesByName(current.ProcessName);

            foreach (var process in processes)
            {
                if (process.Id != current.Id && process.MainWindowHandle != IntPtr.Zero)
                {
                    IntPtr handle = process.MainWindowHandle;

                    // ウィンドウが最小化されている場合は復元
                    if (IsIconic(handle))
                    {
                        ShowWindow(handle, SW_RESTORE);
                    }

                    // ウィンドウをフォアグラウンドに
                    SetForegroundWindow(handle);
                    break;
                }
            }
        }
    }

}
