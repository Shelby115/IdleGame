namespace IdleGame.Domain
{
    public interface ITechnology
    {
        string Description { get; }
        string Name { get; }
        bool HasBeenPurchased { get; }

        bool CanAfford(Resources resources);
        void Purchase(Resources resources, ResourceProducers producers);
    }
}