using Marathon_Bet.Views;
using System;
using System.Diagnostics;
using System.Threading;

namespace Marathon_Bet.Controllers
{
    public static class Notifier
    {
        public static bool TrackedEventsRequiresNotification { get; set; }

        public static void Notify()
        {
            if (Program.Settings.NotifierSettings.GeneralNotifyIsNeed)
            {
                if (TrackedEventsRequiresNotification)
                {
                    NotificationProcess(false, 1000);
                }
                else if (Program.NecessaryEvents.Count > 0 && Program.TrackedEvents.Count == 0)
                {
                    NotificationProcess(true, 500);
                }
            }
            TrackedEventsRequiresNotification = false;
        }

        private static void NotificationProcess(bool autoClose, int audioFrequency)
        {
            UserInterface.PermissionToEnter = true;

            int audioDuration = int.Parse(Program.Settings.NotifierSettings.AudioDuration!.Replace('[', ' ').Replace(']', ' ').Trim());
            int videoDuration = int.Parse(Program.Settings.NotifierSettings.VideoDuration!.Replace('[', ' ').Replace(']', ' ').Trim());

            if (Program.Settings.NotifierSettings.AudioNotifyIsNeed)
                Console.Beep(audioFrequency, audioDuration);

            if (Program.Settings.NotifierSettings.VideoNotifyIsNeed)
            {
                Process notification = new Process();

                notification.StartInfo.FileName = "notepad.exe";
                notification.Start();

                if (autoClose)
                {
                    Thread.Sleep(videoDuration);
                    notification.Kill();
                }
            }

            UserInterface.PermissionToEnter = false;
        }
    }
}