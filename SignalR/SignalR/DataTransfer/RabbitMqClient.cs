using Microsoft.AspNetCore.Connections;
using Newtonsoft.Json;
using System.Text;
using RabbitMQ.Client;
using SignalR.Models;
using RabbitMQ.Client.Events;
using Microsoft.AspNetCore.SignalR;

namespace SignalR.DataTransfer
{
    public class RabbitMqClient : IDisposable
    {
        private readonly ConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IHubContext<MyHub> _hubContext;

        public RabbitMqClient(string hostName, IHubContext<MyHub> hubContext)
        {
            _factory = new ConnectionFactory() { HostName = hostName };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            _hubContext = hubContext;
        }


        public void SendMessage (string queueName, TraceData trace)
        {
            try
            {
                _channel.QueueDeclare(queue: queueName,
                                      durable: false,
                                      exclusive: false,
                                      autoDelete: false,
                                      arguments: null);               
                  try
                    {
                        

                        var json = JsonConvert.SerializeObject(trace);
                        var body = Encoding.UTF8.GetBytes(json);

                        var properties = _channel.CreateBasicProperties();
                        properties.Persistent = true;

                        _channel.BasicPublish(exchange: "",
                                             routingKey: queueName,
                                             basicProperties: properties,
                                             body: body);
                        Console.WriteLine($"Sent: {json}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public void StartListening(string queueName)
        {
            _channel.QueueDeclare(queue: queueName,
                                  durable: false,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Received message: {message}");

                // Veriyi SignalR ile istemcilere gönder
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);

            ;
            };

            _channel.BasicConsume(queue: queueName,
                                  autoAck: true,
                                  consumer: consumer);

 
        }
        public void Dispose()
        {
            _channel.Close();
            _connection.Close();
        }
    }
}
