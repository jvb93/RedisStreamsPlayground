namespace Consumer
{
    public interface IRedisStreamConsumer
    {
        void BeginRead();
        void BeginReadWithConsumerGroup();
    }
}
