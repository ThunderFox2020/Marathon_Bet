using Marathon_Bet.Models;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Marathon_Bet.Controllers
{
    public static class Tracker
    {
        private static readonly List<Event> trackedEvents = new List<Event>();

        public static List<Event> GetTrackedEvents()
        {
            for (int i = 0; i < trackedEvents.Count; i++)
            {
                for (int j = 0; j < Program.AllEvents?.Count; j++)
                {
                    if (trackedEvents[i].League == Program.AllEvents[j].League && trackedEvents[i].Title == Program.AllEvents[j].Title)
                    {
                        trackedEvents[i] = Program.AllEvents[j];

                        if (Program.AllEvents[j].Score?.Split(':')[0] is not "0" && Program.AllEvents[j].Score?.Split(':')[1] is not "0")
                        {
                            Notifier.TrackedEventsRequiresNotification = true;
                            trackedEvents.RemoveAt(i);
                            i = -1;
                        }

                        break;
                    }
                    if (j == Program.AllEvents.Count - 1)
                    {
                        Notifier.TrackedEventsRequiresNotification = true;
                        trackedEvents.RemoveAt(i);
                        i = -1;
                    }
                }
            }
            return trackedEvents;
        }
        public static void AddToTrackedEvents(int index)
        {
            if (Program.AllEvents is not null)
            {
                if (0 <= index && index < Program.AllEvents.Count)
                {
                    for (int i = 0; i < trackedEvents.Count; i++)
                    {
                        if (Program.AllEvents[index].League == trackedEvents[i].League && Program.AllEvents[index].Title == trackedEvents[i].Title)
                        {
                            Console.WriteLine("Требуемое событие уже присутствует в списке отслеживания");
                            Console.Write(new string(' ', 10) + "=> ");
                            Thread.Sleep(1000);

                            return;
                        }
                    }

                    if (Program.AllEvents[index].Score?.Split(':')[0] == "0" || Program.AllEvents[index].Score?.Split(':')[1] == "0")
                    {
                        trackedEvents.Add(Program.AllEvents[index]);

                        Console.WriteLine("Требуемое событие добавлено в список отслеживания");
                        Console.Write(new string(' ', 10) + "=> ");
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        Console.WriteLine("Требуемое событие невозможно добавить в список отслеживания");
                        Console.Write(new string(' ', 10) + "=> ");
                        Thread.Sleep(1000);
                    }
                }
                else
                {
                    Console.WriteLine("Требуемое событие отсутствует");
                    Console.Write(new string(' ', 10) + "=> ");
                    Thread.Sleep(1000);
                }
            }
            else
            {
                Console.WriteLine("Список текущих событий не сформирован");
                Console.Write(new string(' ', 10) + "=> ");
                Thread.Sleep(1000);
            }
        }
        public static void RemoveToTrackedEvents(int index)
        {
            if (0 <= index && index < trackedEvents.Count)
            {
                trackedEvents.RemoveAt(index);

                Console.WriteLine("Требуемое событие удалено из списка отслеживания");
                Console.Write(new string(' ', 10) + "=> ");
                Thread.Sleep(1000);
            }
            else
            {
                Console.WriteLine("Требуемое событие отсутствует");
                Console.Write(new string(' ', 10) + "=> ");
                Thread.Sleep(1000);
            }
        }
    }
}