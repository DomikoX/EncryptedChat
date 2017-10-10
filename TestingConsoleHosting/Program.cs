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
        static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(ServerService),new Uri("net.tcp://192.168.1.75:3100"));
            host.AddServiceEndpoint(typeof(IServer), new NetTcpBinding(){Security =  new NetTcpSecurity(){Mode = SecurityMode.None}}, "CryptedChat");
            
            host.Open();

            Console.WriteLine("Service is ready .... Listenign on: net.tcp://192.168.1.75:3100/CryptedChat");
            Console.Read();

        }
    }
}
