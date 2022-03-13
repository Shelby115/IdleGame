namespace IdleGame.Domain;

public class ResourceProducedEventArgs : EventArgs
{
    public string ResourceName { get; }
    public int ProducedQuantity { get; }

    public ResourceProducedEventArgs(string resourceName, int producedQuantity)
    {
        ResourceName = resourceName;
        ProducedQuantity = producedQuantity;
    }
}