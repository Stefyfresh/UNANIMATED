using System;

namespace UNANIMATED.UI
{
    [Flags]
    public enum UIType
    {
        Score = 1,
        Accuracy = 2,
        MaxCombo = 4,
        VignetteBars = 8,
        BlackBGBars = 16,
        FourByThreeBars = 32,
        Health = 64,
        LeftJudgementLine = 128,
        RightJudgementLine = 256,
        MeasureBars = 512,
        SpeedLines = 1024,
        UpNextIndicators = 2048,
        Reticle = 4096,
        Notes = 8192,
    }
}