using System;

namespace Zxs.Event
{
    public class GlobalSignalBus
    {
        private static readonly Lazy<SignalBus> _instance = new(() => new SignalBus());
        public static SignalBus Instance => _instance.Value;
    }
}