namespace IdleGame.Domain;

public class ResourcePenaltyEventArgs : EventArgs
{
    public string ResourceName { get; }
    public long Quantity { get; }
    public int NegativePenaltyRateForOtherResources { get; }
    public long PenaltyAmount { get; }

    public ResourcePenaltyEventArgs(IResource resource, long quantity)
    {
        ResourceName = resource.Name;
        Quantity = quantity;
        NegativePenaltyRateForOtherResources = resource.NegativePenaltyRateForOtherResources;
        PenaltyAmount = quantity * NegativePenaltyRateForOtherResources;
    }
}