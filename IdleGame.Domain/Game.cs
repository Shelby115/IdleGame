namespace IdleGame.Domain;

public class Game
{
    private static readonly TimeSpan AutosaveInterval = TimeSpan.FromSeconds(30);
    private readonly Timer AutosaveTimer;
    private DateTime LastSavedOn = DateTime.UtcNow;

    public Resources Resources { get; private set; }
    public ResourceProducers Producers { get; private set; }
    public Technologies Technologies { get; private set; }

    public event EventHandler<SaveEventArgs>? Saved;
    public event EventHandler<OfflineProductionEventArgs>? ResourcesProducedWhileOffline;

    public bool IsDoneLoading = false;

    public Game()
    {
        AutosaveTimer = new Timer(Autosave, null, AutosaveInterval, AutosaveInterval);

        // Initialize resources.
        Resources = new Resources(new List<Resource>()
        {
            new Resource("Wood", 10000, 0),
            new Resource("Stone", 10000, 0),
            new Resource("Food", 2500, 2)
        });

        // Intialize resource producers/consumers.
        var resourceCosts = new Dictionary<string, IDictionary<string, (int baseCost, int costMultiplier)>>()
        {
            {
                "WoodClicker",
                new Dictionary<string, (int baseCost, int costMultiplier)>
                {
                    { "Wood", (0, 5) },
                    { "Stone", (0, 10) }
                }
            },
            {
                "StoneClicker",
                new Dictionary<string, (int baseCost, int costMultiplier)>
                {
                    { "Wood", (0, 10) },
                    { "Stone", (0, 5) }
                }
            },
            {
                "FoodClicker",
                new Dictionary<string, (int baseCost, int costMultiplier)>
                {
                    { "Wood", (0, 5) },
                    { "Stone", (0, 5) }
                }
            },
            {
                "WoodCollector",
                new Dictionary<string, (int baseCost, int costMultiplier)>
                {
                    { "Wood", (25, 10) },
                    { "Stone", (25, 15) }
                }
            },
            {
                "StoneCollector",
                new Dictionary<string, (int baseCost, int costMultiplier)>
                {
                    { "Wood", (25, 15) },
                    { "Stone", (25, 10) }
                }
            },
            {
                "FoodCollector",
                new Dictionary<string, (int baseCost, int costMultiplier)>
                {
                    { "Wood", (25, 10) },
                    { "Stone", (25, 10) }
                }
            }
        };

        var clickers = Resources.Select(x => new Clicker($"{x.Name}Clicker", x, 1, TimeSpan.Zero) { ResourceUpgradeCosts = resourceCosts[$"{x.Name}Clicker"] });
        var collectors = Resources.Select(x => new Collector($"{x.Name}Collector", x, 0, TimeSpan.FromSeconds(1))
        {
            ResourceUpgradeCosts = resourceCosts[$"{x.Name}Collector"],
            CauseOtherAutomaticProducersToUpgradeOnUpgrade = x.Name != "Food"
        });
        var consumer = new Collector("FoodConsumer", Resources.Get("Food"), -1, TimeSpan.FromSeconds(1))
        {
            QuantityAddedOnUpgrade = -1,
            UpgradeWhenOtherAutomaticProducersUpgrade = true
        };

        var producers = new List<IResourceProducer>();
        producers.AddRange(clickers);
        producers.AddRange(collectors);
        producers.Add(consumer);

        Producers = new ResourceProducers(producers);

        // Intialize technologies.
        Technologies = new Technologies(new List<ITechnology>()
        {
            new Technology()
            {
                Name = "Basic Education",
                Description = "+200% efficiency to all clickers and collectors, Wood: -10000, Stone: -10000, Food: -10000",
                ResourceCosts = new Dictionary<string, long>()
                {
                    { "Wood", 10000 },
                    { "Stone", 10000 },
                    { "Food", 10000 }
                },
                GetAffectedResourceProducer = (IResourceProducer x) => x.Quantity > 0,
                ApplyTechnologyToResourceProducer = (IResourceProducer x) => x.UpgradeMultiplierForFree(2.0f)
            }
        });

        IsDoneLoading = true;
    }

    public void LoadGame(SavedGame savedGame)
    {
        LastSavedOn = savedGame.LastSavedOn;

        // Load resource quantities.
        foreach (var resource in savedGame.Resources)
        {
            if (Resources.Get(resource.Key) == null) { continue; }
            Resources.SetResourceQuantity(resource.Key, resource.Value);
        }

        // Load resource producer values.
        foreach (var savedProducer in savedGame.ResourceProducers.Values)
        {
            var producer = Producers.Get(savedProducer.Name);
            if (producer == null) { continue; }
            producer.LoadValues(savedProducer);
        }

        // Load technology purchase status.
        foreach (var technologyStatus in savedGame.Technologies)
        {
            var technology = Technologies.FirstOrDefault(x => x.Name == technologyStatus.Key);
            if (technology == null) { continue; }
            technology.HasBeenPurchased = technologyStatus.Value;
        }

        // Calculate, announce, and add resources produced offline.
        var secondsOffline = CalculateSecondsOffline();
        var resourcesProducedOffline = CalculateResourcesProducedWhileOffline(secondsOffline);
        if (resourcesProducedOffline != null && resourcesProducedOffline.Values.Sum() > 0)
        {
            ResourcesProducedWhileOffline?.Invoke(this, new OfflineProductionEventArgs(resourcesProducedOffline, secondsOffline));
            foreach (var resources in resourcesProducedOffline)
            {
                Resources.Get(resources.Key)?.Add(resources.Value);
            }
        }

        IsDoneLoading = true;
    }

    private long CalculateSecondsOffline()
    {
        var now = DateTime.UtcNow;
        var timeDifference = now - LastSavedOn;
        return (long)timeDifference.TotalSeconds;
    }
    private IDictionary<string, long> CalculateResourcesProducedWhileOffline(long secondsOffline)
    {
        if (secondsOffline > 0)
        {
            return Resources.ToDictionary(
                x => x.Name,
                x => Producers.GetNetProductionPerSecond(x.Name) * secondsOffline
            );
        }

        return null;
    }

    public SavedGame AsSavedGame()
    {
        return new SavedGame()
        {
            LastSavedOn = this.LastSavedOn,
            Resources = Resources.ToDictionary(x => x.Name, x => x.Quantity),
            Technologies = Technologies.ToDictionary(x => x.Name, x => x.HasBeenPurchased),
            ResourceProducers = Producers.ToDictionary(x => x.Name, x => new SavedResourceProducer()
            {
                Name = x.Name,
                Quantity = x.Quantity,
                TimesUpgraded = x.TimesUpgraded,
                Multiplier = x.Multiplier
            })
        };
    }

    public void Save()
    {
        LastSavedOn = DateTime.UtcNow;
        var args = new SaveEventArgs(this.AsSavedGame());
        Saved?.Invoke(this, args);
    }

    private void Autosave(object? state)
    {
        Save();
    }
}