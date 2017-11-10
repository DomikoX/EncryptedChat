using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace GuiClient
{
    /// <summary>
    /// Interaction logic for UserInput.xaml
    /// </summary>
    public partial class UserInput : Window
    {
        public UserInput()
        {
            InitializeComponent();
            FocusManager.SetFocusedElement(this, NicknameBox);
            Closing += (sender, args) =>
            {
                if (string.IsNullOrEmpty(NicknameBox.Text.Trim()) || string.IsNullOrEmpty(PassphraseBox.Password.Trim())) args.Cancel = true;
            };

            NicknameBox.KeyUp += (sender, args) =>
            {
                if (args.Key == Key.Enter) FocusManager.SetFocusedElement(this,PassphraseBox);
            };

            PassphraseBox.KeyUp += (sender, args) =>
            {
                if (args.Key == Key.Enter) Button_Click(null, null);
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NicknameBox.Text.Trim()) ||
                string.IsNullOrEmpty(PassphraseBox.Password.Trim())) return;
            Close();
        }
    }
}