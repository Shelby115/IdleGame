namespace IdleGame.Domain;

public class Technology : ITechnology
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool HasBeenPurchased { get; set; }
    public IDictionary<string, long> ResourceCosts { get; set; }
    public Func<IResourceProducer, bool> GetAffectedResourceProducer { get; set; }
    public Action<IResourceProducer> ApplyTechnologyToResourceProducer { get; set; }

    public bool CanAfford(Resources resources)
    {
        if (HasBeenPurchased) { return false; }

        foreach (var resourceCost in ResourceCosts)
        {
            var resource = resources.Get(resourceCost.Key);
            if (resource.Quantity < resourceCost.Value) { return false; }
        }

        return true;
    }

    public void Purchase(Resources resources, ResourceProducers producers)
    {
        if (CanAfford(resources) == false) { return; }

        HasBeenPurchased = true;

        foreach (var resourceCost in ResourceCosts)
        {
            var resource = resources.Get(resourceCost.Key);
            resource.Add(-1 * resourceCost.Value);
        }

        var affectedProducers = producers.Where(x => GetAffectedResourceProducer(x));
        foreach (var producer in affectedProducers)
        {
            ApplyTechnologyToResourceProducer(producer);
        }
    }
}