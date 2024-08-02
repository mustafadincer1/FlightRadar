using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AppClient7.Configuration;
using AppClient7.Manager;
using AppClient7.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Servers;
using Newtonsoft.Json;
using Serilog;
using Serilog.Formatting.Json;
using Serilog.Sinks.MongoDB;
using SharpCompress.Compressors.Xz;
using WebSocketSharp;

class Program
{
    private TcpClient client;
    private NetworkStream stream;
    static async Task Main(string[] args)
    {

        using CancellationTokenSource cts = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cts.Cancel();
        }; 
        var mongoDbManager = new MongoDbManager(ConfigurationHelper.mongoConnectionString, ConfigurationHelper.mongoDbName, ConfigurationHelper.mongoCollectionName);

        Console.WriteLine("Kullanmak İstediğiniz Server'ı Seçin.");
        Console.WriteLine("1) SignalR");
        Console.WriteLine("2) StompServer");
        if(Console.ReadLine() == "1")
        {
            DataSender.port = ConfigurationHelper.SignalRPort;
        }
        else
        {
            DataSender.port = ConfigurationHelper.StompServerPort;
        }

        Console.WriteLine("Kullanmak İstediğiniz Yöntemi Seçin.");
        Console.WriteLine("1) API");
        Console.WriteLine("2) MongoDB");
        if (Console.ReadLine() == "1")
        {
            await DataSender.SendAircraftDataFromAPIAsync(mongoDbManager, cts.Token);
        }
        else
        {
            await DataSender.SendAircraftDataFromDbAsync(mongoDbManager, cts.Token);
        }
       
        
 

    }
}


