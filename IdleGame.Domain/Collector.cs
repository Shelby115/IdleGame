namespace IdleGame.Domain;

public class Collector : ResourceProducer
{
    public override bool IsAutomatic => true;

    public Collector(string name, IResource resource, int quantity, TimeSpan cooldown, Func<IResourceProducer, IDictionary<string, int>> getUpgradeCosts)
        : base(name, resource, quantity, cooldown, getUpgradeCosts)
    {
        StartResourceProduction();
    }

    protected override void ProduceResource(object? state)
    {
        base.ProduceResource(state);
        // Collectors should automatically call StartResourceProduction() when the timer completes.
        StartResourceProduction();
    }
}