using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace ChatService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract(CallbackContract = typeof(IClient), SessionMode = SessionMode.Required)]
    public interface IServer
    {
        /// <summary>
        /// Register user and return unique ID for him 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [OperationContract]
        int Register(string name);

        /// <summary>
        /// Remove me from list of actual connected users
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void Unregister();


        /// <summary>
        /// Send cryptedMessage to all connected users
        /// </summary>
        /// <param name="cryptedMessage"></param>
        [OperationContract(IsOneWay = true)]
        void BroadcastMessageToAll(string cryptedMessage);

        /// <summary>
        /// Send cryptedMessage to all users which are connected to me 
        /// </summary>
        /// <param name="cryptedMessage"></param>
        [OperationContract(IsOneWay = true)]
        void BroadcastMessageToConnectedUsers(string cryptedMessage);


        /// <summary>
        /// Send message to single user 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cryptedMessage"></param>
        [OperationContract(IsOneWay = true)]
        void SendMessage(int userId, string cryptedMessage);

        /// <summary>
        /// Connect me with this user. Since this moment this user will get all my messages sended by <see cref="BroadcastMessageToConnectedUsers"/>
        /// </summary>
        /// <param name="userId"></param>
        [OperationContract(IsOneWay = true)]
        void ConnectWithUser(int userId);
        
    }
    
}
