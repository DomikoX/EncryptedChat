using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using ChatService;
using Helpers;

namespace GuiClient
{
    public class Client : IClient
    {
        public string Name { get; set; }
        public string PassPhrase { get; set; }
        private string _id;
        private IServer _channel;
        private IFileService _fileChannel;
        private string _address = "net.tcp://localhost/";

        public delegate void MessageIcomeHandler(string userId, string username, string msg);
        public event MessageIcomeHandler MessageIncomeEvent;

        public delegate void FileIcomeHandler(string userId, string username, string originalFileName, string cryptedfileName);
        public event FileIcomeHandler FileIncomeEvent;

        public delegate void NewUserJoinedHandler(string userId);
        public event NewUserJoinedHandler NewUserJoinedEvent;


        public Client(string name, string passPhrase)
        {
            Name = name;
            PassPhrase = passPhrase;

            DuplexChannelFactory<IServer> channelFactory = new DuplexChannelFactory<IServer>(this,
                new NetTcpBinding()
                {
                    Security = new NetTcpSecurity() {Mode = SecurityMode.None},
                },
                new EndpointAddress(_address + "CryptedChat"));
            _channel = channelFactory.CreateChannel();


            ChannelFactory<IFileService> fileChannelFactory =  new ChannelFactory<IFileService>(new NetTcpBinding()
            {
                Security = new NetTcpSecurity()
                {
                    Mode = SecurityMode.None
                },
                MaxBufferSize = Int32.MaxValue,
                MaxReceivedMessageSize = Int32.MaxValue,
                TransferMode = TransferMode.Streamed,
                CloseTimeout = new TimeSpan(0, 1, 0),
                OpenTimeout = new TimeSpan(0, 1, 0),
                ReceiveTimeout = new TimeSpan(0, 30, 0),
                SendTimeout = new TimeSpan(0, 30, 0)
            },new EndpointAddress(_address + "CryptedChatFiles"));
            _fileChannel = fileChannelFactory.CreateChannel();


            _id = _channel.Register(Name);

            SendHelloMessage();
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

        public void SendMessageToConnected(string text)
        {
            var jsonMsg = Message.Serialize(new Message()
            {
                SenderUserName = Name,
                SenderUserId = _id,
                Type = MessageType.TextMessage,
                Content = text
            });
            _channel.BroadcastMessageToConnectedUsers(Cipher.Encrypt(jsonMsg, PassPhrase));
            MessageIncomeEvent?.Invoke(_id, Name, text);
        }

        public Task<string> UploadFile(Stream file)
        {
            return Task.Run(() => _fileChannel.UploadFile(file));
        }

        public Task<Stream> DownloadFile(string fileName)
        {
            return Task.Run(() => _fileChannel.DownloadFile(fileName));
        }

        public void SendFileToConnected(string originalFileName, string cryptedFileName)
        {
            var jsonMsg = Message.Serialize(new Message()
            {
                SenderUserName = Name,
                SenderUserId = _id,
                Type = MessageType.FileMessage,
                Content = originalFileName,
                AdditionalData = cryptedFileName,
            });
            _channel.BroadcastMessageToConnectedUsers(Cipher.Encrypt(jsonMsg, PassPhrase));
            MessageIncomeEvent?.Invoke(_id, Name, $"You sent file: {originalFileName}");
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
                    MessageIncomeEvent?.Invoke(msg.SenderUserId, msg.SenderUserName, msg.Content);
                    break;
                case MessageType.HelloMessage:
                    if (msg.SenderUserId == _id) return;
                    NewUserJoinedEvent?.Invoke(msg.SenderUserId);
                    MessageIncomeEvent?.Invoke(msg.SenderUserId, msg.SenderUserName, "connected");
                    SendJoinMessage(msg.SenderUserId);
                    _channel.ConnectWithUser(msg.SenderUserId);
                    break;
                case MessageType.UnconnectMessage:
                    MessageIncomeEvent?.Invoke(msg.SenderUserId, msg.SenderUserName, "disconnected");
                    break;
                case MessageType.JoinMessage:
                    NewUserJoinedEvent?.Invoke(msg.SenderUserId);
                    MessageIncomeEvent?.Invoke(msg.SenderUserId, msg.SenderUserName, "is online");
                    _channel.ConnectWithUser(msg.SenderUserId);
                    break;
                case MessageType.FileMessage:
                    FileIncomeEvent?.Invoke(msg.SenderUserId, msg.SenderUserName, msg.Content, msg.AdditionalData);
                    break;
                default:
                    break;
            }
        }

        public void Disconnect()
        {
            SendUnconnectMessage();
            _channel.Unregister();
        }
    }
}