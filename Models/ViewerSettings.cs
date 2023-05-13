using System;

namespace Marathon_Bet.Models
{
    [Serializable]
    public class ViewerSettings
    {
        public ViewerSettings()
        {
            GeneralViewIsNeed = true;
        }

        private bool generalViewIsNeed;

        public bool AllEventsViewIsNeed { get; set; } = true;
        public bool NecessaryEventsViewIsNeed { get; set; } = true;
        public bool TrackedEventsViewIsNeed { get; set; } = true;

        public bool GeneralViewIsNeed
        {
            get => generalViewIsNeed;
            set
            {
                generalViewIsNeed = value;
                if (!generalViewIsNeed)
                {
                    AllEventsViewIsNeed = false;
                    NecessaryEventsViewIsNeed = false;
                    TrackedEventsViewIsNeed = false;
                }
            }
        }
    }
}