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
        public UserInput(string inputLabel)
        {
            InitializeComponent();
            Label.Content = inputLabel;
            Closing += (sender, args) =>
            {
                if (string.IsNullOrEmpty(TextBox.Text.Trim())) args.Cancel = true;
            };
            TextBox.KeyUp += (sender, args) =>
            {
                if (args.Key == Key.Enter) Button_Click(null, null);
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBox.Text.Trim())) return;
            Close();
        }
    }
}