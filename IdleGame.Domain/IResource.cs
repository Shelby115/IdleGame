namespace IdleGame.Domain
{
    public interface IResource
    {
        string Name { get; }
        long Quantity { get; }

        void Add(long quantity);
    }
}