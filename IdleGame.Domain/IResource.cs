namespace IdleGame.Domain
{
    public interface IResource
    {
        string Name { get; }
        string ImageName { get; }
        long Quantity { get; }
        int NegativePenaltyRateForOtherResources { get; }

        event EventHandler<ResourcePenaltyEventArgs>? Penalized;

        void Add(long quantity);
    }
}