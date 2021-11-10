using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisChat
{
    public class RedisOptions
    {
        public string Host { get; set; }
        public ushort Port { get; set; }
        public string StreamName { get; set; }
    }
}
