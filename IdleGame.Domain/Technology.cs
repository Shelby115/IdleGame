namespace IdleGame.Domain;

public class Technology : ITechnology
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool HasBeenPurchased { get; set; }
    private IDictionary<string, long> ResourceCosts { get; set; }
    private Func<IResourceProducer, bool> IsAffectedResourceProducer { get; set; }
    private Action<IResourceProducer> ApplyTechnologyToResourceProducer { get; set; }

    public Technology(string name, string description, bool hasBeenPurchased, IDictionary<string, long> resourceCosts, Func<IResourceProducer, bool> isAffectedResourceProducer, Action<IResourceProducer> applyTechnologyToResourceProducer)
    {
        Name = name;
        Description = description;
        HasBeenPurchased = hasBeenPurchased;
        ResourceCosts = resourceCosts;
        IsAffectedResourceProducer = isAffectedResourceProducer;
        ApplyTechnologyToResourceProducer = applyTechnologyToResourceProducer;
    }

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

        var affectedProducers = producers.Where(x => IsAffectedResourceProducer(x));
        foreach (var producer in affectedProducers)
        {
            ApplyTechnologyToResourceProducer(producer);
        }
    }
}