namespace IdleGame.Domain;

public interface IClicker : IResourceProducer
{
    TimeSpan CooldownDuration { get; }
    DateTime LastClickedOn { get; }
    string Name { get; }

    public event EventHandler? Clicked;
    public event EventHandler? OffCooldown;

    void Click();
}