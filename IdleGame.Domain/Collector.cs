namespace IdleGame.Domain;

public class Collector : ResourceProducer
{
    public override bool IsAutomatic => true;

    public Collector(string name, IResource resource, int quantity, TimeSpan cooldown)
        : base(name, resource, quantity, cooldown)
    {
        StartResourceProduction();
    }

    protected override void ProduceResource(object? state)
    {
        base.ProduceResource(state);
        // Collectors should automatically call StartResourceProduction() when the timer completes.
        StartResourceProduction();
    }

    public override string ToString()
    {
        return this.Name;
    }
}