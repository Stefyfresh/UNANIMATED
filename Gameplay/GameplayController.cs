using Rhythm;

namespace UNANIMATED.Gameplay
{
    public static class GameplayController
    {
        public static bool screenShakeOverride;
        public static bool screenRotOverride;
        public static bool screenZoomOverride;

        public static void Reset()
        {
            screenShakeOverride = false;
            screenRotOverride = false;
            screenZoomOverride = false;
        }

        public static void ParseCommand(CommandEventInfo currentCommand)
        {
            // Get relevant variables
            GameplayOption type = System.Enum.Parse<GameplayOption>(currentCommand.GetStringParam(0));
            bool enabled = currentCommand.GetBoolParam(1, true);
            float time = currentCommand.Duration;

            // Logging
            UNANIMATED.Logger.LogInfo($"Parsed gameplay command at {currentCommand.Time} ms: {type} | params {currentCommand.ParamString} | length {time:0} ms");

            // Command logic
            switch (type)
            {
                case GameplayOption.Reset:
                    Reset();
                    break;
                case GameplayOption.ScreenShake:
                    screenShakeOverride = enabled;
                    break;
                case GameplayOption.ScreenRot:
                    screenRotOverride = enabled;
                    break;
                case GameplayOption.ScreenZoom:
                    screenZoomOverride = enabled;
                    break;

                    // default:
                    //     {
                    //         // Other mode
                    //         if (time >= 0)
                    //         {
                    //             // Command has a time
                    //             switch (type)
                    //             {

                    //             }

                    //         }
                    //         else
                    //         {
                    //             // Command is instant
                    //             switch (type)
                    //             {

                    //             }
                    //         }
                    //     }
                    //     break;
            }
        }
    }
}