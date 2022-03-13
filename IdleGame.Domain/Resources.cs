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
            Add(resource);
        }
    }

    public IResource Get(string name)
    {
        return _Resources[name];
    }

    public void Add(IResource resource)
    {
        _Resources.Add(resource.Name, resource);
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