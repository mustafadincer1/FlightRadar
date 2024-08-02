using Microsoft.AspNetCore.SignalR;

namespace SignalR.DataTransfer
{
    public class MyHub : Hub
    {

        public async Task SendMessageAsync(string message)
        {
            Console.WriteLine(message);

            // Tüm bağlı istemcilere "ReceiveMessage" isimli metodla mesajı gönderir
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
