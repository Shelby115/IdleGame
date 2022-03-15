using System.Collections;

namespace IdleGame.Domain;

public class Technologies : IEnumerable<ITechnology>
{
    private readonly IDictionary<string, ITechnology> _Technologies;

    public Technologies()
    {
        _Technologies = new Dictionary<string, ITechnology>();
    }

    public Technologies(IEnumerable<ITechnology> Technologies) : this()
    {
        foreach (var technology in Technologies)
        {
            _Technologies.Add(technology.Name, technology);
        }
    }

    public ITechnology? Get(string name)
    {
        return _Technologies.TryGetValue(name, out var technology)
             ? technology
             : null;
    }

    public IEnumerator<ITechnology> GetEnumerator()
    {
        return _Technologies.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}