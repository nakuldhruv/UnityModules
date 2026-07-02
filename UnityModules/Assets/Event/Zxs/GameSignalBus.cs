namespace UnityModules.Event.Zxs
{
    public class GameSignalBus : SignalBus
    {
        private static GameSignalBus _instance;
        public static GameSignalBus Instance => _instance ??= new GameSignalBus();
    }
}