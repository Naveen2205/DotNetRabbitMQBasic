using System;
using System.Text;
using System.Collections.Concurrent;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RPC_Client
{
    class Program
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();
        private readonly IBasicProperties props;

        public Program(){
            var factory = new ConnectionFactory() { HostName = "localhost" };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare().QueueName;
            consumer = new EventingBasicConsumer(channel);

            props = channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;
            consumer.Received += (model, ea)=>{
                Console.WriteLine("Response received");
                var body =ea.Body.ToArray();
                var response = Encoding.UTF8.GetString(body);
                if(ea.BasicProperties.CorrelationId == correlationId){
                    respQueue.Add(response);
                }
            };
            channel.BasicConsume(
                consumer: consumer,
                queue: replyQueueName,
                autoAck: true
            );
        }

        public string Call(string message){
            var messageBytes = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(
                exchange: "",
                routingKey: "rpc_queue",
                basicProperties: props,
                body: messageBytes
            );
            var res = respQueue.Take();
             Console.WriteLine("Returning point...");
             return res;
        }

        public void Close(){
            connection.Close();
        }
        static void Main(string[] args)
        {
            var rpcClient = new Program();
            Console.WriteLine("[x] Requesting fib(30)");
            var response = rpcClient.Call("30");
            Console.WriteLine("[.] Got '{0}'", response);
            rpcClient.Close();
        }
    }
}
