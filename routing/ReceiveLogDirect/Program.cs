using System;
using RabbitMQ.Client;
using System.Text;
using System.Linq;

namespace ReceiveLogDirect
{
    class Program
    {
        static void Main(string[] args)
        {
            var severity = args.Length > 1 ? args[0] : "info";
            var message = args.Length > 1 ? string.Join(" ",args.Skip(1).ToArray()) : "Custom "+ severity+" generated";
            var factory = new ConnectionFactory() {HostName = "localhost"};
            using (var connection = factory.CreateConnection()){
                using(var channel = connection.CreateModel()){
                    channel.ExchangeDeclare(
                        exchange: "direct_log",
                        type: ExchangeType.Direct
                    );
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(
                        exchange: "direct_log",
                        routingKey: severity,
                        basicProperties: null,
                        body: body
                    );
                    Console.WriteLine("[x] send {0} : {1}", severity, message);
                }
                Console.WriteLine("Press [Enter] to exit");
                Console.ReadLine();
            }
        }
    }
}
