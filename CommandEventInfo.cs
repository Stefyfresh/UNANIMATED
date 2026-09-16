using System;
using System.Linq;

namespace UNANIMATED
{
    public class CommandEventInfo : Rhythm.EventInfo
    {
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


        public bool HasEndTime
        {
            get { return eventParams.Count() > 1; }
        }

        public int EndTime
        {
            get
            {
                if (HasEndTime && int.TryParse(eventParams[0], out int parsed)) return parsed;
                else return startTime;
            }
        }

        public int Duration
        {
            get { return HasEndTime ? EndTime - startTime : 0; }
        }

        public string[] Parameters
        {
            get { return paramArray; }
        }
        public string ParamString
        {
            get { return string.Join(", ", Parameters); }
        }

        public int Time
        {
            get
            {
                return startTime;
            }
        }

        public float GetFloatParam(int index)
        {
            return float.TryParse(GetStringParam(index), out float parsed) ? parsed : 0;
        }

        public float GetIntParam(int index)
        {
            return int.TryParse(GetStringParam(index), out int parsed) ? parsed : 0;
        }

        public string GetStringParam(int index)
        {
            string output = paramArray.ElementAtOrDefault(index);
            output ??= string.Empty;
            if (output != string.Empty) output = output.Trim();
            return output;
        }

        public bool GetBoolParam(int index)
        {
            bool isBool = bool.TryParse(GetStringParam(index), out bool parsedBool);
            bool isInt = int.TryParse(GetStringParam(index), out int parsedInt);
            return (isBool && parsedBool) || (isInt && parsedInt == 1) || (!isBool && !isInt);
        }

        public T GetEnumParam<T>(int index) where T : struct
        {
            return Enum.Parse<T>(GetStringParam(index));
        }

        public bool CommandsEqual(CommandEventInfo compare)
        {
            if (compare == null) return false;
            if (compare.eventParams.SequenceEqual(eventParams) && compare.Time == Time && compare.eventType == eventType) return true;
            else return false;
        }
    }
}