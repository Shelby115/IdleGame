namespace IdleGame.Domain;

public class ResourceProducer : IResourceProducer
{
    /// <summary>
    /// Name of this resource producer.
    /// </summary>
    public string Name { get; }
    public virtual bool IsAutomatic => false;

    public string ResourceName => Resource.Name;
    protected readonly IResource Resource;
    public int Quantity { get; set; }

    public readonly TimeSpan Cooldown;
    protected DateTime LastProducedOn;
    protected readonly Timer Timer;

    public int QuantityAddedOnUpgrade { get; set; }
    public bool CauseOtherAutomaticProducersToUpgradeOnUpgrade { get; set; }
    public bool UpgradeWhenOtherAutomaticProducersUpgrade { get; set; }
    public int TimesUpgraded { get; set; }
    public IDictionary<string, (int baseCost, int costMultiplier)> ResourceUpgradeCosts { get; set; }

    private const int MultiplierQuantityCost = 1;
    public float Multiplier { get; private set; }

    public event EventHandler? ResourceProductionStarted;
    public event EventHandler<ResourceProducedEventArgs>? ResourceProductionFinished;
    public event EventHandler? Upgraded;

    internal ResourceProducer(string name, IResource resource, int quantity, TimeSpan cooldown)
    {
        Name = name?.Trim() ?? throw new ArgumentNullException(nameof(name));
        Resource = resource;
        Quantity = quantity;

        Cooldown = cooldown;
        LastProducedOn = DateTime.MinValue;
        Timer = new Timer(ProduceResource, null, Timeout.InfiniteTimeSpan, Cooldown);

        QuantityAddedOnUpgrade = 1;
        UpgradeWhenOtherAutomaticProducersUpgrade = false;
        TimesUpgraded = 0;
        ResourceUpgradeCosts = new Dictionary<string, (int baseCost, int costMultiplier)>();

        Multiplier = 1.0f;
    }

    /// <summary>
    /// Calculates the quantity of resource to be produced per cooldown.
    /// </summary>
    public int GetProductionQuantity()
    {
        return (int)Math.Round(Quantity * Multiplier, 0);
    }

    /// <summary>
    /// Kicks off the resource production cooldown timer.
    /// At the end of the timer the resources will be produced.
    /// </summary>
    /// <returns>True = Resources were produced. False = Producer was on cooldown, no resources produced.</returns>
    public void StartResourceProduction()
    {
        if (DateTime.UtcNow >= LastProducedOn.Add(Cooldown))
        {
            Timer.Change(Cooldown, Cooldown);
            ResourceProductionStarted?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Produces resources at the end of the resource production cooldown timer.
    /// </summary>
    protected virtual void ProduceResource(object? state)
    {
        LastProducedOn = DateTime.UtcNow;
        var quantity = GetProductionQuantity();
        Resource.Add(quantity);
        ResourceProductionFinished?.Invoke(this, new ResourceProducedEventArgs(Resource.Name, quantity));
    }


    /// <summary>
    /// Retrieves the cost per relevant resource to upgrade this resource producer.
    /// </summary>
    public IDictionary<string, int> GetUpgradeCosts()
    {
        return ResourceUpgradeCosts.ToDictionary(
            keySelector: x => x.Key,
            elementSelector: x => x.Value.baseCost + x.Value.costMultiplier * TimesUpgraded
        );
    }

    /// <summary>
    /// Checks if the upgradeCosts can be afforded with the provided resources.
    /// </summary>
    /// <param name="resources">Resources available to spend on upgrade.</param>
    /// <returns>True = Can afford the upgrade cost. False = Cannot afford the upgrade costs.</returns>
    public bool CanAffordUpgradeCosts(Resources resources)
    {
        var upgradeCosts = GetUpgradeCosts();
        foreach (var cost in upgradeCosts)
        {
            var resource = resources.Get(cost.Key);
            if (resource == null) { return false; }
            if (resource.Quantity < cost.Value) { return false; }
        }

        return true;
    }

    /// <summary>
    /// Upgrades the resource producer if it can be afforded.
    /// </summary>
    /// <returns>True = The upgrade was purchased. False = The upgrade was not purchased.</returns>
    public bool Upgrade(Resources resources)
    {
        var costs = GetUpgradeCosts();
        var canAffordUpgradeCosts = CanAffordUpgradeCosts(resources);
        if (canAffordUpgradeCosts == false) { return false; }

        foreach (var cost in costs)
        {
            resources.Get(cost.Key).Add(-1 * cost.Value);
        }

        Quantity += QuantityAddedOnUpgrade;
        TimesUpgraded += 1;
        Upgraded?.Invoke(this, EventArgs.Empty);

        return true;
    }

    /// <summary>
    /// Upgrades the resource producer without cost. Increments the <see cref="TimesUpgraded"/> counter, but does not fire the <see cref="Upgraded"/> event.
    /// </summary>
    public void UpgradeForFree()
    {
        Quantity += QuantityAddedOnUpgrade;
        TimesUpgraded += 1;
    }


    /// <summary>
    /// Calculates the cost for a multiplier upgrade.
    /// </summary>
    public int GetMultiplierUpgradeCost()
    {
        return (int)Multiplier * MultiplierQuantityCost;
    }

    /// <summary>
    /// Checks if an upgrade to the resource proceduer's multiplier can be afforded.
    /// </summary>
    public bool CanAffordMultiplierUpgrade()
    {
        return Quantity > GetMultiplierUpgradeCost();
    }

    /// <summary>
    /// Upgrades the resource producer's multiplier by the specified amount.
    /// </summary>
    public void UpgradeMultiplier(float multiplierAmountToAdd)
    {
        if (CanAffordMultiplierUpgrade())
        {
            Quantity -= GetMultiplierUpgradeCost();
            Multiplier += multiplierAmountToAdd;
        }
    }

    /// <summary>
    /// Upgrades the resource producer's multiplier by the specified amount without a cost.
    /// </summary>
    public void UpgradeMultiplierForFree(float multiplierAmountToAdd)
    {
        Multiplier += multiplierAmountToAdd;
    }

    /// <summary>
    /// Sets the values based on the saved resource producer.
    /// </summary>
    public virtual void LoadValues(SavedResourceProducer savedProducer)
    {
        this.Quantity = savedProducer.Quantity;
        this.TimesUpgraded = savedProducer.TimesUpgraded;
        this.Multiplier = savedProducer.Multiplier;
    }

    public override string ToString()
    {
        return this.Name;
    }
}