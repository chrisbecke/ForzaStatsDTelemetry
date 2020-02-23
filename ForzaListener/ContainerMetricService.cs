using System;
using System.Text;

namespace ForzaListener
{
    public class ContainerMetricService : IMetrics
    {

        static readonly string prefix = "forza";
        static readonly int mtu = 512;

        public ContainerMetricService()
        {
        }

        Random random = new Random();
        StringBuilder buf = new StringBuilder();

        public void Flush()
        {
            Console.Write(buf);
            buf.Clear();
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
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                stat = prefix + stat;
            }

            if (rate < 1)
            {
                if (random.NextDouble() < rate)
                {
                    format = $"{format}@{rate}";
                }
                else return;
            }

            if (buf.Length + format.Length > mtu)
            {
                Flush();
            }

            if (buf.Length > 0)
                buf.Append("\n");

            buf.Append($"{{ '{stat}': {format} }}");
        }
    }
}
