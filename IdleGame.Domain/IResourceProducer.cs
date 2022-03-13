
namespace IdleGame.Domain
{
    public interface IResourceProducer
    {
        string Name { get; }
        string ResourceName { get; }
        int TimesUpgraded { get; }
        int Quantity { get; }
        float Multiplier { get; }
        bool IsAutomatic { get; }
        Func<IResourceProducer, IDictionary<string, int>> GetUpgradeCosts { get; }

        event EventHandler<ResourceProducedEventArgs>? ResourceProductionFinished;
        event EventHandler? ResourceProductionStarted;

        event EventHandler? Upgraded;

        int GetProductionQuantity();
        bool CanAffordUpgradeCosts(Resources resources);
        bool CanAffordUpgradeCosts(Resources resources, IDictionary<string, int> upgradeCosts);
        void StartResourceProduction();
        bool Upgrade(Resources resources, int quantity = 1);
        bool CanAffordMultiplierUpgrade();
        void UpgradeMultiplier(float multiplierAmountToAdd);
        int GetMultiplierUpgradeCost();
    }
}