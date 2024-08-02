namespace SignalR.Configuration
{
    public static class ConfigurationHelper
    {
        private static IConfigurationRoot configuration = GetConfiguration();
        public static string serverIp = configuration["TCP:Adress"];
        public static string RabbitMQAdress = configuration["RabbitMQ:Adress"];
        public static string QueueName = configuration["RabbitMQ:Queue"];
        public static int port = int.Parse(configuration["TCP:Port"]);

        public static IConfigurationRoot GetConfiguration()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }
    }
}
