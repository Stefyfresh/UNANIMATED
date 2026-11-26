using System;
using System.Linq;

namespace UNANIMATED
{
    public class CommandEventInfo : Rhythm.EventInfo
    {
        public static bool IsEnableCommand(Rhythm.EventInfo check)
        {
            return check.eventParams != null && Enum.TryParse(check.eventType, out ControlCommand command) && command == ControlCommand.UNANIMATED && Enum.TryParse(check.eventParams.ElementAtOrDefault(0), out GeneralOptions option) && option == GeneralOptions.Enable;
        }

        private string[] paramArray;
        public CommandEventInfo(Rhythm.EventInfo info)
        {
            eventType = info.eventType;
            startTime = info.startTime;
            eventParams = info.eventParams;

            eventType ??= string.Empty;
            eventParams ??= [];

            paramArray = HasEndTime ? eventParams[1].Split(':') : eventParams[0].Split(':');
        }


        public bool HasEndTime => eventParams.Count() > 1;

        public int EndTime
        {
            get
            {
                if (HasEndTime && int.TryParse(eventParams[0], out int parsed)) return parsed;
                else return startTime;
            }
        }

        public int Duration => HasEndTime ? EndTime - startTime : 0;

        public string[] Parameters => paramArray;

        public string ParamString => string.Join(", ", Parameters);

        public int Time => startTime;

        public ControlCommand Command => Enum.TryParse(eventType, out ControlCommand commandType) ? commandType : ControlCommand.None;

        public string GetStringParam(int index)
        {
            string output = paramArray.ElementAtOrDefault(index);
            output ??= string.Empty;
            if (output != string.Empty) output = output.Trim();
            return output;
        }

        public float GetFloatParam(int index)
        {
            return float.TryParse(GetStringParam(index), out float parsed) ? parsed : 0;
        }

        public int GetIntParam(int index)
        {
            return int.TryParse(GetStringParam(index), out int parsed) ? parsed : 0;
        }

        public bool GetBoolParam(int index, bool defaultValue = false)
        {
            bool isBool = bool.TryParse(GetStringParam(index), out bool parsedBool);
            bool isInt = int.TryParse(GetStringParam(index), out int parsedInt);
            return (isBool && parsedBool) || (isInt && parsedInt == 1) || (defaultValue && !isBool && !isInt);
        }

        public T GetEnumParamForced<T>(int index) where T : struct
        {
            return Enum.Parse<T>(GetStringParam(index));
        }

        public T GetEnumParam<T>(int index) where T : struct
        {
            return Enum.TryParse(GetStringParam(index), out T parsed) ? parsed : default;
        }

        public bool TryGetEnumParam<T>(int index, out T parsedEnum) where T : struct
        {
            return Enum.TryParse(GetStringParam(index), out parsedEnum);
        }

        public T GetFlagsEnumParamForced<T>(int index) where T : struct
        {
            return Enum.Parse<T>(GetStringParam(index));
        }

        public T GetFlagsEnumParam<T>(int index) where T : struct
        {
            return Enum.TryParse(GetStringParam(index).Replace('|', ',').Replace(';', ','), out T parsed) ? parsed : default;
        }

        public int[] GetIntArrayParam(int index)
        {
            return [.. GetStringParam(index).Split(['|', ';'], StringSplitOptions.RemoveEmptyEntries).Select((str) => int.TryParse(str, out int parsed) ? parsed : 0)];
        }

        public int[] GetFloatArrayParam(int index)
        {
            return [.. GetStringParam(index).Split(['|', ';'], StringSplitOptions.RemoveEmptyEntries).Select((str) => int.TryParse(str, out int parsed) ? parsed : 0)];
        }

        public bool CommandsEqual(CommandEventInfo compare)
        {
            if (compare == null) return false;
            if (compare.eventParams.SequenceEqual(eventParams) && compare.Time == Time && compare.eventType == eventType) return true;
            else return false;
        }
    }
}