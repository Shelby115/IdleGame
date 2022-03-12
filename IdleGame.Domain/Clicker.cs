namespace IdleGame.Domain;

public class Clicker : IClicker, IResourceProducer
{
    public string Name { get; }
    public TimeSpan CooldownDuration { get; }
    public DateTime LastClickedOn { get; private set; }
    public int ProductionQuantity { get; private set; }

    public event EventHandler<ResourceEventArgs>? ResourceProduced;

    private readonly Timer Timer;
    public event EventHandler? Clicked;
    public event EventHandler? OffCooldown;

    public Clicker(string name, TimeSpan cooldownDuration, int productionQuantity)
    {
        Name = name?.Trim() ?? throw new ArgumentNullException(nameof(name));
        CooldownDuration = cooldownDuration;
        LastClickedOn = DateTime.MinValue;
        ProductionQuantity = productionQuantity;
        Timer = new Timer(TimerCallback, null, Timeout.InfiniteTimeSpan, CooldownDuration);
    }

    public void Click()
    {
        var now = DateTime.UtcNow;
        LastClickedOn = now;
        ResourceProduced?.Invoke(this, new ResourceEventArgs(ProductionQuantity));
        Clicked?.Invoke(this, new EventArgs());
        Timer.Change(CooldownDuration, TimeSpan.Zero);
    }

    private void TimerCallback(object? state)
    {
        OffCooldown?.Invoke(this, EventArgs.Empty);
        Timer.Change(Timeout.InfiniteTimeSpan, CooldownDuration);
    }
}