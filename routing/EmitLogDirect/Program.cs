using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace EmitLogDirect
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() {HostName = "localhost"};
            using(var connection = factory.CreateConnection()){
                using(var channel = connection.CreateModel()){
                    channel.ExchangeDeclare(
                        exchange: "direct_log",
                        type: ExchangeType.Direct
                    );
                    var queueName = channel.QueueDeclare().QueueName;
                    //Binding queue and exchange
                    foreach(var severity in args){
                        channel.QueueBind(
                            queue: queueName,
                            exchange: "direct_log",
                            routingKey: severity
                        );
                    }
                    Console.WriteLine("Loading queues...");
                    var consumer = new EventingBasicConsumer(channel);
                    Console.WriteLine("[*] waiting for messages");
                    consumer.Received += (model, ea)=>{
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var routingKey = ea.RoutingKey;
                        Console.WriteLine("[x] Received {0} : {1}", ea.RoutingKey, message);
                    };
                    channel.BasicConsume(
                        queue: queueName,
                        autoAck: true,
                        consumer: consumer
                    );
                    Console.ReadLine();
                }
            }
        }
    }
}
