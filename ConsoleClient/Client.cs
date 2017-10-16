using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using ChatService;
using Helpers;

namespace ConsoleClient
{
    public class Client : IClient
    {
        public string Name { get; set; }
        public string PassPhrase { get; set; }
        private string _id;
        private IServer _channel;
        private List<ConsoleColor> _colors = new List<ConsoleColor>();
        private Dictionary<string, ConsoleColor> _userColors = new Dictionary<string, ConsoleColor>();


        public Client(string name, string passPhrase)
        {
            Name = name;
            PassPhrase = passPhrase;

            DuplexChannelFactory<IServer> channelFactory = new DuplexChannelFactory<IServer>(this, new NetTcpBinding() { Security = new NetTcpSecurity() { Mode = SecurityMode.None } },
                new EndpointAddress("net.tcp://192.168.1.75:3100/CryptedChat"));
            _channel = channelFactory.CreateChannel();

            _id = _channel.Register(Name);
            
            Console.Clear();

            SendHelloMessage();

            while (true)
            {
                Console.Write($"<{Name}>:");
                var text = Console.ReadLine();
                if (string.IsNullOrEmpty(text)) continue;
                this.SendMessageToConnected(text);
            }
        }

        private void SendHelloMessage()
        {
            var jsonMsg = Message.Serialize(new Message()
            {
                SenderUserName = Name,
                SenderUserId = _id,
                Type = MessageType.HelloMessage,
            });
            _channel.BroadcastMessageToAll(Cipher.Encrypt(jsonMsg, PassPhrase));
        }

        private void SendMessageToConnected(string text)
        {
            var jsonMsg = Message.Serialize(new Message()
            {
                SenderUserName = Name,
                SenderUserId = _id,
                Type = MessageType.TextMessage,
                Content = text
            });
            _channel.BroadcastMessageToConnectedUsers(Cipher.Encrypt(jsonMsg, PassPhrase));
        }
        private void SendUnconnectMessage()
        {
            var jsonMsg = Message.Serialize(new Message()
            {
                SenderUserName = Name,
                SenderUserId = _id,
                Type = MessageType.UnconnectMessage,
            });
            _channel.BroadcastMessageToConnectedUsers(Cipher.Encrypt(jsonMsg, PassPhrase));
        }

        private void SendJoinMessage(string id)
        {
            var jsonMsg = Message.Serialize(new Message()
            {
                SenderUserName = Name,
                SenderUserId = _id,
                ReceiverUserId = id,
                Type = MessageType.JoinMessage
            });
            _channel.SendMessage(id, Cipher.Encrypt(jsonMsg, PassPhrase));
        }


        public void SendMessage(string cryptedMessage)
        {
            Message msg = null;
            try
            {
                var textMsg = Cipher.Decrypt(cryptedMessage, PassPhrase);
                msg = Message.Deserialize(textMsg);
            }
            catch (Exception e)
            {
                //any error while decrypting or deserializating is considered as fail
                return;
            }

            if (msg == null) return;

            switch (msg.Type)
            {
                case MessageType.TextMessage:
                    ConsoleWriteLine(msg.SenderUserId,msg.SenderUserName,msg.Content);
                    break;
                case MessageType.HelloMessage:
                    if (msg.SenderUserId == _id) return;
                    SetUserColor(msg.SenderUserId);
                    ConsoleWriteLine(msg.SenderUserId, msg.SenderUserName, "connected");
                    SendJoinMessage(msg.SenderUserId);
                    _channel.ConnectWithUser(msg.SenderUserId);
                    break;
                case MessageType.UnconnectMessage:
                    ConsoleWriteLine(msg.SenderUserId, msg.SenderUserName, "disconnected");
                    break;
                case MessageType.JoinMessage:
                    SetUserColor(msg.SenderUserId);
                    ConsoleWriteLine(msg.SenderUserId, msg.SenderUserName, "is online");
                    _channel.ConnectWithUser(msg.SenderUserId);
                    break;
                case MessageType.FileMessage:
                    break;
                default:
                    break;
            }
        }

        private void ConsoleWriteLine(string userId, string username, string msg)
        {
            bool typing = false;
            if(Console.CursorLeft > $"<{Name}>:".Length+1)
            {
                typing = true;
                Console.CursorLeft = 0;
                Console.CursorTop++;
            }
            else
            {
                ClearCurrentConsoleLine();
            }

            Console.ForegroundColor = _userColors.ContainsKey(userId) ? _userColors[userId] : ConsoleColor.Cyan;
            Console.Write($"<{username}>:");
            Console.ResetColor();
            Console.WriteLine(msg);
            Console.Beep(2200,250);
            
            if (!typing) Console.Write($"<{Name}>:");
        }

        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            for (int i = 0; i < Console.WindowWidth; i++)
                Console.Write(" ");
            Console.SetCursorPosition(0, currentLineCursor);
        }

        private void SetUserColor(string userId)
        {
            if (_colors.Count == 0)
            {
                _colors = ((ConsoleColor[])Enum.GetValues(typeof(ConsoleColor))).ToList();
                _colors.RemoveAt(0); // remove black
            }
            if (_userColors.ContainsKey(userId)) return;
            int rand= new Random().Next(0,_colors.Count-1);
            _userColors.Add(userId, _colors[rand]);
            _colors.RemoveAt(rand);
            
        }

        public void Disconnect()
        {
            SendUnconnectMessage();
            _channel.Unregister();
        }
    }
}