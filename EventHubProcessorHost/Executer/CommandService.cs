using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using System.Configuration;

namespace ExecuterProgram
{
    static class CommandService
    {
        static readonly string deviceId = ConfigurationManager.AppSettings["deviceId"];
        static readonly string deviceConnectionString = ConfigurationManager.AppSettings["deviceConnectionString"];

        public static async Task SendDeviceToCloudMessageAsync(string myMessage)
        {
            var serviceClient = ServiceClient.CreateFromConnectionString(deviceConnectionString);

            var str = myMessage;


            if (!string.IsNullOrEmpty(str))
            {
                var message = new Message(Encoding.ASCII.GetBytes(str));

                await serviceClient.SendAsync(deviceId, message);
            }
        }
    }
}

