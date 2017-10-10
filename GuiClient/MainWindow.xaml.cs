﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GuiClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Color> _colors = GenerateListOfColors();
        private Dictionary<int, Color> _userColors = new Dictionary<int, Color>();
        public Client Client { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            var name = ShowDialogBoxFor("Name").Trim();
            var passphrase = ShowDialogBoxFor("Passphrase").Trim();

            Client = new Client(name, passphrase);
            Client.MessageIncomeEvent += ClientOnMessageIncomeEvent;
            Client.NewUserJoinedEvent += ClientOnNewUserJoinedEvent;
            ChatBox.IsReadOnly = true;
        }

        private string ShowDialogBoxFor(string label)
        {
            var input = new UserInput(label);
            input.ShowDialog();
            return input.TextBox.Text;
        }

        private void ClientOnNewUserJoinedEvent(int userId)
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

        private void ClientOnMessageIncomeEvent(int userId, string username, string msg)
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
    }
}