namespace IdleGame.Domain
{
    public interface ITechnology
    {
        string Description { get; }
        string Name { get; }
        bool HasBeenPurchased { get; set; }

        bool CanAfford(Resources resources);
        void Purchase(Resources resources, ResourceProducers producers);
    }
}