using ServiceStack.RabbitMq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack;
using ServiceStack.Messaging;

namespace ConsoleApplication1
{
    public class Query
    {
        public int Value { get; set; }
    }

    public class Response
    {
        public string Message { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var stop = false;

            var clientThread = new Thread(() =>
            {
                var mqServer = new RabbitMqServer();
                var random = new Random();

                var mqClient = (RabbitMqQueueClient)mqServer.CreateMessageQueueClient();

                int publishCount = 0, recieveCount = 0;

                var tmpQueue = Guid.NewGuid().ToString();

                while (!stop)
                {
                    Thread.Sleep(1000);

                    mqClient.Publish(new Message<Query>(new Query { Value = random.Next(100) }) { ReplyTo = tmpQueue });
                    Console.WriteLine("Sent a query message");
                    publishCount++;

                    var responseMessage = mqClient.Get<Response>(tmpQueue);
                    Console.WriteLine("Recieved a response on queue: " + tmpQueue);
                    if (responseMessage != null)
                    {
                        mqClient.Ack(responseMessage);
                        recieveCount++;
                        Console.WriteLine(string.Format("PublishCount: {0}, RecieveCount: {1}, Response: {2}",
                            publishCount, recieveCount, responseMessage.GetBody().Message));
                    }
                }

                mqClient.Channel.QueueDelete(tmpQueue);

            }) { IsBackground = true };

            clientThread.Start();

            Console.ReadLine();
        }
    }
}
