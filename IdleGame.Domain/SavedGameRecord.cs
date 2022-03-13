﻿namespace IdleGame.Domain;

public class SavedGame
{
    public IDictionary<string, long> Resources { get; set; }
    public IDictionary<string, SavedResourceProducer> ResourceProducers { get; set; }

    public long GetResourceQuantity(string resourceName, long defaultQuantity)
    {
        return Resources.TryGetValue(resourceName, out var quantity)
             ? quantity
             : defaultQuantity;
    }

    public SavedResourceProducer? GetSavedResourceProducer(string producerName)
    {
        return ResourceProducers.TryGetValue(producerName, out var savedProducer)
             ? savedProducer
             : null;
    }
}

public class SavedResourceProducer
{
    public string Name { get; set; }
    public string ResourceName { get; set; }
    public int Quantity { get; set; }
    public int TimesUpgraded { get; set; }
    public float Multiplier { get; set; }
    public bool IsAutomatic { get; set; }
}