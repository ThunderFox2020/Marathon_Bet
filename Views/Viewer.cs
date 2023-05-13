using Marathon_Bet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Marathon_Bet.Views
{
    public static class Viewer
    {
        public static void Logo()
        {
            Console.WriteLine("          \n          \n          \n          ");
            Console.Write(new string(' ', 10));
            Console.WriteLine("$$   $$   $$$$   $$$$$    $$$$   $$$$$$  $$  $$   $$$$   $$  $$");
            Console.Write(new string(' ', 10));
            Thread.Sleep(500);
            Console.WriteLine("$$$ $$$  $$  $$  $$  $$  $$  $$    $$    $$  $$  $$  $$  $$$ $$");
            Console.Write(new string(' ', 10));
            Thread.Sleep(500);
            Console.WriteLine("$$ $ $$  $$$$$$  $$$$$   $$$$$$    $$    $$$$$$  $$  $$  $$ $$$");
            Console.Write(new string(' ', 10));
            Thread.Sleep(500);
            Console.WriteLine("$$   $$  $$  $$  $$  $$  $$  $$    $$    $$  $$  $$  $$  $$  $$");
            Console.Write(new string(' ', 10));
            Thread.Sleep(500);
            Console.WriteLine("$$   $$  $$  $$  $$  $$  $$  $$    $$    $$  $$   $$$$   $$  $$");
            Console.Write(new string(' ', 10));
            Thread.Sleep(500);
            Console.WriteLine();
            Console.Write(new string(' ', 10));
            Thread.Sleep(500);
            Console.WriteLine("                     $$$$$   $$$$$  $$$$$$                     ");
            Console.Write(new string(' ', 10));
            Thread.Sleep(500);
            Console.WriteLine("                     $$  $$  $$       $$                       ");
            Console.Write(new string(' ', 10));
            Thread.Sleep(500);
            Console.WriteLine("                     $$$$$   $$$$$    $$                       ");
            Console.Write(new string(' ', 10));
            Thread.Sleep(500);
            Console.WriteLine("                     $$  $$  $$       $$                       ");
            Console.Write(new string(' ', 10));
            Thread.Sleep(500);
            Console.WriteLine("                     $$$$$   $$$$$    $$                       ");
            Console.Write(new string(' ', 10));
            Thread.Sleep(5000);
            Console.Clear();
        }
        public static void View()
        {
            if (Program.Settings.ViewerSettings.GeneralViewIsNeed)
            {
                Console.Clear();
                
                Console.WriteLine("          \n          \n          \n          \n          " + InCenter(DateTime.Now.ToString(), "–").Replace('*', '–') + "\n");
                Animation();
                
                if (Program.Settings.ViewerSettings.AllEventsViewIsNeed) Print("Текущие события", Program.AllEvents);

                if (Program.Settings.ViewerSettings.AllEventsViewIsNeed && Program.Settings.ViewerSettings.NecessaryEventsViewIsNeed)
                {
                    Console.WriteLine("          \n          \n          \n          \n          ");
                    Animation();
                }

                if (Program.Settings.ViewerSettings.NecessaryEventsViewIsNeed) Print("Подходящие события", Program.NecessaryEvents);

                if (Program.Settings.ViewerSettings.NecessaryEventsViewIsNeed && Program.Settings.ViewerSettings.TrackedEventsViewIsNeed)
                {
                    Console.WriteLine("          \n          \n          \n          \n          ");
                    Animation();
                }

                if (Program.Settings.ViewerSettings.TrackedEventsViewIsNeed) Print("Отслеживаемые события", Program.TrackedEvents);

                Console.WriteLine(new string(' ', 10) + new string('\\', 100));
                Animation();
                
                Console.WriteLine(new string(' ', 10) + new string('/', 100));
                Animation();
                
                Console.WriteLine(new string(' ', 10));
                Animation();
                
                Console.Write(new string(' ', 10) + "=> ");
                Animation();
            }
        }
        public static string InCenter(string oldString, string filler)
        {
            string newStringVar1 = new string(oldString);

            for (int i = 0; i < (100 - oldString.Length) / 2 - 1; i++)
            {
                newStringVar1 = newStringVar1.Insert(0, filler);
                newStringVar1 = newStringVar1.Insert(newStringVar1.Length, filler);
            }

            newStringVar1 = "*" + newStringVar1 + "*";

            if (newStringVar1.Length < 100)
            {
                string newStringVar2 = new string(newStringVar1);

                while (newStringVar2.Length < 100)
                {
                    newStringVar2 = newStringVar2.Insert(newStringVar2.Length - 2, filler);
                }

                return newStringVar2;
            }

            return newStringVar1;
        }
        public static string InWidth(string stat1, string stat2)
        {
            int spacesNumber = 96 - (stat1 + stat2).Length;

            for (int i = 0; i < spacesNumber; i++)
            {
                stat1 = stat1.Insert(stat1.Length, " ");
            }

            return "* " + stat1 + stat2 + " *";
        }
        public static void Animation()
        {
            Thread.Sleep(20);
        }
        private static void Print(string title, List<Event> events)
        {
            var leagueNames = (from item in events select item.League).Distinct();
            var eventNames = from item in events select item.Title;

            Console.WriteLine(new string(' ', 10) + InCenter(title, " ").Replace('*', ' ') + "\n");
            Animation();

            if (events.Count is 0)
            {
                Console.WriteLine(new string(' ', 10) + "=> События отсутствуют <=");
                Animation();

                return;
            }
            for (int i = 0; i < leagueNames.Count(); i++)
            {
                int count = 0;

                Console.WriteLine(new string(' ', 10) + new string('*', 100));
                Animation();

                Console.WriteLine(new string(' ', 10) + InCenter(leagueNames.ElementAt(i), " "));
                Animation();

                Console.WriteLine(new string(' ', 10) + new string('*', 100));
                Animation();

                for (int j = 0; j < eventNames.Count(); j++)
                {
                    for (int k = 0; k < events.Count; k++)
                    {
                        if (events[k].League == leagueNames.ElementAt(i) && events[k].Title == eventNames.ElementAt(j))
                        {
                            if (count is not 0)
                            {
                                Console.WriteLine(new string(' ', 10) + "* " + new string('=', 96) + " *");
                                Animation();
                            }

                            Console.WriteLine(new string(' ', 10) + InCenter($"{k + 1}. {events[k].Title}", " "));
                            Animation();

                            Console.WriteLine(new string(' ', 10) + InWidth("Счёт: " + events[k].Score, "Период: " + events[k].Period));
                            Animation();

                            Console.WriteLine(new string(' ', 10) + InWidth("Коэффициент: " + events[k].Coefficient, "Время: " + events[k].Time));
                            Animation();

                            count++;
                        }
                    }
                }

                Console.WriteLine(new string(' ', 10) + new string('*', 100));
                Animation();

                if (!(i == leagueNames.Count() - 1))
                {
                    Console.WriteLine("          \n          \n          ");
                    Animation();
                }
            }
        }
    }
}