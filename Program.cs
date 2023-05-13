using Marathon_Bet.Controllers;
using Marathon_Bet.Models;
using Marathon_Bet.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Xml.Serialization;

namespace Marathon_Bet
{
    public static class Program
    {
        static Program()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));

            if (File.Exists(@"..\..\..\Settings\Default.xml"))
            {
                using (FileStream fileStream = new FileStream(@"..\..\..\Settings\Default.xml", FileMode.OpenOrCreate))
                {
                    Settings = serializer.Deserialize(fileStream) as Settings ?? new Settings();
                }
            }
        }

        public static Settings Settings { get; private set; } = new Settings();

        public static List<Event> AllEvents { get; private set; } = new List<Event>();
        public static List<Event> NecessaryEvents { get; private set; } = new List<Event>();
        public static List<Event> TrackedEvents { get; private set; } = new List<Event>();

        public static void Main()
        {
            Console.Title = "Marathon Bet";

            Viewer.Logo();

            Console.WriteLine("          \n          \n          \n          ");
            Console.Write(new string(' ', 10) + "=> ");

            Loader loader = new Loader();
            Parser parser = new Parser();

            while (true)
            {
                try
                {
                    UserInterface.CommandHandling();

                    UserInterface.PermissionToEnter = true;
                    AllEvents = parser.GetAllEvents(loader.GetHtml("https://www.marathonbet.ru/su/live/43658?ecids=all&epcids=all"));
                    UserInterface.PermissionToEnter = false;

                    UserInterface.CommandHandling();
                    NecessaryEvents = Filter.GetNecessaryEvents();
                    UserInterface.CommandHandling();
                    TrackedEvents = Tracker.GetTrackedEvents();
                    UserInterface.CommandHandling();

                    Viewer.View();
                    UserInterface.CommandHandling();
                    Notifier.Notify();
                    UserInterface.CommandHandling();

                    UserInterface.PermissionToEnter = true;
                    Thread.Sleep(10000);
                    UserInterface.PermissionToEnter = false;
                }
                catch (WebException)
                {
                    Console.Clear();
                    Console.WriteLine("          \n          \n          \n          \n          " + Viewer.InCenter(" Соединение с интернетом нестабильно ", "X").Replace('*', 'X') + "\n");
                    Console.Write(new string(' ', 10) + "=> ");
                    Thread.Sleep(1000);
                }
                catch (ArgumentOutOfRangeException)
                {
                    Console.Clear();
                    Console.WriteLine("          \n          \n          \n          \n          " + Viewer.InCenter(" Процесс изменения списка текущих событий ", "X").Replace('*', 'X') + "\n");
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
            }
        }
        public static void SetDefaultSettings()
        {
            Settings = new Settings();

            Console.WriteLine("Стандартные настройки установлены");
            Console.Write(new string(' ', 10) + "=> ");
            Thread.Sleep(1000);
        }
        public static void SaveSettings()
        {
            string fileName = UserInterface.ExtractValueFromCommand();

            XmlSerializer serializer = new XmlSerializer(typeof(Settings));

            using (FileStream fileStream = new FileStream($"..\\..\\..\\Settings\\{fileName}.xml", FileMode.OpenOrCreate))
            {
                serializer.Serialize(fileStream, Settings);
            }

            Console.WriteLine("Требуемые настройки сохранены");
            Console.Write(new string(' ', 10) + "=> ");
            Thread.Sleep(1000);
        }
        public static void LoadSettings()
        {
            string fileName = UserInterface.ExtractValueFromCommand();

            XmlSerializer serializer = new XmlSerializer(typeof(Settings));

            if (File.Exists($"..\\..\\..\\Settings\\{fileName}.xml"))
            {
                using (FileStream fileStream = new FileStream($"..\\..\\..\\Settings\\{fileName}.xml", FileMode.OpenOrCreate))
                {
                    Settings = (Settings)serializer.Deserialize(fileStream)!;
                }

                Console.WriteLine("Требуемые настройки загружены");
                Console.Write(new string(' ', 10) + "=> ");
                Thread.Sleep(1000);
            }
            else
            {
                Console.WriteLine("Требуемые настройки отсутствуют");
                Console.Write(new string(' ', 10) + "=> ");
                Thread.Sleep(1000);
            }
        }
    }
}