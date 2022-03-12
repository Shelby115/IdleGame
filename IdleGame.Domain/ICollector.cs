namespace IdleGame.Domain;

public interface ICollector : IResourceProducer
{
    string Name { get; }
    TimeSpan Interval { get; }

    void Upgrade();
}