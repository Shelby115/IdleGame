namespace IdleGame.Domain
{
    public class SaveEventArgs : EventArgs
    {
        public SavedGame SavedGame { get; private set; }
        public SaveEventArgs(SavedGame savedGame)
        {
            SavedGame = savedGame;
        }
    }
}