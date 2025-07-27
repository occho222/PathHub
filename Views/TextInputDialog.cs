using System;
using System.Windows;
using System.Windows.Input;

namespace ModernLauncher.Views
{
    /// <summary>
    /// TextInputDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class TextInputDialog : Window
    {
        public string InputText { get; private set; } = string.Empty;

        public TextInputDialog(string title, string prompt)
        {
            InitializeComponent();
            
            Title = title;
            PromptTextBlock.Text = prompt;
            
            // Set special button text for group creation
            if (title.Contains("Group") || prompt.Contains("Group"))
            {
                OkButton.Content = "Add Group";
                OkButton.MinWidth = 110;
            }
            
            InputTextBox.Focus();
        }

        public TextInputDialog(string title, string prompt, string initialValue) : this(title, prompt)
        {
            InputTextBox.Text = initialValue;
            InputTextBox.SelectAll();
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AcceptInput();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                CancelInput();
                e.Handled = true;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            AcceptInput();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelInput();
        }

        private void AcceptInput()
        {
            InputText = InputTextBox.Text;
            DialogResult = true;
        }

        private void CancelInput()
        {
            DialogResult = false;
        }
    }
}