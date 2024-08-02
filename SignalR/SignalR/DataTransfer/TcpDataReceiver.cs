namespace SignalR.DataTransfer
{
    using Microsoft.AspNetCore.SignalR;
    using Newtonsoft.Json;
    using System;
    using System.Net.Sockets;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using SignalR.Configuration;
    using SignalR.Models;

    // TCP veri alıcısı için arka plan servisi
    public class TcpDataReceiver : BackgroundService
    {
        private readonly IHubContext<MyHub> _hubContext;
        private readonly RabbitMqClient _rabbitMqClient;
        
  
        public TcpDataReceiver(IHubContext<MyHub> hubContext)
        {
            _hubContext = hubContext;
            _rabbitMqClient = new RabbitMqClient(ConfigurationHelper.RabbitMQAdress, hubContext);
        }

        // Arka plan hizmeti çalıştırılır
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int port = ConfigurationHelper.port;

            // TCP dinleyiciyi başlat
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine("Server is listening...");

            // RabbitMQ'dan mesaj dinlemeye başla
            //_rabbitMqClient.StartListening(ConfigurationHelper.QueueName);

            // Servis çalışmaya devam ederken döngüde kal
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Yeni bir TCP istemcisi kabul et
                    using (TcpClient client = await listener.AcceptTcpClientAsync())
                    {
                        Console.WriteLine("Client connected.");
                        using (NetworkStream stream = client.GetStream())
                        {
                            byte[] buffer = new byte[4096];

                            // Veri akışından veri oku
                            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, stoppingToken);
                            if (bytesRead > 0)
                            {
                                string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                                Console.WriteLine($"Received: {receivedData}");

                                // Gelen veriyi deseralize et
                                var traceData = JsonConvert.DeserializeObject<TraceData>(receivedData);
                               await _hubContext.Clients.All.SendAsync("ReceiveMessage", receivedData);
                                // Veriyi RabbitMQ'ya gönder
                               // _rabbitMqClient.SendMessage(ConfigurationHelper.QueueName, traceData);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception: {e.Message}");
                }
            }
            // Dinleyiciyi durdur
            listener.Stop();
        }
    }
}
