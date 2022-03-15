namespace IdleGame.Domain;

public class Clicker : ResourceProducer
{
    public Clicker(string name, IResource resource, int quantity, TimeSpan cooldown)
        : base(name, resource, quantity, cooldown)
    {

    }

    protected override void ProduceResource(object? state)
    {
        base.ProduceResource(state);
        // Clickers should wait for the StartResourceProduction() to be called manually.
        Timer.Change(Timeout.InfiniteTimeSpan, TimeSpan.Zero);
    }

    public override string ToString()
    {
        return this.Name;
    }
}