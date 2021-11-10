using System;
using System.Linq;
using System.Threading;
using StackExchange.Redis;

namespace Consumer
{
    public class RedisStreamConsumer : IRedisStreamConsumer
    {
        private readonly ConnectionMultiplexer _redisMux;
        private readonly RedisOptions _redisOptions;
        public RedisStreamConsumer(ConnectionMultiplexer redisMux, RedisOptions redisOptions)
        {
            _redisMux = redisMux;
            _redisOptions = redisOptions;
        }

        public void BeginRead()
        {
            var db = _redisMux.GetDatabase();
            Console.WriteLine("Catching up!");
            var results = db.StreamRead(_redisOptions.StreamName, "0-0");
            WriteStreamContent(results);
            var lastReceived = results.Last().Id;

            while (true)
            {
                results = db.StreamRead(_redisOptions.StreamName, lastReceived);
                if (results.Any())
                {
                    WriteStreamContent(results);
                    lastReceived = results.Last().Id;
                }
                Thread.Sleep(1000);
            }
        }

        public void BeginReadWithConsumerGroup()
        {
            var db = _redisMux.GetDatabase();
            Console.WriteLine("Catching up!");
            var results = db.StreamRead(_redisOptions.StreamName, "0-0");
            WriteStreamContent(results);


            try
            {
                db.StreamCreateConsumerGroup(_redisOptions.StreamName, _redisOptions.ConsumerGroupName,
                    StreamPosition.NewMessages);
            }
            catch (RedisServerException redisServerException)
            {

            }


            var consumerName = Guid.NewGuid().ToString();
            while (true)
            {
                results = db.StreamReadGroup(_redisOptions.StreamName, _redisOptions.ConsumerGroupName, consumerName,
                    ">", count:100);
                if (results.Any())
                {
                    WriteAndAcknowledgeStreamContent(results, db);
                }

                Thread.Sleep(1000);
            }
        }

        private void WriteStreamContent(StreamEntry[] streamEntries)
        {
            foreach (var streamEntry in streamEntries)
            {
                Console.WriteLine($"{streamEntry.Id} - {streamEntry.Values.First().Name}: {streamEntry.Values.First().Value}");
            }
        }
        private void WriteAndAcknowledgeStreamContent(StreamEntry[] streamEntries, IDatabase db)
        {
            foreach (var streamEntry in streamEntries)
            {
                Console.WriteLine($"{streamEntry.Id} - {streamEntry.Values.First().Name}: {streamEntry.Values.First().Value}");
               
            }

            db.StreamAcknowledge(_redisOptions.StreamName, _redisOptions.ConsumerGroupName,
                streamEntries.Select(x => x.Id).ToArray());
        }
    }
}