namespace IdleGame.Domain;

public class Resource : IResource
{
    public string Name { get; set; }
    public long Quantity { get; set; }
    public int NegativePenaltyRateForOtherResources { get; set; }

    /// <summary>
    /// Fires when this resource is reduced below zero.
    /// Should be used to reduce other resources as a penalty for going negative.
    /// </summary>
    public event EventHandler<ResourcePenaltyEventArgs>? Penalized;

    public Resource() { }
    public Resource(string name, long quantity, int negativePenaltyRateForOtherResources)
    {
        Name = name?.Trim() ?? throw new ArgumentNullException(nameof(name));
        Quantity = quantity;
        NegativePenaltyRateForOtherResources = negativePenaltyRateForOtherResources;
    }

    /// <summary>
    /// Adds the amount to the resource's quantity.
    /// If it is to go negative, the resource's quantity is set to zero and the <see cref="Penalized"/> event is fired.
    /// </summary>
    /// <param name="quantity">Amount to add to the resource's quantity (allows negatives).</param>
    public void Add(long quantity)
    {
        var newQuantity = Quantity + quantity;
        if (newQuantity < 0)
        {
            Quantity = 0;
            if (NegativePenaltyRateForOtherResources > 0)
            {
                Penalized?.Invoke(this, new ResourcePenaltyEventArgs(this, newQuantity));
            }
        }
        else
        {
            Quantity = newQuantity;
        }
    }
}