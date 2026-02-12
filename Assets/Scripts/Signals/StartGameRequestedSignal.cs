using ColorBlockJam.Models;

namespace ColorBlockJam.Signals
{
    public readonly struct StartGameRequestedSignal
    {
        public readonly LevelInfo LevelInfo;

        public StartGameRequestedSignal(LevelInfo levelInfo)
        {
            LevelInfo = levelInfo;
        }
    }
}