namespace IdleGame.Domain;

public class Resource : IResource
{
    public string Name { get; }
    public long Quantity { get; private set; }

    public Resource(string name, int quantity)
    {
        Name = name?.Trim() ?? throw new ArgumentNullException(nameof(name));
        Quantity = quantity;
    }

    public void Add(long quantity)
    {
        Quantity += quantity;
    }
}