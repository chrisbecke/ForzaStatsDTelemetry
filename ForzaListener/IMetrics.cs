using System;

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
    public void Unique(string name, int value, double rate = 1);
    public void Flush();
}
