namespace IdleGame.Domain;

public class OfflineProductionEventArgs : EventArgs
{
    public long SecondsOffline { get; }
    public IDictionary<string, long> ResourcesProducedWhileOffline { get; }

    public OfflineProductionEventArgs(IDictionary<string, long> resourcesProducedWhileOffline, long secondsOffline)
    {
        ResourcesProducedWhileOffline = resourcesProducedWhileOffline;
        SecondsOffline = secondsOffline;
    }
}