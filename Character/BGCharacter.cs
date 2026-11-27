using System;

namespace UNANIMATED.Character
{
    [Flags]
    public enum BGCharacter
    {
        None = 0,
        Left = 1,
        LeftMiddle = 2,
        Middle = 4,
        RightMiddle = 8,
        Right = 16,
        All = 31,
    }
}