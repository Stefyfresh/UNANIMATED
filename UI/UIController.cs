using System.Linq;

namespace UNANIMATED.UI
{
    public static class UIController
    {
        public static bool forceLockedUI;

        public static void Reset()
        {
            forceLockedUI = false;

        }

        public static void ParseCommand(CommandEventInfo currentCommand)
        {
            // Get relevant variables
            UIOption type = System.Enum.Parse<UIOption>(currentCommand.GetStringParam(0));
            bool enabled = currentCommand.GetBoolParam(1, true);
            float time = currentCommand.Duration;

            // Logging
            UNANIMATED.Logger.LogInfo($"Parsed UI command at {currentCommand.Time} ms: {type} | params {currentCommand.ParamString} | length {time:0} ms");

            // Command logic
            switch (type)
            {
                case UIOption.Reset:
                    Reset();
                    break;
                case UIOption.ForceLockedUI:
                    forceLockedUI = enabled;
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

        public static class Defaults
        {
            public static readonly float canvasCamFollowerScale = 0.1069f;
        }
    }
}