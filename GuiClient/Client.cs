using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using ChatService;
using Helpers;

namespace GuiClient
{
    public class Client : IClient
    {
        public string Name { get; set; }
        public string PassPhrase { get; set; }
        private int _id;
        private IServer _channel;

        public delegate void MessageIcomeHandler(int userId, string username, string msg);
        public event MessageIcomeHandler MessageIncomeEvent;
        public delegate void NewUserJoinedHandler(int userId);
        public event NewUserJoinedHandler NewUserJoinedEvent;



        public Client(string name, string passPhrase)
        {
            Name = name;
            PassPhrase = passPhrase;

            DuplexChannelFactory<IServer> channelFactory = new DuplexChannelFactory<IServer>(this,
                new NetTcpBinding() {Security = new NetTcpSecurity() {Mode = SecurityMode.None}},
                new EndpointAddress("net.tcp://192.168.1.75:3100/CryptedChat"));
            _channel = channelFactory.CreateChannel();

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
            MessageIncomeEvent?.Invoke(_id,Name,text); 
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

        private void SendJoinMessage(int id)
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