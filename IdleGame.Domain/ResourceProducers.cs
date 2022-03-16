using System.Collections;

namespace IdleGame.Domain;

public class ResourceProducers : IEnumerable<IResourceProducer>
{
    private readonly IDictionary<string, IResourceProducer> _Producers;

    public ResourceProducers()
    {
        _Producers = new Dictionary<string, IResourceProducer>();
    }

    public ResourceProducers(IEnumerable<IResourceProducer> resourceProducers) : this()
    {
        foreach (var producer in resourceProducers)
        {
            Add(producer);
            if (producer.IsAutomatic && producer.CauseOtherAutomaticProducersToUpgradeOnUpgrade)
            {
                producer.Upgraded += UpgradeOtherAutomaticProducersOnUpgrade;
            }
        }
    }

    private void UpgradeOtherAutomaticProducersOnUpgrade(object? sender, EventArgs e)
    {
        if (sender is not IResourceProducer upgradedProducer) { return; }

        foreach (var producer in _Producers.Values)
        {
            if (producer.Name == upgradedProducer.Name) { continue; }
            if (producer.UpgradeWhenOtherAutomaticProducersUpgrade == false) { continue; }
            producer.UpgradeForFree();
        }
    }

    public IResourceProducer? Get(string name)
    {
        return _Producers.TryGetValue(name, out var producer)
             ? producer
             : null;
    }

    public int GetNetProductionPerSecond(string resourceName)
    {
        return _Producers.Values
            .Where(x => x.ResourceName == resourceName && x.IsAutomatic)
            .Sum(x => x.GetProductionQuantity());
    }

    public int GetProduction(string resourceName)
    {
        return _Producers.Values
                         .Where(x => x.ResourceName == resourceName && x.IsAutomatic)
                         .Where(x => x.Quantity > 0)
                         .Sum(x => x.GetProductionQuantity());
    }

    public int GetConsumption(string resourceName)
    {
        return _Producers.Values
                         .Where(x => x.ResourceName == resourceName && x.IsAutomatic)
                         .Where(x => x.Quantity < 0)
                         .Sum(x => x.GetProductionQuantity());
    }

    public void Add(IResourceProducer producer)
    {
        _Producers.Add(producer.Name, producer);
    }

    public IEnumerator<IResourceProducer> GetEnumerator()
    {
        return _Producers.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}