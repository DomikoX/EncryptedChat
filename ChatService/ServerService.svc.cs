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
        private static int NO_ID = -1;
        private int _ids = 0;
        private static IClient ClientCallback => OperationContext.Current.GetCallbackChannel<IClient>();
        public List<User> AllConnectedUsers { get; set; } = new List<User>();

        private User Me
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

        public int Register(string name)
        {
            var newUser = new User(OperationContext.Current.GetCallbackChannel<IClient>(), ++_ids);
            //try to remove if already exist (etc. in the case user try to connect from the same machine  and his unregister was't made properly) 
            AllConnectedUsers.Remove(newUser);

            AllConnectedUsers.Add(newUser);
            return newUser.Id;
        }

        public void Unregister()
        {
            //doesn't matter on user Id, in case of comparing users, callback is enought
            var newUser = new User(ClientCallback, NO_ID);
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

        private void BrodcastMessage(List<User> listOfUsers, string cryptedMessage)
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


        public void SendMessage(int userId, string cryptedMessage)
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

        public void ConnectWithUser(int userId)
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