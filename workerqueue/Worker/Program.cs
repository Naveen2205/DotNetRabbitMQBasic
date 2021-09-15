using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;

namespace Worker
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using(var connection = factory.CreateConnection()){
                using(var channel = connection.CreateModel()){
                    channel.QueueDeclare(
                        queue: "hello_task_1",
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );
                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                    Console.WriteLine("[*] waiting for messages.");
                    var consumer = new EventingBasicConsumer(channel);
                    Console.WriteLine("channel passed");
                    consumer.Received += (model, ea)=> {
                        Console.WriteLine("in received");
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine("[x] Received {0}", message);
                        int dots = message.Split('.').Length - 1;
                        Thread.Sleep(dots * 1000);
                        Console.WriteLine("[x] Done");
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    };
                    
                    Console.WriteLine("outside of received");
                    channel.BasicConsume(queue: "task_queue", autoAck: true, consumer: consumer);
                    Console.ReadLine();
                }
            }
        }
    }
}
