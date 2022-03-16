namespace IdleGame.Domain;

public class SaveEventArgs : EventArgs
{
    public SavedGame SavedGame { get; }

    public SaveEventArgs(SavedGame savedGame)
    {
        SavedGame = savedGame;
    }
}