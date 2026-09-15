using Rhythm;
using UnityEngine;

namespace UNANIMATED.Visuals
{
    public static class VisualController
    {
        public static bool isInverted;

        // ---- Materials ---- 
        public static Material invertMaterial;
        // Above scene elements, guides, speed lines, measure bars, player, below everything else

        public static Material backgroundMaterial;
        // Above scene elements, speed lines, measure bars, below everything else

        public static Material scorecardMaterial;
        // Below vignette and score, above everything else

        public static Material twinBarMaterial;
        // Below vignette and score, above everything else 


        public static void Reset()
        {
            isInverted = false;
        }

        public static void ParseCommand(CommandEventInfo currentCommand)
        {
            // Get relevant variables
            VisualOption type = System.Enum.Parse<VisualOption>(currentCommand.GetStringParam(0));
            bool enabled = currentCommand.GetBoolParam(1);
            float time = currentCommand.Duration;

            // Logging
            UNANIMATED.Logger.LogInfo($"Parsed visuals command at {currentCommand.Time} ms: {type} | params {currentCommand.ParamString} | length {time:0} ms");

            // Command logic
            switch (type)
            {
                case VisualOption.Reset:
                    Reset();
                    break;
                case VisualOption.Invert:

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



        public static void Init()
        {
            GameObject.Find("/Rhythm Game Container/RhythmUI/UiParentCanvas/UiParent/Masks/BackgroundDim");
        }
    }
}