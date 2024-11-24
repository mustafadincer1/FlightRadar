using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace AppClient7.Configuration
{
    public static class ConfigurationHelper
    {
        private static IConfigurationRoot configuration = GetConfiguration();
        public static string JsonFilePath = configuration["Logging:JsonFilePath"];
        public static string TextFilePath = configuration["Logging:TextFilePath"];
        public static string apiUrl = configuration["API:Adress"];

        public static string mongoConnectionString = configuration["MongoDB:ConnectionString"];
        public static string mongoDbName = configuration["MongoDB:DatabaseName"];
        public static string mongoCollectionName = configuration["MongoDB:CollectionName"];
        public static string serverIp = configuration["TCP:Adress"];
        public static int SignalRPort = int.Parse(configuration["TCP:SignalRPort"]);
        public static int StompServerPort = int.Parse(configuration["TCP:StompServerPort"]);

        public static string JsonDataPath = configuration["JsonData:Path"];

        public static string Type = configuration["Data:Type"];
        public static string Status = configuration["Data:Status"];
        public static string DataType = configuration["Data:DataType"];
        public static string DeviceUnit = configuration["Data:DeviceUnit"];
        public static string TimeZone = configuration["Data:TimeZone"];

        public static IConfigurationRoot GetConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }
    }
}

//string jsonData = File.ReadAllText("C:\\Users\\staj\\Desktop\\New folder (2)\\aircraftlist1.52.json");

//var configuration = ConfigurationHelper.GetConfiguration();

//string JsonFilePath = configuration["Logging:JsonFilePath"];
//string TextFilePath = configuration["Logging:TextFilePath"];
//string apiUrl = configuration["API:Adress"];

//var mongoConnectionString = configuration["MongoDB:ConnectionString"];
//var mongoDbName = configuration["MongoDB:DatabaseName"];
//var mongoCollectionName = configuration["MongoDB:CollectionName"];
