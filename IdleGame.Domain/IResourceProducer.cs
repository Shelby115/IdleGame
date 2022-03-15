
namespace IdleGame.Domain
{
    public interface IResourceProducer
    {
        bool IsAutomatic { get; }
        float Multiplier { get; }
        string Name { get; }
        int Quantity { get; }
        int QuantityAddedOnUpgrade { get; }
        string ResourceName { get; }
        IDictionary<string, (int baseCost, int costMultiplier)> ResourceUpgradeCosts { get; }
        int TimesUpgraded { get; }
        bool CauseOtherAutomaticProducersToUpgradeOnUpgrade { get; }
        bool UpgradeWhenOtherAutomaticProducersUpgrade { get; }

        event EventHandler<ResourceProducedEventArgs>? ResourceProductionFinished;
        event EventHandler? ResourceProductionStarted;
        event EventHandler? Upgraded;

        bool CanAffordMultiplierUpgrade();
        bool CanAffordUpgradeCosts(Resources resources);
        int GetMultiplierUpgradeCost();
        int GetProductionQuantity();
        IDictionary<string, int> GetUpgradeCosts();
        void StartResourceProduction();
        bool Upgrade(Resources resources);
        void UpgradeMultiplier(float multiplierAmountToAdd);
        void UpgradeForFree();
        void UpgradeMultiplierForFree(float multiplierAmountToAdd);

        void LoadValues(SavedResourceProducer savedProducer);
    }
}