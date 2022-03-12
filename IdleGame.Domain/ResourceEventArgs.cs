namespace IdleGame.Domain;

public class ResourceEventArgs : EventArgs
{
    public int ProducedQuantity { get; }

    public ResourceEventArgs(int producedQuantity)
    {
        ProducedQuantity = producedQuantity;
    }
}