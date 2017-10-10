using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;

namespace Helpers
{
    public class Message
    {
        public MessageType Type { get; set; }
        public string SenderUserName { get; set; }

        public int SenderUserId { get; set; }

        // -1 - no specific receiver. message is for all users
        public int ReceiverUserId { get; set; } = -1;

        public string Content { get; set; }
        public string AdditionalData { get; set; }

        public static string Serialize(Message message)
        {
            return new JavaScriptSerializer().Serialize(message);
        }

        public static Message Deserialize(string jsonMessage)
        {
            try
            {
                return new JavaScriptSerializer().Deserialize<Message>(jsonMessage);
            }
            catch
            {
                return null;
            }
        }
    }

    public enum MessageType
    {
        TextMessage,
        HelloMessage,
        UnconnectMessage,
        JoinMessage,
        FileMessage
    }
}