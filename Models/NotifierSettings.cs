using System;

namespace Marathon_Bet.Models
{
    [Serializable]
    public class NotifierSettings
    {
        public NotifierSettings()
        {
            GeneralNotifyIsNeed = true;
            AudioNotifyIsNeed = true;
            VideoNotifyIsNeed = true;
            AudioDuration = null;
            VideoDuration = null;
        }

        private bool generalNotifyIsNeed;
        private bool audioNotifyIsNeed;
        private bool videoNotifyIsNeed;
        private string? audioDuration;
        private string? videoDuration;

        public bool GeneralNotifyIsNeed
        {
            get => generalNotifyIsNeed;
            set
            {
                generalNotifyIsNeed = value;
                if (!generalNotifyIsNeed)
                {
                    AudioNotifyIsNeed = false;
                    VideoNotifyIsNeed = false;
                    AudioDuration = null;
                    VideoDuration = null;
                }
            }
        }
        public bool AudioNotifyIsNeed
        {
            get => audioNotifyIsNeed;
            set
            {
                audioNotifyIsNeed = value;
                if (!audioNotifyIsNeed) AudioDuration = null;
            }
        }
        public bool VideoNotifyIsNeed
        {
            get => videoNotifyIsNeed;
            set
            {
                videoNotifyIsNeed = value;
                if (!videoNotifyIsNeed) VideoDuration = null;
            }
        }
        public string? AudioDuration
        {
            get => audioDuration;
            set => audioDuration = value is null ? "[4800]" : value;
        }
        public string? VideoDuration
        {
            get => videoDuration;
            set => videoDuration = value is null ? "[200]" : value;
        }
    }
}