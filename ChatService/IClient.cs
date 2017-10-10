using System.ServiceModel;

namespace ChatService
{
    [ServiceContract]
    public interface IClient
    {
        /// <summary>
        /// Deliver a message from server to client 
        /// </summary>
        /// <param name="cryptedMessage"></param>
        [OperationContract(IsOneWay = true)]
        void SendMessage(string cryptedMessage);
    }
}