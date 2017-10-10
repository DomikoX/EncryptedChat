using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter nickname:");
            var name = Console.ReadLine();
            Console.WriteLine("Enter passPhrase:");
            var passPhrase = Console.ReadLine();
            var client = new Client(name, passPhrase);
            AppDomain.CurrentDomain.ProcessExit += new EventHandler((sender, eventArgs) =>
            {
                client.Disconnect();

            });
            // do some work

        }

        
    }
}
