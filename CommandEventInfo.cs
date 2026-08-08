using System.Linq;

namespace UNANIMATED
{
    public class CommandEventInfo : Rhythm.EventInfo
    {
        public CommandEventInfo(Rhythm.EventInfo info)
        {
            eventType = info.eventType;
            startTime = info.startTime;
            eventParams = info.eventParams;
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
            get { return HasEndTime ? eventParams[1].Split(':') : eventParams[0].Split(':'); }
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
            if (HasEndTime)
            {
                return float.TryParse(eventParams[1].Split(':')[index], out float parsed) ? parsed : 0;
            }
            else
            {
                return float.TryParse(eventParams[0].Split(':')[index], out float parsed) ? parsed : 0;
            }
        }

        public string GetStringParam(int index)
        {
            if (HasEndTime)
            {
                return eventParams[1].Split(':').ElementAtOrDefault(index);
            }
            else
            {
                return eventParams[0].Split(':').ElementAtOrDefault(index);
            }
        }

        public bool CommandsEqual(CommandEventInfo compare)
        {
            if (compare == null) return false;
            if (compare.eventParams.SequenceEqual(eventParams) && compare.Time == Time && compare.eventType == eventType) return true;
            else return false;
        }
    }
}