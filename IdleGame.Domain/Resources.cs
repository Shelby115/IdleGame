using System.Collections;

namespace IdleGame.Domain;

public class Resources : IEnumerable<IResource>
{
    private readonly IDictionary<string, IResource> _Resources;

    public Resources()
    {
        _Resources = new Dictionary<string, IResource>();
    }

    public Resources(IEnumerable<IResource> resources) : this()
    {
        foreach(var resource in resources)
        {
            _Resources.Add(resource.Name, resource);
            resource.Penalized += PenalizeOtherResources;
        }
    }

    public IResource? Get(string name)
    {
        return _Resources.TryGetValue(name, out var resource)
             ? resource
             : null;
    }

    public void SetResourceQuantity(string resourceName, long quantity)
    {
        _Resources[resourceName].Add(quantity - _Resources[resourceName].Quantity);
    }

    /// <summary>
    /// Subtracts the penalty amount from other resources when a resource fires the penalized event.
    /// </summary>
    private void PenalizeOtherResources(object? sender, ResourcePenaltyEventArgs e)
    {
        if ((e?.PenaltyAmount ?? 0) == 0) { return; }

        foreach (var resource in _Resources.Values)
        {
            if (resource.Name == e.ResourceName) { continue; }
            resource.Add(e.PenaltyAmount);
        }
    }

    public IEnumerator<IResource> GetEnumerator()
    {
        return _Resources.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}