using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using ChatService;

namespace TestingConsoleHosting
{
    class Program
    {

        private static string _address = "net.tcp://localhost/";
        static void Main(string[] args)
        {
            HostServer();
            HostFileServer();
            Console.Read();
        }

        private static void HostFileServer()
        {
            ServiceHost host = new ServiceHost(typeof(FileService), new Uri(_address));
            host.AddServiceEndpoint(typeof(IFileService), new NetTcpBinding()
            {
                Security = new NetTcpSecurity()
                {
                    Mode = SecurityMode.None
                },
                MaxBufferSize = Int32.MaxValue,
                MaxReceivedMessageSize = Int32.MaxValue,
                TransferMode = TransferMode.Streamed,
                CloseTimeout =  new TimeSpan(0,1,0),
                OpenTimeout = new TimeSpan(0,1,0),
                ReceiveTimeout = new TimeSpan(0,30,0),
                SendTimeout = new TimeSpan(0,30,0)
            }, "CryptedChatFiles");

            host.Open();
            Console.WriteLine($"FileService is ready .... Listenign on: {_address}/CryptedChatFiles");
        }

        private static void HostServer()
        {
            ServiceHost host = new ServiceHost(typeof(ServerService), new Uri(_address));
            host.AddServiceEndpoint(typeof(IServer), new NetTcpBinding()
            {
                Security = new NetTcpSecurity()
                {
                    Mode = SecurityMode.None
                }
            }, "CryptedChat");

            host.Open();
            Console.WriteLine($"Service is ready .... Listenign on: {_address}/CryptedChat");
        }
    }
}