using System;

namespace UNANIMATED.UI
{
    [Flags]
    public enum SlideUIType
    {
        AllOverlayUI = 1,
        LeftJudgementLine = 2,
        RightJudgementLine = 4,
        BackgroundGradient = 8,
        HealthBar = 16,
        SpeedLines = 32,
        FourByThreeBars = 64,
        BlackBGBars = 128,
    }
}