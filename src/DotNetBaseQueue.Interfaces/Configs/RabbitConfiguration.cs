namespace DotNetBaseQueue.Interfaces.Configs
{
    public class RabbitConfiguration : RabbitInfoQueueConfiguration
    {
        public string HostName { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
    }
}