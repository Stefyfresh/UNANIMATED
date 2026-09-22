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
        JudgementLine = 128,
        MeasureBars = 256,
        SpeedLines = 512,
        UpNextIndicators = 1024,
        Reticle = 2048,
        Notes = 4096,
    }
}