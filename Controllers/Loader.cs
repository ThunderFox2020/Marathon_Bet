using Marathon_Bet.Views;
using System;
using System.Net;
using System.Threading;

namespace Marathon_Bet.Controllers
{
    public class Loader
    {
        private readonly WebClient webClient = new WebClient();

        public string GetHtml(string url)
        {
            Start:

            string? html = null;

            webClient.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs response) =>
            {
                if (response.Error is null && response.Cancelled is false)
                {
                    html = response.Result;
                }
            };

            webClient.DownloadStringAsync(new Uri(url));

            DateTime start = DateTime.Now;

            while (true)
            {
                if (html is not null)
                {
                    break;
                }
                if (DateTime.Compare(DateTime.Now, start.AddSeconds(60)) >= 0)
                {
                    webClient.CancelAsync();

                    while (true)
                    {
                        if (!webClient.IsBusy)
                        {
                            Console.Clear();
                            Console.WriteLine("          \n          \n          \n          \n          " + Viewer.InCenter(" Загрузка данных длится дольше, чем обычно ", "X").Replace('*', 'X') + "\n");
                            Console.Write(new string(' ', 10) + "=> ");
                            Thread.Sleep(1000);
                            goto Start;
                        }
                    }
                }
            }

            return html;
        }
    }
}