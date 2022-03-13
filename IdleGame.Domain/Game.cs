namespace IdleGame.Domain;

public class Game
{
    public Resources Resources { get; private set; }
    public ResourceProducers Producers { get; private set; }
    public ICollection<ITechnology> Technologies { get; private set; }

    public bool IsDoneLoading = false;

    public void NewGame()
    {
        Resources = new Resources();
        Resources.Add(new Resource("Wood", DefaultWoodQuantity));
        Resources.Add(new Resource("Stone", DefaultStoneQuantity));
        Resources.Add(new Resource("Food", DefaultFoodQuantity));

        Producers = new ResourceProducers();
        Producers.Add(new Clicker("WoodClicker", Resources.Get("Wood"), 1, TimeSpan.Zero, CalculateUpgradeCost));
        Producers.Add(new Clicker("StoneClicker", Resources.Get("Stone"), 1, TimeSpan.Zero, CalculateUpgradeCost));
        Producers.Add(new Clicker("FoodClicker", Resources.Get("Food"), 1, TimeSpan.Zero, CalculateUpgradeCost));

        Producers.Add(new Collector("WoodCollector", Resources.Get("Wood"), 0, TimeSpan.FromSeconds(1), CalculateUpgradeCost));
        Producers.Add(new Collector("StoneCollector", Resources.Get("Stone"), 0, TimeSpan.FromSeconds(1), CalculateUpgradeCost));
        Producers.Add(new Collector("FoodCollector", Resources.Get("Food"), 0, TimeSpan.FromSeconds(1), CalculateUpgradeCost));

        Producers.Add(new Collector("FoodConsumer", Resources.Get("Food"), -1, TimeSpan.FromSeconds(1), CalculateUpgradeCost));
        Producers.Get("FoodConsumer").ResourceProductionFinished += ApplyPenaltyToOtherResourcesIfFoodIsNegative;

        Producers.Get("WoodCollector").Upgraded += IncreaseFoodConsumptionOnCollectorUpgrade;
        Producers.Get("StoneCollector").Upgraded += IncreaseFoodConsumptionOnCollectorUpgrade;

        Technologies = new List<ITechnology>();

        var basicEducationDesc = "+200% efficiency to all clickers and collectors, Wood: -10000, Stone: -10000, Food: -10000";
        var basicEducationCosts = new Dictionary<string, long>()
        {
            { "Wood", 10000 },
            { "Stone", 10000 },
            { "Food", 10000 }
        };
        Technologies.Add(new Technology("Basic Education", basicEducationDesc, false, basicEducationCosts, (IResourceProducer x) => x.Quantity > 0, (IResourceProducer x) => x.SetMultiplier(x.Multiplier + 2.0f)));


        IsDoneLoading = true;
    }

    public void LoadGame(SavedGame savedGame)
    {
        Resources = new Resources();
        Resources.Add(new Resource("Wood", savedGame.GetResourceQuantity("Wood", DefaultWoodQuantity)));
        Resources.Add(new Resource("Stone", savedGame.GetResourceQuantity("Stone", DefaultStoneQuantity)));
        Resources.Add(new Resource("Food", savedGame.GetResourceQuantity("Food", DefaultFoodQuantity)));

        Producers = new ResourceProducers();

        var woodClicker = savedGame.GetSavedResourceProducer("WoodClicker");
        Producers.Add(new Clicker("WoodClicker", Resources.Get("Wood"), woodClicker?.Quantity ?? 1, TimeSpan.Zero, CalculateUpgradeCost, woodClicker?.Multiplier ?? 1.0f, woodClicker?.TimesUpgraded));

        var stoneClicker = savedGame.GetSavedResourceProducer("StoneClicker");
        Producers.Add(new Clicker("StoneClicker", Resources.Get("Stone"), stoneClicker?.Quantity ?? 1, TimeSpan.Zero, CalculateUpgradeCost, stoneClicker?.Multiplier ?? 1.0f, stoneClicker?.TimesUpgraded));

        var foodClicker = savedGame.GetSavedResourceProducer("FoodClicker");
        Producers.Add(new Clicker("FoodClicker", Resources.Get("Food"), foodClicker?.Quantity ?? 1, TimeSpan.Zero, CalculateUpgradeCost, foodClicker?.Multiplier ?? 1.0f, foodClicker?.TimesUpgraded));

        var woodCollector = savedGame.GetSavedResourceProducer("WoodCollector");
        Producers.Add(new Collector("WoodCollector", Resources.Get("Wood"), woodCollector?.Quantity ?? 0, TimeSpan.FromSeconds(1), CalculateUpgradeCost, woodCollector?.Multiplier ?? 1.0f, woodCollector?.TimesUpgraded));

        var stoneCollector = savedGame.GetSavedResourceProducer("StoneCollector");
        Producers.Add(new Collector("StoneCollector", Resources.Get("Stone"), stoneCollector?.Quantity ?? 0, TimeSpan.FromSeconds(1), CalculateUpgradeCost, stoneCollector?.Multiplier ?? 1.0f, stoneCollector?.TimesUpgraded));

        var foodCollector = savedGame.GetSavedResourceProducer("FoodCollector");
        Producers.Add(new Collector("FoodCollector", Resources.Get("Food"), foodCollector?.Quantity ?? 0, TimeSpan.FromSeconds(1), CalculateUpgradeCost, foodCollector?.Multiplier ?? 1.0f, foodCollector?.TimesUpgraded));

        var foodConsumer = savedGame.GetSavedResourceProducer("FoodConsumer");
        Producers.Add(new Collector("FoodConsumer", Resources.Get("Food"), foodConsumer?.Quantity ?? -1, TimeSpan.FromSeconds(1), CalculateUpgradeCost, foodConsumer?.Multiplier ?? 1.0f, foodConsumer?.TimesUpgraded));
        Producers.Get("FoodConsumer").ResourceProductionFinished += ApplyPenaltyToOtherResourcesIfFoodIsNegative;

        Producers.Get("WoodCollector").Upgraded += IncreaseFoodConsumptionOnCollectorUpgrade;
        Producers.Get("StoneCollector").Upgraded += IncreaseFoodConsumptionOnCollectorUpgrade;

        Technologies = new List<ITechnology>();

        var basicEducationDesc = "+200% efficiency to all clickers and collectors, Wood: -10000, Stone: -10000, Food: -10000";
        var basicEducationCosts = new Dictionary<string, long>()
        {
            { "Wood", 10000 },
            { "Stone", 10000 },
            { "Food", 10000 }
        };
        var basicEducationPurchaseStatus = savedGame.GetTechnologyPurchasedStatus("Basic Education");
        Technologies.Add(new Technology("Basic Education", basicEducationDesc, basicEducationPurchaseStatus, basicEducationCosts, (IResourceProducer x) => x.Quantity > 0, (IResourceProducer x) => x.SetMultiplier(x.Multiplier + 2.0f)));

        IsDoneLoading = true;
    }

    public SavedGame AsSavedGame()
    {
        var resources = new Dictionary<string, long>();
        foreach (var resource in Resources)
        {
            resources.Add(resource.Name, resource.Quantity);
        }

        var resourceProducers = new Dictionary<string, SavedResourceProducer>();
        foreach (var producer in Producers)
        {
            resourceProducers.Add(producer.Name, new SavedResourceProducer()
            {
                Name= producer.Name,
                ResourceName = producer.ResourceName,
                Quantity = producer.Quantity,
                TimesUpgraded = producer.TimesUpgraded,
                Multiplier = producer.Multiplier,
                IsAutomatic = producer.IsAutomatic
            });
        }

        var technologies = new Dictionary<string, bool>();
        foreach (var technology in Technologies)
        {
            technologies.Add(technology.Name, technology.HasBeenPurchased);
        }

        return new SavedGame()
        {
            Resources = resources,
            ResourceProducers = resourceProducers,
            Technologies = technologies
        };
    }

    public ITechnology? GetTechnology(string technologyName)
    {
        return Technologies.FirstOrDefault(x => x.Name == technologyName);
    }

    public const int DefaultWoodQuantity = 10000;
    public const int DefaultStoneQuantity = 10000;
    public const int DefaultFoodQuantity = 25000;
    public const int ClickerPrimaryResourceMultiplier = 10;
    public const int ClickerSecondaryResourceMultiplier = 5;
    public const int CollectorPrimaryResourceMultiplier = 25;
    public const int CollectorSecondaryResourceMultiplier = 10;
    public const int CollectorPrimaryResourceAdded = 25;
    public const int CollectorSecondaryResourceAdded = 25;

    public IDictionary<string, int> CalculateUpgradeCost(IResourceProducer x)
    {
        return x.Name switch
        {
            "WoodClicker" => new Dictionary<string, int>()
                {
                    { "Wood", ClickerSecondaryResourceMultiplier * x.Quantity },
                    { "Stone", ClickerPrimaryResourceMultiplier * x.Quantity }
                },
            "StoneClicker" => new Dictionary<string, int>()
                {
                    { "Wood", ClickerPrimaryResourceMultiplier * x.Quantity },
                    { "Stone", ClickerSecondaryResourceMultiplier * x.Quantity }
                },
            "FoodClicker" => new Dictionary<string, int>()
                {
                    { "Wood", ClickerPrimaryResourceMultiplier * x.Quantity },
                    { "Stone", ClickerPrimaryResourceMultiplier * x.Quantity }
                },
            "WoodCollector" => new Dictionary<string, int>()
                {
                    { "Wood", CollectorSecondaryResourceMultiplier * x.TimesUpgraded + CollectorSecondaryResourceAdded },
                    { "Stone", CollectorPrimaryResourceMultiplier * x.TimesUpgraded + CollectorPrimaryResourceAdded }
                },
            "StoneCollector" => new Dictionary<string, int>()
                {
                    { "Wood", CollectorPrimaryResourceMultiplier * x.TimesUpgraded + CollectorPrimaryResourceAdded },
                    { "Stone", CollectorSecondaryResourceMultiplier * x.TimesUpgraded + CollectorSecondaryResourceAdded }
                },
            "FoodCollector" => new Dictionary<string, int>()
                {
                    { "Wood", CollectorSecondaryResourceMultiplier * x.TimesUpgraded + CollectorPrimaryResourceAdded },
                    { "Stone", CollectorSecondaryResourceMultiplier * x.TimesUpgraded + CollectorPrimaryResourceAdded }
                },
            _ => new Dictionary<string, int>()
                {
                    { "Wood", 0 },
                    { "Stone", 0 }
                },
        };
    }

    private void ApplyPenaltyToOtherResourcesIfFoodIsNegative(object? sender, ResourceProducedEventArgs e)
    {
        var producedResource = Resources.Get(e.ResourceName);
        var quantity = producedResource.Quantity;
        if (quantity < 0)
        {
            foreach (var resource in Resources)
            {
                if (resource.Name == producedResource.Name)
                {
                    resource.Add(-1 * quantity);
                }
                else
                {
                    var penaltyQuantity = 2 * quantity;
                    resource.Add(penaltyQuantity);
                }
            }
        }
    }

    private void IncreaseFoodConsumptionOnCollectorUpgrade(object? sender, EventArgs e)
    {
        var producer = sender as IResourceProducer;
        if (producer == null) { return; }

        if (producer.IsAutomatic && producer.ResourceName != "Food")
        {
            Producers.Get("FoodConsumer").Upgrade(Resources, -1);
        }
    }
}