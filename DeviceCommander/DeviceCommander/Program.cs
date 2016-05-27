using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceCommander
{
    class Program
    {
        static void Main(string[] args)
        {
            SendMessageToCloudAsync("test");
            Console.WriteLine("Press any key to quit.");
            Console.ReadLine();
        }

        private static async void SendMessageToCloudAsync(string myMessage)
        {
            await CommandService.SendDeviceToCloudMessageAsync(myMessage);
            Console.WriteLine("{0} > Sending interactive message: {1}", DateTime.Now, myMessage);
            ShowConsole();
        }

        private static void ShowConsole()
        {
            Console.Write("Type a message to send: ");
            string toSend = Console.ReadLine();            
            SendMessageToCloudAsync(toSend);
            Console.ReadLine();
        }
    }
}
