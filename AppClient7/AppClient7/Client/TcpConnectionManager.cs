using AppClient7.Configuration;
using SharpCompress.Common;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class TcpConnectionManager
{
    private TcpClient tcpClient;
    private NetworkStream networkStream;
    private readonly string serverIp;
    private readonly int serverPort;

    public TcpConnectionManager()
    {
        serverIp = ConfigurationHelper.serverIp;
        serverPort = ConfigurationHelper.SignalRPort;
        Connect();
    }

    private void Connect()
    {
        try
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(serverIp, serverPort);
            networkStream = tcpClient.GetStream();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to server: {ex.Message}");
            // Optionally implement a retry mechanism here
        }
    }

    public async Task SendDataAsync(string data)
    {
        
        if (networkStream != null && networkStream.CanWrite)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            try
            {
                await networkStream.WriteAsync(dataBytes, 0, dataBytes.Length);
                byte[] buffer = new byte[4096];
                string receivedData = Encoding.UTF8.GetString(buffer);

                using (var writer = new StreamWriter("C:\\Users\\staj\\source\\repos\\SignalR\\SignalR\\Controllers\\tcp.txt", append: true)) // append: true ile dosyayı append modunda aç
                {
                    await writer.WriteLineAsync(receivedData);
                }
            }
            catch (IOException ex)
            {
                if (ex.InnerException is SocketException se && se.SocketErrorCode == SocketError.ConnectionReset)
                {
                    Console.WriteLine("Connection was forcibly closed by the remote host. Reconnecting...");
                    Reconnect();
                    await SendDataAsync(data); // Retry sending data after reconnection
                }
                else
                {
                    Console.WriteLine($"IOException: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected exception: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("Network stream is not writable. Reconnecting...");
            Reconnect();
            await SendDataAsync(data); // Retry sending data after reconnection
        }
    }

    private void Reconnect()
    {
        CloseConnection();
        Connect();
    }

    public void CloseConnection()
    {
        networkStream?.Close();
        tcpClient?.Close();
    }
}
