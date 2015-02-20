using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBTest
{
    class Program
    {
        //http://stackoverflow.com/questions/13168459/azure-service-bus-over-http-behind-proxy
        static void Main(string[] args)
        {
            ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.Http;

            var connectionString = "Endpoint=sb://xxxxxxxxxxxxxxxxx.servicebus.windows.net/;SharedAccessKeyName=[KEYNAME];SharedAccessKey=[KEY]";

            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            Queue(namespaceManager);

            var eHubName = "Hub1";

            if (!namespaceManager.EventHubExists(eHubName))
            {
                var desc = namespaceManager.CreateEventHub(eHubName);

                //desc.
            }

            var sendclient = EventHubClient.CreateFromConnectionString(connectionString, eHubName);

            sendclient.Send(new EventData(new byte[] { 1, 2, 3, 4, 5 }));

            EventHubConsumerGroup group = sendclient.GetDefaultConsumerGroup();
            var receiver = group.CreateReceiver(sendclient.GetRuntimeInformation().PartitionIds[0]);

            while (true)
            {
                var data = receiver.Receive();

                Console.WriteLine(data.GetBytes());
            }

            //Queue(namespaceManager);
        }

        private static void Queue(NamespaceManager namespaceManager)
        {
            var qName = "vda_processedcdf";


            namespaceManager.Settings.OperationTimeout = new TimeSpan(0, 10, 0);

            MessagingFactorySettings settings = new MessagingFactorySettings
            {
                OperationTimeout = namespaceManager.Settings.OperationTimeout,
                TokenProvider = namespaceManager.Settings.TokenProvider
            };

            var address = ServiceBusEnvironment.CreateServiceUri("sb", "rtstvdadf-ne", string.Empty);

            var messagingFactory = MessagingFactory.Create(address, settings);

            var client = messagingFactory.CreateQueueClient(qName, ReceiveMode.ReceiveAndDelete);

            //var client = QueueClient.CreateFromConnectionString(connectionString, qName);
            //client.

            while (true)
            {
                var msg = client.Receive(TimeSpan.FromSeconds(1));

                if (msg != null)
                {
                    var body = msg.GetBody<string>();
                    Console.WriteLine("{0}", msg.MessageId);
                    msg.Complete();
                }
            }
        }
    }
}
