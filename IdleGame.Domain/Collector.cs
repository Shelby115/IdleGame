namespace IdleGame.Domain;

public class Collector : ICollector, IResourceProducer
{
    public string Name { get; }
    public TimeSpan Interval { get; }
    private Timer Timer { get; }
    public int ProductionQuantity { get; private set; }

    public event EventHandler<ResourceEventArgs>? ResourceProduced;

    public Collector(string name, TimeSpan interval, int productionQuantity)
    {
        Name = name?.Trim() ?? throw new ArgumentNullException(nameof(name));
        Interval = interval;
        Timer = new Timer(TimerCallback, null, TimeSpan.Zero, interval);
    }

    public void Upgrade()
    {
        ProductionQuantity += 1;
    }

    private void TimerCallback(object? state)
    {
        ResourceProduced?.Invoke(this, new ResourceEventArgs(ProductionQuantity));
    }
}