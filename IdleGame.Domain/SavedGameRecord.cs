﻿namespace IdleGame.Domain;

public class SavedGame
{
    public DateTime LastSavedOn { get; set; }
    public IDictionary<string, long> Resources { get; set; }
    public IDictionary<string, SavedResourceProducer> ResourceProducers { get; set; }
    public IDictionary<string, bool> Technologies { get; set; }

    public long GetResourceQuantity(string resourceName, long defaultQuantity)
    {
        return Resources.TryGetValue(resourceName, out var quantity)
             ? quantity
             : defaultQuantity;
    }

    public SavedResourceProducer? GetSavedResourceProducer(string producerName)
    {
        if (ResourceProducers == null) { return null; }
        return ResourceProducers.TryGetValue(producerName, out var savedProducer)
             ? savedProducer
             : null;
    }

    public bool GetTechnologyPurchasedStatus(string technologyName)
    {
        if (Technologies == null) { return false; }
        return Technologies.TryGetValue(technologyName, out var status)
             ? status
             : false;
    }
}

public class SavedResourceProducer
{
    public string Name { get; set; }
    public int Quantity { get; set; }
    public int TimesUpgraded { get; set; }
    public float Multiplier { get; set; }
}