using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ForzaListner
{
    public interface IMetrics
    {
        public void Gauge(string name, uint value);
        public void Incr(string name);
        public void IncrBy(string name, int count);
        public void Decr(string name);
        public void DecrBy(string name, int count);
        public void Duration(string name, TimeSpan duration);
        public void Histogram(string name, int value);
        public void Annotate(string name, string format);
        public void Unique(string name, int value, double rate=1);
        public void Flush();
    }

    public class StatsDConfig
    {
        public int port { get; set; } = 8125;
        public string address { get; set; } = "127.0.0.1";
        public int mtu { get; set; } = 512;
        public string prefix { get; set; } = "acme.";
    }

    class StatsDService : IMetrics
    {
        StatsDConfig config;
        UdpClient server;
        Random random = new Random();
        StringBuilder buf = new StringBuilder();

        public StatsDService(StatsDConfig options)
        {
            config = options;

            server = new UdpClient();
            var addressList = Dns.GetHostAddresses(config.address);

            var endPoint = new IPEndPoint(addressList[0], config.port);

            server.Connect(endPoint);
        }

        public void Flush()
        {
            byte[] send_buffer = Encoding.ASCII.GetBytes(buf.ToString());
            buf.Clear();
            server.Send(send_buffer, send_buffer.Length);
        }

        public void Increment(string name, int count, float rate)
        {
            Send(name, rate, $"{count}|c");
        }

        public void Incr(string name)
        {
            Increment(name, 1, 1);
        }

        public void IncrBy(string name, int n)
        {
            Increment(name, n, 1);
        }

        public void Decrement(string name, int count, float rate)
        {
            Increment(name, -count, rate);
        }

        public void Decr(string name)
        {
            Increment(name, -1, 1);
        }

        public void DecrBy(string name, int value)
        {
            Increment(name, -value, 1);
        }

        public void Duration(string name, TimeSpan duration)
        {
            Send(name, 1, $"{duration.TotalMilliseconds}|ms");
        }

        public void Histogram(string name, int value)
        {
            Send(name, 1, $"{value}|ms");
        }

        public void Gauge(string name, UInt32 value)
        {
            Send(name, 1, $"{value}|g");
        }

        public void Annotate(string name, string value)
        {
            Send(name, 1, $"{value}|a");
        }

        public void Unique(string name, int value, double rate)
        {
            Send(name, rate, $"{value}|s");
        }

        public void Send(string stat, double rate, string format)
        {
            if(!string.IsNullOrWhiteSpace(config.prefix))
            {
                stat = config.prefix + stat;
            }

            if (rate < 1)
            {
                if (random.NextDouble() < rate)
                {
                    format = $"{format}@{rate}";
                }
                else return;
            }

            if(buf.Length + format.Length > config.mtu)
            {
                Flush();
            }

            if (buf.Length > 0)
                buf.Append("\n");

            buf.Append($"{stat}:{format}");
        }
    }
}
