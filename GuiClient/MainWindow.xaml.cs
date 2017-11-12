using Helpers;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Path = System.IO.Path;

namespace GuiClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Color> _colors = GenerateListOfColors();
        private Dictionary<string, Color> _userColors = new Dictionary<string, Color>();
        public Client Client { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            var input = new UserInput();
            input.ShowDialog();

            var name = input.NicknameBox.Text.Trim();
            var passphrase = input.PassphraseBox.Password.Trim();

            Client = new Client(name, passphrase);
            Client.MessageIncomeEvent += ClientOnMessageIncomeEvent;
            Client.NewUserJoinedEvent += ClientOnNewUserJoinedEvent;
            Client.FileIncomeEvent += ClientOnFileIncomeEvent;
            ChatBox.IsReadOnly = true;

            FocusManager.SetFocusedElement(this, InputBox);
        }

        private async void ClientOnFileIncomeEvent(string userId, string username, string originalFileName,
            string cryptedfileName)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "CryptedChatTemp");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            var cryptedPath = Path.Combine(path, cryptedfileName);
            path = Path.Combine(path, originalFileName);
            var file = await Client.DownloadFile(cryptedfileName);
            
            if (File.Exists(path)) File.Delete(path);

            using (var fileStream = new FileStream(cryptedPath, FileMode.Create, FileAccess.Write))
            {
                file.CopyTo(fileStream);
            }

            Cipher.DecryptFile(cryptedPath,path,Client.PassPhrase);
            File.Delete(cryptedPath);
            ChatBox.AppendHyperLink(username, originalFileName, _userColors[userId]);
        }
        
        private void ClientOnNewUserJoinedEvent(string userId)
        {
            if (_colors.Count == 0)
            {
                _colors = GenerateListOfColors();
                _colors.RemoveAt(0); // remove black
            }
            if (_userColors.ContainsKey(userId)) return;
            int rand = new Random().Next(0, _colors.Count - 1);
            _userColors.Add(userId, _colors[rand]);
            _colors.RemoveAt(rand);
        }

        private static List<Color> GenerateListOfColors()
        {
            return new Color[]
            {
                Colors.Coral, Colors.Blue, Colors.BlueViolet, Colors.Chartreuse, Colors.Brown, Colors.DarkGreen,
                Colors.DarkRed, Colors.DeepPink, Colors.DarkOrange, Colors.OrangeRed
            }.ToList();
        }

        private void ClientOnMessageIncomeEvent(string userId, string username, string msg)
        {
            if (!_userColors.ContainsKey(userId)) ClientOnNewUserJoinedEvent(userId);
            ChatBox.AppendText(username, msg, _userColors[userId]);
        }

        private void SendMessage()
        {
            if (string.IsNullOrEmpty(InputBox.Text)) return;
            Client.SendMessageToConnected(InputBox.Text);
            InputBox.Text = String.Empty;
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void InputBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) SendMessage();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            var result = fileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                var fi = new FileInfo(fileDialog.FileName);
                if (!File.Exists(fi.FullName)) return;

                var cryptedTempFilePath = Path.Combine(fi.DirectoryName, "Crypted" + fi.Name);

                Cipher.EncryptFile(fi.FullName,cryptedTempFilePath,Client.PassPhrase);
                var fs = File.OpenRead(cryptedTempFilePath);
                
                var cryptedFileName = await Client.UploadFile(fs);
                Client.SendFileToConnected(fi.Name, cryptedFileName);
                fs.Close();
                File.Delete(cryptedFileName);
            }
        }
    }
}