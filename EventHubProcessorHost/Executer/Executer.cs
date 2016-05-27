using Microsoft.ServiceBus.Messaging;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;

namespace ExecuterProgram
{
    class Executer
    {

        private static void WriteUsage()
        {
            
        }


        static void Main(string[] args)
        {
           
            string eventHubConnectionString = ConfigurationManager.AppSettings["eventHubConnectionString"]; 
            string eventHubName = ConfigurationManager.AppSettings["eventHubName"];
            string eventHubConsumerGroupName = EventHubConsumerGroup.DefaultGroupName;
            string storageAccountName = ConfigurationManager.AppSettings["storageAccountName"];
            string storageAccountKey = ConfigurationManager.AppSettings["storageAccountKey"];
            string eventProcessorHostName = ConfigurationManager.AppSettings["eventProcessorHostName"];

            string storageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", storageAccountName, storageAccountKey);

            EventProcessorHost eventProcessorHost = new EventProcessorHost(eventProcessorHostName, 
                                                                           eventHubName,
                                                                           eventHubConsumerGroupName, 
                                                                           eventHubConnectionString, 
                                                                           storageConnectionString);

            Console.WriteLine("Registering Executer EventProcessor...");

            var options = new EventProcessorOptions();
            options.InitialOffsetProvider = (pId) => DateTime.UtcNow;
            options.ExceptionReceived += (sender, e) => { Console.WriteLine(e.Exception); };
            eventProcessorHost.RegisterEventProcessorAsync<RuleExecuterEventProcessor>(options).Wait();

            Console.WriteLine("Receiving. Press enter key to stop the Executer and stop the app.");
            Console.ReadLine();

            eventProcessorHost.UnregisterEventProcessorAsync().Wait();
            
        }
    }
}
