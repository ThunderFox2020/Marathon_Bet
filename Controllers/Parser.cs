using AngleSharp.Html.Parser;
using Marathon_Bet.Models;
using Marathon_Bet.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Marathon_Bet.Controllers
{
    public class Parser
    {
        private readonly HtmlParser htmlParser = new HtmlParser();

        public List<Event> GetAllEvents(string html)
        {
            List<Event> allEvents = new List<Event>();

            if (HasEvents(html) && HasHockey(ref html))
            {
                AddLeagueAndTitle(allEvents, html);
                AddScoreAndPeriodAndTime(allEvents, html);
                AddCoefficient(allEvents, html);
            }

            return allEvents;
        }

        private bool HasEvents(string html)
        {
            return (from item
                    in htmlParser.ParseDocument(html).GetElementsByTagName("link")
                    where (item.GetAttribute("rel") is "canonical" && item.GetAttribute("href") is "https://www.marathonbet.ru/su/live/43658")
                    select item).Count() is 1;
        }
        private bool HasHockey(ref string html)
        {
            var hockey = from item
                         in htmlParser.ParseDocument(html).GetElementsByTagName("div")
                         where (item.ClassName is not null && item.ClassName.Contains("sport-category-container") && item.GetAttribute("data-sport-treeid") is "43658")
                         select item;

            if (hockey.Count() is not 0)
            {
                html = hockey.ElementAt(0).InnerHtml;
                return true;
            }

            return false;
        }
        private void AddLeagueAndTitle(List<Event> allEvents, string html)
        {
            var leagueHtml = from item
                             in htmlParser.ParseDocument(html).GetElementsByTagName("div")
                             where (item.ClassName is not null && item.ClassName.Contains("category-container"))
                             select item.InnerHtml;

            StringBuilder leagueName = new StringBuilder();

            for (int i = 0; i < leagueHtml.Count(); i++)
            {
                var leagueNameElements = from item
                                         in htmlParser.ParseDocument(leagueHtml.ElementAt(i)).GetElementsByTagName("span")
                                         where (item.ClassName is not null && item.ClassName.Contains("nowrap"))
                                         select item.TextContent.Trim();

                for (int j = 0; j < leagueNameElements.Count(); j++)
                {
                    leagueName.Append(leagueNameElements.ElementAt(j) + " ");
                }

                var eventNames = from item
                                 in htmlParser.ParseDocument(leagueHtml.ElementAt(i)).GetElementsByTagName("div")
                                 where (item.ClassName is not null && item.ClassName.Contains("bg coupon-row"))
                                 select item.GetAttribute("data-event-name");

                for (int k = 0; k < eventNames.Count(); k++)
                {
                    allEvents.Add(new Event { League = leagueName.ToString().Trim(), Title = eventNames.ElementAt(k) });
                }

                leagueName.Clear();
            }
        }
        private void AddScoreAndPeriodAndTime(List<Event> allEvents, string html)
        {
            var eventHtml = from item
                            in htmlParser.ParseDocument(html).GetElementsByTagName("div")
                            where (item.ClassName is not null && item.ClassName.Contains("bg coupon-row"))
                            select item.InnerHtml;

            for (int i = 0; i < allEvents.Count; i++)
            {
                var eventStatistics = from item
                                      in htmlParser.ParseDocument(eventHtml.ElementAt(i)).GetElementsByTagName("div")
                                      where (item.ClassName is not null && item.ClassName.Contains("cl-left red"))
                                      select item.TextContent.Trim();

                string statistics = eventStatistics.ElementAt(0);
                string score = statistics.Substring(0, 4).Trim();
                string period = statistics[0..^5].Trim();
                string time = statistics.Substring(statistics.Length - 6).Trim();

                Event e = allEvents[i];
                e.Score = score;

                if (!period.Contains(',')) e.Period = "1";
                else if (period.Count((char c) => (c is ',')) is 1) e.Period = "2";
                else if (period.Count((char c) => (c is ',')) is 2) e.Period = "3";

                e.Time = time;
                allEvents[i] = e;
            }
        }
        private void AddCoefficient(List<Event> allEvents, string html)
        {
            Dictionary<string, string> coefficients = new Dictionary<string, string>();

            string[] eventIds = (from item
                                in htmlParser.ParseDocument(html).GetElementsByTagName("div")
                                where (item.ClassName is not null && item.ClassName.Contains("bg coupon-row"))
                                select item.GetAttribute("data-event-treeId")).ToArray();

            Task[] tasks = new Task[eventIds.Length];

            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = new Task(GetCoefficient!, new GetCoefficientParameters() { EventId = eventIds[i], Coefficients = coefficients });
                tasks[i].Start();
            }

            Task.WaitAll(tasks);

            for (int i = 0; i < allEvents.Count; i++)
            {
                Event e = allEvents[i];
                e.Coefficient = coefficients[eventIds[i]];
                allEvents[i] = e;
            }
        }
        private void GetCoefficient(object getCoefficientParameters)
        {
            GetCoefficientParameters parameters = (GetCoefficientParameters)getCoefficientParameters;
            string? coefficient = null;

            do
            {
                try
                {
                    Loader loader = new Loader();
                    HtmlParser htmlParser = new HtmlParser();

                    var coefficientHtml = from item
                                          in htmlParser.ParseDocument(loader.GetHtml("https://www.marathonbet.ru/su/live/43658?ecids=all&epcids=all&marketIds=" + parameters.EventId)).GetElementsByTagName("div")
                                          where (item.TextContent.Trim() is "Обе команды забьют")
                                          select item.Closest("tr")!.InnerHtml;

                    if (coefficientHtml.Count() is 0) coefficient = "0";
                    else
                    {
                        var coefficientValues = from item
                                                in htmlParser.ParseDocument(coefficientHtml.ElementAt(0)).GetElementsByTagName("span")
                                                select item.TextContent;

                        coefficient = (Math.Round(double.Parse(coefficientValues.ElementAt(0).Replace(".", ",")), 2)).ToString().Replace(",", ".");
                    }
                }
                catch (WebException)
                {
                    Console.Clear();
                    Console.WriteLine("          \n          \n          \n          \n          " + Viewer.InCenter(" Соединение с интернетом нестабильно ", "X").Replace('*', 'X') + "\n");
                    Console.Write(new string(' ', 10) + "=> ");
                    Thread.Sleep(1000);
                }
                catch (Exception)
                {
                    Console.Clear();
                    Console.WriteLine("          \n          \n          \n          \n          " + Viewer.InCenter(" Неизвестная ошибка ", "X").Replace('*', 'X') + "\n");
                    Console.Write(new string(' ', 10) + "=> ");
                    Thread.Sleep(1000);
                }
            } while (coefficient is null);

            parameters.Coefficients!.Add(parameters.EventId!, coefficient);
        }
        
        private class GetCoefficientParameters
        {
            public string? EventId { get; set; }
            public Dictionary<string, string>? Coefficients { get; set; }
        }
    }
}