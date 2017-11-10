using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace ChatService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ServerService : IServer
    {
        private static IClient ClientCallback => OperationContext.Current.GetCallbackChannel<IClient>();
        public List<Client> AllConnectedUsers { get; set; } = new List<Client>();
     

        private Client Me
        {
            get
            {
                try
                {
                    return AllConnectedUsers.Find(user => user.Callback.Equals(ClientCallback));
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        public string Register(string name)
        {
            var newUser = new Client(ClientCallback, Guid.NewGuid().ToString("N"));
            //try to remove if already exist (etc. in the case user try to connect from the same machine  and his unregister was't made properly) 
            AllConnectedUsers.Remove(newUser);

            AllConnectedUsers.Add(newUser);
            return newUser.Id;
        }

        public void Unregister()
        {
            //doesn't matter on user Id, in case of comparing users, callback is enought
            var newUser = new Client(ClientCallback, "");
            AllConnectedUsers.Remove(newUser);
        }

        public void BroadcastMessageToAll(string cryptedMessage)
        {
            BrodcastMessage(AllConnectedUsers, cryptedMessage);
        }

        public void BroadcastMessageToConnectedUsers(string cryptedMessage)
        {
            BrodcastMessage(Me?.ConnectedUsers, cryptedMessage);
        }

        private void BrodcastMessage(List<Client> listOfUsers, string cryptedMessage)
        {
            if (listOfUsers == null) return;

            for (int i = listOfUsers.Count - 1; i >= 0; i--)
            {
                var user = listOfUsers[i];
                try
                {
                    user.Callback.SendMessage(cryptedMessage);
                }
                catch (Exception e)
                {
                    //TODO need to find proper type of exeption
                    listOfUsers.Remove(user);
                }
            }
        }


        public void SendMessage(string userId, string cryptedMessage)
        {
            try
            {
                var user = AllConnectedUsers.Find(u => u.Id.Equals(userId));
                user.Callback.SendMessage(cryptedMessage);
            }
            catch
            {
                //cannot find user with selected Id in current connected users
            }
        }

        public void ConnectWithUser(string userId)
        {
            try
            {
                var newConnectedUser = AllConnectedUsers.Find(user => user.Id.Equals(userId));
                Me?.ConnectedUsers.Add(newConnectedUser);
            }
            catch
            {
                //cannot find user with selected Id in current connected users
            }
        }
    }
}