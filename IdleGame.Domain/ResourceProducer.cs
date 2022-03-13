namespace IdleGame.Domain;

public class ResourceProducer : IResourceProducer
{
    /// <summary>
    /// Name of this resource producer.
    /// </summary>
    public string Name { get; }

    public string ResourceName => Resource.Name;
    public virtual bool IsAutomatic => false;

    /// <summary>
    /// The number of times this resource producer has been upgraded.
    /// </summary>
    public int TimesUpgraded { get; private set; }

    /// <summary>
    /// The amount of a resource to be produced.
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// The resource that is produced by this resource producer.
    /// </summary>
    protected readonly IResource Resource;

    /// <summary>
    /// The time inteval between productions of the resource.
    /// </summary>
    public readonly TimeSpan Cooldown;

    protected DateTime LastProducedOn;
    public Func<IResourceProducer, IDictionary<string, int>> GetUpgradeCosts { get; }

    private const int MultiplierQuantityCost = 1;
    public float Multiplier { get; private set; }

    protected readonly Timer Timer;

    /// <summary>
    /// Fires when the resource producer starts its cooldown interval.
    /// </summary>
    public event EventHandler? ResourceProductionStarted;

    /// <summary>
    /// Fires when the resource producer finishes its cooldown interval and produces resources.
    /// </summary>
    public event EventHandler<ResourceProducedEventArgs>? ResourceProductionFinished;

    /// <summary>
    /// Fires when the resource producer is upgraded.
    /// </summary>
    public event EventHandler? Upgraded;

    internal ResourceProducer(string name, IResource resource, int quantity, TimeSpan cooldown, Func<IResourceProducer, IDictionary<string, int>> getUpgradeCosts, float? multiplier = null, int? timesUpgraded = null)
    {
        Name = name?.Trim() ?? throw new ArgumentNullException(nameof(name));
        Resource = resource;
        Quantity = quantity;
        TimesUpgraded = timesUpgraded ?? 0;
        Multiplier = multiplier ?? 1.0f;
        Cooldown = cooldown;
        LastProducedOn = DateTime.MinValue;
        GetUpgradeCosts = getUpgradeCosts;
        Timer = new Timer(ProduceResource, null, Timeout.InfiniteTimeSpan, Cooldown);
    }

    /// <summary>
    /// Upgrades the resource producer if it can be afforded.
    /// </summary>
    /// <returns>True = The upgrade was purchased. False = The upgrade was not purchased.</returns>
    public bool Upgrade(Resources resources, int quantity = 1)
    {
        var costs = GetUpgradeCosts(this);
        var canAffordUpgradeCosts = CanAffordUpgradeCosts(resources, costs);
        if (canAffordUpgradeCosts == false) { return false; }

        foreach (var cost in costs)
        {
            resources.Get(cost.Key).Add(-1 * cost.Value);
        }

        Quantity += quantity;
        TimesUpgraded += 1;
        Upgraded?.Invoke(this, EventArgs.Empty);

        return true;
    }

    /// <summary>
    /// Checks if the upgradeCosts can be afforded with the provided resources.
    /// </summary>
    /// <param name="resources">Resources available to spend on upgrade.</param>
    /// <param name="upgradeCosts">The quantity (value) cost of each upgrade by name (key).</param>
    /// <returns>True = Can afford the upgrade cost. False = Cannot afford the upgrade costs.</returns>
    public bool CanAffordUpgradeCosts(Resources resources, IDictionary<string, int> upgradeCosts)
    {
        foreach (var cost in upgradeCosts)
        {
            var resource = resources.Get(cost.Key);
            if (resource == null) { return false; }
            if (resource.Quantity < cost.Value) { return false; }
        }

        return true;
    }

    /// <summary>
    /// Checks if the upgradeCosts can be afforded with the provided resources.
    /// </summary>
    /// <param name="resources">Resources available to spend on upgrade.</param>
    /// <returns>True = Can afford the upgrade cost. False = Cannot afford the upgrade costs.</returns>
    public bool CanAffordUpgradeCosts(Resources resources)
    {
        var upgradeCosts = GetUpgradeCosts(this);
        foreach (var cost in upgradeCosts)
        {
            var resource = resources.Get(cost.Key);
            if (resource == null) { return false; }
            if (resource.Quantity < cost.Value) { return false; }
        }

        return true;
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

    public int GetProductionQuantity()
    {
        return (int)(Quantity * Multiplier);
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

    public int GetMultiplierUpgradeCost()
    {
        return (int)Multiplier * MultiplierQuantityCost;
    }

    public bool CanAffordMultiplierUpgrade()
    {
        return Quantity > GetMultiplierUpgradeCost();
    }

    public void UpgradeMultiplier(float multiplierAmountToAdd)
    {
        if (CanAffordMultiplierUpgrade())
        {
            Quantity -= GetMultiplierUpgradeCost();
            Multiplier += multiplierAmountToAdd;
        }
    }
}