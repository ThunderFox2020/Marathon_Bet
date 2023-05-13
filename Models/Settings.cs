using System;

namespace Marathon_Bet.Models
{
    [Serializable]
    public class Settings
    {
        public FilterSettings FilterSettings { get; } = new FilterSettings();
        public ViewerSettings ViewerSettings { get; } = new ViewerSettings();
        public NotifierSettings NotifierSettings { get; } = new NotifierSettings();
    }
}