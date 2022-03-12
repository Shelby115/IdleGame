namespace IdleGame.Domain;

public interface IResourceProducer
{
    int ProductionQuantity { get; }

    event EventHandler<ResourceEventArgs>? ResourceProduced;
}