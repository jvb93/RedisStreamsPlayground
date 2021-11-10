namespace Consumer
{
    public class RedisOptions
    {
        public string Host { get; set; }
        public ushort Port { get; set; }
        public string StreamName { get; set; }
        public string ConsumerGroupName { get; set; }
    }
}
