using ServiceStack.RabbitMq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Util;
using ServiceStack;
using ServiceStack.Messaging;

namespace ss_mq_test1
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

            var serverThread = new Thread(() =>
            {
                var mqServer = new RabbitMqServer();
                var mqClient = mqServer.CreateMessageQueueClient();

                while (!stop)
                {
                    var queryMessage = mqClient.Get<Query>(QueueNames<Query>.In);
                    Console.WriteLine("Got a query message");
                    if (queryMessage != null)
                    {
                        mqClient.Ack(queryMessage);
                        var query = queryMessage.GetBody();
                        var response = new Response { Message = string.Format("Query value: {0}, time: {1}", query.Value, DateTime.Now) };

                        try
                        {
                            mqClient.Publish(
                                queryMessage.ReplyTo,
                                new Message<Response>(response));
                            Console.WriteLine("Sent back a response");
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine("Exception: " + exception.Message);
                        }
                    }
                }            
            }) { IsBackground = true };


            serverThread.Start();

            Console.ReadLine();
        }
    }
}
