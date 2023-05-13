using Marathon_Bet.Controllers;
using Marathon_Bet.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Marathon_Bet.Views
{
    public static class UserInterface
    {
        static UserInterface()
        {
            commands = new List<string>()
            {
                "Просмотреть список команд",
                "Добавить событие в отслеживаемые",
                "Удалить событие из отслеживаемых",
                "Просмотреть текущие настройки",
                "Просмотреть все настройки",
                "Установить стандартные настройки",
                "Установить специальные настройки",
                "Загрузить настройки",
                "Удалить настройки",
                "Сохранить настройки",
                "Включить режим 'не беспокоить'",
                "Отключить режим 'не беспокоить'",
                "Завершить сеанс"
            };

            commandWaiter = new Task(WaitForCommand);
            commandWaiter.Start();
        }

        private static bool requestForInput;
        private static bool isCommandExists;
        private static bool isCommandHandling;
        private static bool dontDisturbeModeIsOn;
        private static bool dontDisturbeModeDisconnector;
        private static string? command;

        private static readonly List<string> commands;
        private static readonly Task commandWaiter;

        public static bool PermissionToEnter { get; set; }

        public static void CommandHandling()
        {
            if (requestForInput)
            {
                PermissionToEnter = true;

                do
                {
                    Thread.Sleep(1);
                } while (requestForInput);

                PermissionToEnter = false;
            }
            if (isCommandExists)
            {
                if (command == "Просмотреть список команд")
                    PrintList("Список команд", (from item in commands select ($"<{commands.FindIndex(i => i.Contains(item)) + 1}> " + item)).ToList());
                else if (command!.Length == new Regex(@"Добавить событие в отслеживаемые \[\d+\]").Match(command).Value.Length)
                    AddToTrackedEvents();
                else if (command.Length == new Regex(@"Удалить событие из отслеживаемых \[\d+\]").Match(command).Value.Length)
                    RemoveToTrackedEvents();
                else if (command == "Просмотреть текущие настройки")
                    PrintSettings("Текущие настройки");
                else if (command == "Просмотреть все настройки")
                    PrintList("Все настройки", (from item in Directory.GetFiles(@"..\..\..\Settings\") select item.Replace(@"..\..\..\Settings\", "").Replace(".dat", "")).ToList());
                else if (command == "Установить стандартные настройки")
                    Program.SetDefaultSettings();
                else if (command == "Установить специальные настройки")
                    SetSpecialSettings();
                else if (command.Length == new Regex(@"Загрузить настройки \[[^\[\]]*\]").Match(command).Value.Length)
                    Program.LoadSettings();
                else if (command.Length == new Regex(@"Удалить настройки \[[^\[\]]*\]").Match(command).Value.Length)
                    DeleteSettings();
                else if (command.Length == new Regex(@"Сохранить настройки \[[^\[\]]*\]").Match(command).Value.Length)
                    Program.SaveSettings();
                else if (command.Length == new Regex(@"Включить режим 'не беспокоить' \[\d+\]").Match(command).Value.Length)
                    TurnDontDisturbeModeOn();
                else if (command == "Отключить режим 'не беспокоить'")
                    TurnDontDisturbeModeOff();
                else if (command == "Завершить сеанс")
                    Exit();

                isCommandHandling = false;
                isCommandExists = false;
            }
        }
        
        public static string ExtractValueFromCommand()
        {
            return command!.Substring(command.IndexOf('[')).Replace('[', ' ').Replace(']', ' ').Trim();
        }
        
        private static void AddToTrackedEvents()
        {
            Tracker.AddToTrackedEvents(int.Parse(ExtractValueFromCommand()) - 1);
        }
        
        private static bool CoefficientIsCorrectFormat(string value)
        {
            bool isCorrectFormat = value.Length == new Regex(@"\[\d+\.\d+ - \d+\.\d+\]").Match(value).Value.Length;

            if (isCorrectFormat)
            {
                double i1 = double.Parse(value.Replace('.', ',').Replace('[', ' ').Replace(']', ' ').Trim().Split("-")[0].Trim());
                double i2 = double.Parse(value.Replace('.', ',').Replace('[', ' ').Replace(']', ' ').Trim().Split("-")[1].Trim());
                
                isCorrectFormat = i1 <= i2;
            }

            return isCorrectFormat;
        }
        
        private static void DeleteSettings()
        {
            string fileName = ExtractValueFromCommand();

            if (File.Exists($"..\\..\\..\\Settings\\{fileName}.dat"))
            {
                File.Delete($"..\\..\\..\\Settings\\{fileName}.dat");

                Console.WriteLine("Требуемые настройки удалены");
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
        private static void DisableDontDisturbeMode()
        {
            Program.Settings.NotifierSettings.GeneralNotifyIsNeed = true;

            Program.Settings.NotifierSettings.AudioNotifyIsNeed = true;
            Program.Settings.NotifierSettings.VideoNotifyIsNeed = true;

            Program.Settings.NotifierSettings.AudioDuration = null;
            Program.Settings.NotifierSettings.VideoDuration = null;
        }
        private static void DontDisturbeModeTimer(object parameters)
        {
            Console.WriteLine($"Режим 'не беспокоить' включен на {(int)parameters} мин.");
            Console.Write(new string(' ', 10) + "=> ");
            Thread.Sleep(1000);

            dontDisturbeModeIsOn = true;
            DateTime start = DateTime.Now;

            while (true)
            {
                if (dontDisturbeModeDisconnector)
                {
                    DisableDontDisturbeMode();
                    break;
                }
                if (DateTime.Compare(DateTime.Now, start.AddMinutes((int)parameters)) >= 0)
                {
                    DisableDontDisturbeMode();
                    break;
                }
            }

            dontDisturbeModeIsOn = false;
            dontDisturbeModeDisconnector = false;

            Console.WriteLine("Режим 'не беспокоить' отключен");
            Console.Write(new string(' ', 10) + "=> ");
            Thread.Sleep(1000);
        }
        
        private static void Exit()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));

            using (FileStream fileStream = new FileStream(@"..\..\..\Settings\Default.xml", FileMode.OpenOrCreate))
            {
                serializer.Serialize(fileStream, Program.Settings);
            }

            Environment.Exit(0);
        }

        private static bool FilterIsCorrectFormatSelector(FilterOption option, string value)
        {
            return option switch
            {
                FilterOption.League => LeagueAndTitleIsCorrectFormat(value),
                FilterOption.Title => LeagueAndTitleIsCorrectFormat(value),
                FilterOption.Score => ScoreIsCorrectFormat(value),
                FilterOption.Period => PeriodIsCorrectFormat(value),
                FilterOption.Time => TimeIsCorrectFormat(value),
                FilterOption.Coefficient => CoefficientIsCorrectFormat(value),
                _ => false,
            };
        }
        private static void FilterOptionSelector(FilterOption option, bool value)
        {
            switch (option)
            {
                case FilterOption.League:
                    Program.Settings.FilterSettings.LeagueFilterIsNeed = value;
                    break;
                case FilterOption.Title:
                    Program.Settings.FilterSettings.TitleFilterIsNeed = value;
                    break;
                case FilterOption.Score:
                    Program.Settings.FilterSettings.ScoreFilterIsNeed = value;
                    break;
                case FilterOption.Period:
                    Program.Settings.FilterSettings.PeriodFilterIsNeed = value;
                    break;
                case FilterOption.Time:
                    Program.Settings.FilterSettings.TimeFilterIsNeed = value;
                    break;
                case FilterOption.Coefficient:
                    Program.Settings.FilterSettings.CoefficientFilterIsNeed = value;
                    break;
                default:
                    break;
            }
        }
        private static void FilterOptionSelector(FilterOption option, string value)
        {
            switch (option)
            {
                case FilterOption.League:
                    Program.Settings.FilterSettings.LeagueValue = value;
                    break;
                case FilterOption.Title:
                    Program.Settings.FilterSettings.TitleValue = value;
                    break;
                case FilterOption.Score:
                    Program.Settings.FilterSettings.ScoreValue = value;
                    break;
                case FilterOption.Period:
                    Program.Settings.FilterSettings.PeriodValue = value;
                    break;
                case FilterOption.Time:
                    Program.Settings.FilterSettings.TimeValue = value;
                    break;
                case FilterOption.Coefficient:
                    Program.Settings.FilterSettings.CoefficientValue = value;
                    break;
                default:
                    break;
            }
        }
        
        private static bool LeagueAndTitleIsCorrectFormat(string value)
        {
            bool isCorrectFormat = value.Length == new Regex(@"\[[^\[\]]*\]").Match(value).Value.Length ||
                                   value.Length == new Regex(@"(\[[^\[\]]*\]\*)+\[[^\[\]]*\]").Match(value).Value.Length;

            return isCorrectFormat;
        }
        
        private static void NotifierOptionSelector(NotifierOption option, bool value)
        {
            switch (option)
            {
                case NotifierOption.Audio:
                    Program.Settings.NotifierSettings.AudioNotifyIsNeed = value;
                    break;
                case NotifierOption.Video:
                    Program.Settings.NotifierSettings.VideoNotifyIsNeed = value;
                    break;
                default:
                    break;
            }
        }
        private static void NotifierOptionSelector(NotifierOption option, string value)
        {
            switch (option)
            {
                case NotifierOption.Audio:
                    Program.Settings.NotifierSettings.AudioDuration = value;
                    break;
                case NotifierOption.Video:
                    Program.Settings.NotifierSettings.VideoDuration = value;
                    break;
                default:
                    break;
            }
        }
        
        private static string OutputFormat(string text)
        {
            return "\n" + new string(' ', 10) + Viewer.InCenter(text, " ").Replace('*', ' ');
        }
        
        private static bool PeriodIsCorrectFormat(string value)
        {
            bool isCorrectFormat = value.Length == new Regex(@"\[[1-3] - [1-3]\]").Match(value).Value.Length;

            if (isCorrectFormat)
            {
                int i1 = int.Parse(value.Replace('[', ' ').Replace(']', ' ').Trim().Split("-")[0].Trim());
                int i2 = int.Parse(value.Replace('[', ' ').Replace(']', ' ').Trim().Split("-")[1].Trim());

                isCorrectFormat = i1 <= i2;
            }

            return isCorrectFormat;
        }
        private static void PrintEnd()
        {
            Console.WriteLine(new string(' ', 10) + new string('\\', 100));
            Viewer.Animation();

            Console.WriteLine(new string(' ', 10) + new string('/', 100));
            Viewer.Animation();

            Console.WriteLine(new string(' ', 10));
            Viewer.Animation();

            Console.Write(new string(' ', 10) + "=> Нажмите Enter для продолжения... ");
            Viewer.Animation();

            Console.ReadLine();
            Console.Write(new string(' ', 10) + "=> ");
        }
        private static void PrintList(string title, List<string> list)
        {
            PrintStart(title);

            if (list.Count > 0)
            {
                Console.WriteLine(new string(' ', 10) + new string('*', 100));
                Viewer.Animation();

                for (int i = 0; i < list.Count; i++)
                {
                    Console.WriteLine(new string(' ', 10) + Viewer.InCenter(list[i], " "));
                    Viewer.Animation();

                    Console.WriteLine(new string(' ', 10) + new string('*', 100));
                    Viewer.Animation();
                }
            }

            PrintEnd();
        }
        private static void PrintOption(string option, string optionOn, string optionOff, bool optionIsNeed, object[][] elements)
        {
            Console.WriteLine(new string(' ', 10) + new string('*', 100));
            Viewer.Animation();

            Console.WriteLine(new string(' ', 10) + Viewer.InCenter(optionIsNeed ? optionOn : optionOff, " "));
            Viewer.Animation();

            Console.WriteLine(new string(' ', 10) + new string('*', 100));
            Viewer.Animation();

            for (int i = 0; i < elements.Length; i++)
            {
                if (i is not 0)
                {
                    Console.WriteLine(new string(' ', 10) + "* " + new string('=', 96) + " *");
                    Viewer.Animation();
                }

                Console.WriteLine(new string(' ', 10) + Viewer.InWidth((string)elements[i][0], (bool)elements[i][2] ? "Включено" : "Отключено"));
                Viewer.Animation();

                if (option is not "Viewer")
                {
                    Console.WriteLine(new string(' ', 10) + Viewer.InWidth((string)elements[i][1], (bool)elements[i][2] ? (string)elements[i][3] : "-"));
                    Viewer.Animation();
                }
            }

            Console.WriteLine(new string(' ', 10) + new string('*', 100));
            Viewer.Animation();
        }
        private static void PrintSettings(string title)
        {
            object[][] filterElements = new object[6][] { new object[4] { "По названию лиги", "Значение", Program.Settings.FilterSettings.LeagueFilterIsNeed, Program.Settings.FilterSettings.LeagueValue! },
                                                          new object[4] { "По названию матча", "Значение", Program.Settings.FilterSettings.TitleFilterIsNeed, Program.Settings.FilterSettings.TitleValue! },
                                                          new object[4] { "По счёту", "Значение", Program.Settings.FilterSettings.ScoreFilterIsNeed, Program.Settings.FilterSettings.ScoreValue! },
                                                          new object[4] { "По периоду", "Значение", Program.Settings.FilterSettings.PeriodFilterIsNeed, Program.Settings.FilterSettings.PeriodValue! },
                                                          new object[4] { "По времени", "Значение", Program.Settings.FilterSettings.TimeFilterIsNeed, Program.Settings.FilterSettings.TimeValue! },
                                                          new object[4] { "По коэффициенту", "Значение", Program.Settings.FilterSettings.CoefficientFilterIsNeed, Program.Settings.FilterSettings.CoefficientValue! } };
            object[][] viewerElements = new object[3][] { new object[4] { "Текущие события", null!, Program.Settings.ViewerSettings.AllEventsViewIsNeed, null! },
                                                          new object[4] { "Подходящие события", null!, Program.Settings.ViewerSettings.NecessaryEventsViewIsNeed, null! },
                                                          new object[4] { "Отслеживаемые события", null!, Program.Settings.ViewerSettings.TrackedEventsViewIsNeed, null! } };
            object[][] notifierElements = new object[2][] { new object[4] { "Звуковое", "Длительность", Program.Settings.NotifierSettings.AudioNotifyIsNeed, Program.Settings.NotifierSettings.AudioDuration! },
                                                            new object[4] { "Визуальное", "Длительность", Program.Settings.NotifierSettings.VideoNotifyIsNeed, Program.Settings.NotifierSettings.VideoDuration! } };
            PrintStart(title);

            PrintOption("Filter", "Фильтрация (включено)", "Фильтрация (отключено)", Program.Settings.FilterSettings.GeneralFilterIsNeed, filterElements);
            
            Console.WriteLine("          \n          \n          ");
            Viewer.Animation();

            PrintOption("Viewer", "Отображение (включено)", "Отображение (отключено)", Program.Settings.ViewerSettings.GeneralViewIsNeed, viewerElements);
            
            Console.WriteLine("          \n          \n          ");
            Viewer.Animation();

            PrintOption("Notifier", "Уведомление (включено)", "Уведомление (отключено)", Program.Settings.NotifierSettings.GeneralNotifyIsNeed, notifierElements);
            
            PrintEnd();
        }
        private static void PrintStart(string title)
        {
            Console.Clear();
            Console.WriteLine("          \n          \n          \n          \n          " + Viewer.InCenter(title, "–").Replace('*', '–') + "\n");
            Viewer.Animation();
        }
        
        private static void RemoveToTrackedEvents()
        {
            Tracker.RemoveToTrackedEvents(int.Parse(ExtractValueFromCommand()) - 1);
        }
        
        private static bool ScoreIsCorrectFormat(string value)
        {
            bool isCorrectFormat = value.Length == new Regex(@"\[\d+:\d+ - \d+:\d+ \|\| \d+:\d+ - \d+:\d+\]").Match(value).Value.Length;

            if (isCorrectFormat)
            {
                int i1 = int.Parse(value.Replace('[', ' ').Replace(']', ' ').Trim().Split("||")[0].Split("-")[0].Split(":")[0].Trim());
                int i2 = int.Parse(value.Replace('[', ' ').Replace(']', ' ').Trim().Split("||")[0].Split("-")[0].Split(":")[1].Trim());
                int i3 = int.Parse(value.Replace('[', ' ').Replace(']', ' ').Trim().Split("||")[0].Split("-")[1].Split(":")[0].Trim());
                int i4 = int.Parse(value.Replace('[', ' ').Replace(']', ' ').Trim().Split("||")[0].Split("-")[1].Split(":")[1].Trim());
                int i5 = int.Parse(value.Replace('[', ' ').Replace(']', ' ').Trim().Split("||")[1].Split("-")[0].Split(":")[0].Trim());
                int i6 = int.Parse(value.Replace('[', ' ').Replace(']', ' ').Trim().Split("||")[1].Split("-")[0].Split(":")[1].Trim());
                int i7 = int.Parse(value.Replace('[', ' ').Replace(']', ' ').Trim().Split("||")[1].Split("-")[1].Split(":")[0].Trim());
                int i8 = int.Parse(value.Replace('[', ' ').Replace(']', ' ').Trim().Split("||")[1].Split("-")[1].Split(":")[1].Trim());
                isCorrectFormat = i1 <= i3 && i2 <= i4 && i5 <= i7 && i6 <= i8;
            }

            return isCorrectFormat;
        }
        private static void SetFilterOption(FilterOption option, string message1, string message2, string message3)
        {
        To1:
            Console.WriteLine(message1);
            Console.Write(new string(' ', 10) + "=> ");

            string filterIsNeed = Console.ReadLine()!;

            if (filterIsNeed is "<=") throw new Exception($"<= - {option}");
            if (filterIsNeed is "=>") throw new Exception($"=> - {option}");
            if (filterIsNeed is "<<=") throw new Exception("<<=");

            if (filterIsNeed is not null && filterIsNeed is not "")
            {
                FilterOptionSelector(option, true);

                Console.WriteLine(message2);
                Console.Write(new string(' ', 10) + "=> ");

                string value;
                bool isCorrectFormat;

                do
                {
                    value = Console.ReadLine()!;

                    if (value is "<<=") goto To1;
                    if (value is null || value is "") break;

                    isCorrectFormat = FilterIsCorrectFormatSelector(option, value);

                    if (!isCorrectFormat)
                    {
                        Console.WriteLine(message3);
                        Console.Write(new string(' ', 10) + "=> ");
                    }
                } while (!isCorrectFormat);

                if (value is null || value is "")
                    FilterOptionSelector(option, null!);
                else
                    FilterOptionSelector(option, value);
            }
            else
            {
                FilterOptionSelector(option, false);
            }
        }
        private static void SetFilterSettings()
        {
        Start:
            Console.WriteLine(OutputFormat("Включить фильтрацию?"));
            Console.Write(new string(' ', 10) + "=> ");

            string generalFilterIsNeed = Console.ReadLine()!;

            if (generalFilterIsNeed is "<=") throw new Exception($"<= - Filter");
            if (generalFilterIsNeed is "=>") throw new Exception($"=> - Filter");
            if (generalFilterIsNeed is "<<=") throw new Exception($"<<=");

            if (generalFilterIsNeed is not null && generalFilterIsNeed is not "")
            {
                Program.Settings.FilterSettings.GeneralFilterIsNeed = true;
            To1:
                try
                {
                    SetFilterOption(FilterOption.League, OutputFormat("Фильтровать по названию лиги?"), OutputFormat("Установите значение"), OutputFormat("Неверный формат. Повторите ввод"));
                }
                catch (Exception e)
                {
                    if (e.Message is "<= - League" || e.Message is "<<=") goto Start;
                    if (e.Message is "=> - League") goto To2;
                }
            To2:
                try
                {
                    SetFilterOption(FilterOption.Title, OutputFormat("Фильтровать по названию матча?"), OutputFormat("Установите значение"), OutputFormat("Неверный формат. Повторите ввод"));
                }
                catch (Exception e)
                {
                    if (e.Message is "<= - Title") goto To1;
                    if (e.Message is "=> - Title") goto To3;
                    if (e.Message is "<<=") goto Start;
                }
            To3:
                try
                {
                    SetFilterOption(FilterOption.Score, OutputFormat("Фильтровать по счёту?"), OutputFormat("Установите значение"), OutputFormat("Неверный формат. Повторите ввод"));
                }
                catch (Exception e)
                {
                    if (e.Message is "<= - Score") goto To2;
                    if (e.Message is "=> - Score") goto To4;
                    if (e.Message is "<<=") goto Start;
                }
            To4:
                try
                {
                    SetFilterOption(FilterOption.Period, OutputFormat("Фильтровать по периоду?"), OutputFormat("Установите значение"), OutputFormat("Неверный формат. Повторите ввод"));
                }
                catch (Exception e)
                {
                    if (e.Message is "<= - Period") goto To3;
                    if (e.Message is "=> - Period") goto To5;
                    if (e.Message is "<<=") goto Start;
                }
            To5:
                try
                {
                    SetFilterOption(FilterOption.Time, OutputFormat("Фильтровать по времени?"), OutputFormat("Установите значение"), OutputFormat("Неверный формат. Повторите ввод"));
                }
                catch (Exception e)
                {
                    if (e.Message is "<= - Time") goto To4;
                    if (e.Message is "=> - Time") goto To6;
                    if (e.Message is "<<=") goto Start;
                }
            To6:
                try
                {
                    SetFilterOption(FilterOption.Coefficient, OutputFormat("Фильтровать по коэффициенту?"), OutputFormat("Установите значение"), OutputFormat("Неверный формат. Повторите ввод"));
                }
                catch (Exception e)
                {
                    if (e.Message is "<= - Coefficient") goto To5;
                    if (e.Message is "=> - Coefficient") throw new Exception($"=> - Filter");
                    if (e.Message is "<<=") goto Start;
                }
            }
            else
            {
                Program.Settings.FilterSettings.GeneralFilterIsNeed = false;
            }
        }
        private static void SetNotifierOption(NotifierOption option, string message1, string message2, string message3)
        {
        To1:
            Console.WriteLine(message1);
            Console.Write(new string(' ', 10) + "=> ");

            string notifyIsNeed = Console.ReadLine()!;

            if (notifyIsNeed is "<=") throw new Exception($"<= - {option}");
            if (notifyIsNeed is "=>") throw new Exception($"=> - {option}");
            if (notifyIsNeed is "<<=") throw new Exception("<<=");

            if (notifyIsNeed is not null && notifyIsNeed is not "")
            {
                NotifierOptionSelector(option, true);

                Console.WriteLine(message2);
                Console.Write(new string(' ', 10) + "=> ");

                string value;
                bool isCorrectFormat;

                do
                {
                    value = Console.ReadLine()!;

                    if (value is "<<=") goto To1;
                    if (value is null || value is "") break;

                    isCorrectFormat = value.Length == new Regex(@"\[\d+\]").Match(value).Value.Length;

                    if (!isCorrectFormat)
                    {
                        Console.WriteLine(message3);
                        Console.Write(new string(' ', 10) + "=> ");
                    }
                } while (!isCorrectFormat);

                if (value is null || value is "")
                    NotifierOptionSelector(option, null!);
                else
                    NotifierOptionSelector(option, value);
            }
            else
            {
                NotifierOptionSelector(option, false);
            }
        }
        private static void SetNotifierSettings()
        {
        Start:
            Console.WriteLine(OutputFormat(new string('=', 96)));
            Console.WriteLine(OutputFormat("Включить уведомления?"));
            Console.Write(new string(' ', 10) + "=> ");

            string generalNotifyIsNeed = Console.ReadLine()!;

            if (generalNotifyIsNeed is "<=") throw new Exception($"<= - Notifier");
            if (generalNotifyIsNeed is "=>") throw new Exception($"=> - Notifier");
            if (generalNotifyIsNeed is "<<=") throw new Exception($"<<=");

            if (generalNotifyIsNeed is not null && generalNotifyIsNeed is not "")
            {
                Program.Settings.NotifierSettings.GeneralNotifyIsNeed = true;
            To1:
                try
                {
                    SetNotifierOption(NotifierOption.Audio, OutputFormat("Включить звуковое уведомление?"), OutputFormat("Установите значение"), OutputFormat("Неверный формат. Повторите ввод"));
                }
                catch (Exception e)
                {
                    if (e.Message is "<= - Audio" || e.Message is "<<=") goto Start;
                    if (e.Message is "=> - Audio") goto To2;
                }
            To2:
                try
                {
                    SetNotifierOption(NotifierOption.Video, OutputFormat("Включить визуальное уведомление?"), OutputFormat("Установите значение"), OutputFormat("Неверный формат. Повторите ввод"));
                }
                catch (Exception e)
                {
                    if (e.Message is "<= - Video") goto To1;
                    if (e.Message is "=> - Video") throw new Exception($"=> - Notifier");
                    if (e.Message is "<<=") goto Start;
                }
            }
            else
            {
                Program.Settings.NotifierSettings.GeneralNotifyIsNeed = false;
            }
        }
        private static void SetSpecialSettings()
        {
            Console.Clear();
            Console.WriteLine("          \n          \n          \n          \n          " + Viewer.InCenter("Специальные настройки", "–").Replace('*', '–') + "\n");
        To1:
            try
            {
                SetFilterSettings();
            }
            catch (Exception e)
            {
                if (e.Message is "<= - Filter" || e.Message is "<<=")
                {
                    Console.Clear();
                    Console.WriteLine("          \n          \n          \n          ");
                    Console.Write(new string(' ', 10) + "=> ");

                    return;
                }
                if (e.Message is "=> - Filter") goto To2;
            }
        To2:
            try
            {
                SetViewerSettings();
            }
            catch (Exception e)
            {
                if (e.Message is "<= - Viewer") goto To1;
                if (e.Message is "=> - Viewer") goto To3;

                if (e.Message is "<<=")
                {
                    Console.Clear();
                    Console.WriteLine("          \n          \n          \n          ");
                    Console.Write(new string(' ', 10) + "=> ");

                    return;
                }
            }
        To3:
            try
            {
                SetNotifierSettings();
            }
            catch (Exception e)
            {
                if (e.Message is "<= - Notifier") goto To2;

                if (e.Message is "=> - Notifier" || e.Message is "<<=")
                {
                    Console.Clear();
                    Console.WriteLine("          \n          \n          \n          ");
                    Console.Write(new string(' ', 10) + "=> ");

                    return;
                }
            }

            Console.Clear();
            Console.WriteLine("          \n          \n          \n          ");
            Console.Write(new string(' ', 10) + "=> ");
        }
        private static void SetViewerOption(ViewerOption option, string message)
        {
            Console.WriteLine(message);
            Console.Write(new string(' ', 10) + "=> ");

            string viewIsNeed = Console.ReadLine()!;

            if (viewIsNeed is "<=") throw new Exception($"<= - {option}");
            if (viewIsNeed is "=>") throw new Exception($"=> - {option}");
            if (viewIsNeed is "<<=") throw new Exception("<<=");

            if (viewIsNeed is not null && viewIsNeed is not "")
                ViewerOptionSelector(option, true);
            else
                ViewerOptionSelector(option, false);
        }
        private static void SetViewerSettings()
        {
        Start:
            Console.WriteLine(OutputFormat(new string('=', 96)));
            Console.WriteLine(OutputFormat("Включить отображение?"));
            Console.Write(new string(' ', 10) + "=> ");

            string generalViewIsNeed = Console.ReadLine()!;

            if (generalViewIsNeed is "<=") throw new Exception($"<= - Viewer");
            if (generalViewIsNeed is "=>") throw new Exception($"=> - Viewer");
            if (generalViewIsNeed is "<<=") throw new Exception($"<<=");

            if (generalViewIsNeed is not null && generalViewIsNeed is not "")
            {
                Program.Settings.ViewerSettings.GeneralViewIsNeed = true;
            To1:
                try
                {
                    SetViewerOption(ViewerOption.AllEvents, OutputFormat("Отображать текущие события?"));
                }
                catch (Exception e)
                {
                    if (e.Message is "<= - AllEvents" || e.Message is "<<=") goto Start;
                    if (e.Message is "=> - AllEvents") goto To2;
                }
            To2:
                try
                {
                    SetViewerOption(ViewerOption.NecessaryEvents, OutputFormat("Отображать подходящие события?"));
                }
                catch (Exception e)
                {
                    if (e.Message is "<= - NecessaryEvents") goto To1;
                    if (e.Message is "=> - NecessaryEvents") goto To3;
                    if (e.Message is "<<=") goto Start;
                }
            To3:
                try
                {
                    SetViewerOption(ViewerOption.TrackedEvents, OutputFormat("Отображать отслеживаемые события?"));
                }
                catch (Exception e)
                {
                    if (e.Message is "<= - NecessaryEvents") goto To2;
                    if (e.Message is "=> - NecessaryEvents") throw new Exception($"=> - Viewer");
                    if (e.Message is "<<=") goto Start;
                }
            }
            else
            {
                Program.Settings.ViewerSettings.GeneralViewIsNeed = false;
            }
        }
        private static void ShortenCommand()
        {
            if (command is "<1>") command = command.Replace("<1>", "Просмотреть список команд");
            else if (command!.Length == new Regex(@"<2> \[\d+\]").Match(command).Value.Length) command = command.Replace("<2>", "Добавить событие в отслеживаемые");
            else if (command.Length == new Regex(@"<3> \[\d+\]").Match(command).Value.Length) command = command.Replace("<3>", "Удалить событие из отслеживаемых");
            else if (command is "<4>") command = command.Replace("<4>", "Просмотреть текущие настройки");
            else if (command is "<5>") command = command.Replace("<5>", "Просмотреть все настройки");
            else if (command is "<6>") command = command.Replace("<6>", "Установить стандартные настройки");
            else if (command is "<7>") command = command.Replace("<7>", "Установить специальные настройки");
            else if (command.Length == new Regex(@"<8> \[[^\[\]]*\]").Match(command).Value.Length) command = command.Replace("<8>", "Загрузить настройки");
            else if (command.Length == new Regex(@"<9> \[[^\[\]]*\]").Match(command).Value.Length) command = command.Replace("<9>", "Удалить настройки");
            else if (command.Length == new Regex(@"<10> \[[^\[\]]*\]").Match(command).Value.Length) command = command.Replace("<10>", "Сохранить настройки");
            else if (command.Length == new Regex(@"<11> \[\d+\]").Match(command).Value.Length) command = command.Replace("<11>", "Включить режим 'не беспокоить'");
            else if (command is "<12>") command = command.Replace("<12>", "Отключить режим 'не беспокоить'");
            else if (command is "<13>") command = command.Replace("<13>", "Завершить сеанс");
        }
        
        private static bool TimeIsCorrectFormat(string value)
        {
            bool isCorrectFormat = value.Length == new Regex(@"\[(?:[0-5]\d):(?:[0-5]\d) - (?:[0-5]\d):(?:[0-5]\d)\]").Match(value).Value.Length;

            if (isCorrectFormat)
            {
                int i1 = int.Parse(value.Replace('[', ' ').Replace(']', ' ').Trim().Split("-")[0].Split(":")[0].Trim());
                int i2 = int.Parse(value.Replace('[', ' ').Replace(']', ' ').Trim().Split("-")[0].Split(":")[1].Trim());
                int i3 = int.Parse(value.Replace('[', ' ').Replace(']', ' ').Trim().Split("-")[1].Split(":")[0].Trim());
                int i4 = int.Parse(value.Replace('[', ' ').Replace(']', ' ').Trim().Split("-")[1].Split(":")[1].Trim());

                isCorrectFormat = i1 < i3 || i1 == i3 && i2 <= i4;
            }

            return isCorrectFormat;
        }
        private static void TurnDontDisturbeModeOff()
        {
            if (dontDisturbeModeIsOn)
            {
                dontDisturbeModeDisconnector = true;
            }
            else
            {
                Console.WriteLine("Режим 'не беспокоить' уже деактивирован");
                Console.Write(new string(' ', 10) + "=> ");
                Thread.Sleep(1000);
            }
        }
        private static void TurnDontDisturbeModeOn()
        {
            if (!dontDisturbeModeIsOn)
            {
                int timerValue = int.Parse(ExtractValueFromCommand());
                Program.Settings.NotifierSettings.GeneralNotifyIsNeed = false;

                Thread dontDisturbeModeTimer = new Thread(new ParameterizedThreadStart(DontDisturbeModeTimer!)) { IsBackground = true };
                dontDisturbeModeTimer.Start(timerValue);
            }
            else
            {
                Console.WriteLine("Режим 'не беспокоить' уже активирован");
                Console.Write(new string(' ', 10) + "=> ");
                Thread.Sleep(1000);
            }
        }
        
        private static void ViewerOptionSelector(ViewerOption option, bool value)
        {
            switch (option)
            {
                case ViewerOption.AllEvents:
                    Program.Settings.ViewerSettings.AllEventsViewIsNeed = value;
                    break;
                case ViewerOption.NecessaryEvents:
                    Program.Settings.ViewerSettings.NecessaryEventsViewIsNeed = value;
                    break;
                case ViewerOption.TrackedEvents:
                    Program.Settings.ViewerSettings.TrackedEventsViewIsNeed = value;
                    break;
                default:
                    break;
            }
        }
        
        private static void WaitForCommand()
        {
            var commandsWithoutArguments = from item
                                           in commands
                                           where item is not "Добавить событие в отслеживаемые" &&
                                                  item is not "Удалить событие из отслеживаемых" &&
                                                  item is not "Загрузить настройки" &&
                                                  item is not "Удалить настройки" &&
                                                  item is not "Сохранить настройки" &&
                                                  item is not "Включить режим 'не беспокоить'"
                                           select item;

            var commandsWithArguments = from item
                                        in commands
                                        where item is "Добавить событие в отслеживаемые" ||
                                               item is "Удалить событие из отслеживаемых" ||
                                               item is "Загрузить настройки" ||
                                               item is "Удалить настройки" ||
                                               item is "Сохранить настройки" ||
                                               item is "Включить режим 'не беспокоить'"
                                        select item;

            while (true)
            {
                if (!isCommandHandling)
                {
                    while (true)
                    {
                        do
                        {
                        } while (Console.ReadKey(true).Key != ConsoleKey.Enter);

                        requestForInput = true;

                        while (true)
                        {
                            if (PermissionToEnter)
                            {
                                Console.WriteLine("Введите команду");
                                Console.Write(new string(' ', 10) + "=> ");

                                command = Console.ReadLine();

                                Console.Write(new string(' ', 10) + "=> ");

                                break;
                            }
                        }

                        if (command == "")
                        {
                            Console.WriteLine("Команда не распознана");
                            Console.Write(new string(' ', 10) + "=> ");
                            Thread.Sleep(1000);

                            requestForInput = false;

                            continue;
                        }

                        ShortenCommand();

                        for (int i = 0; i < commandsWithoutArguments.Count(); i++)
                        {
                            if (command == commandsWithoutArguments.ElementAt(i)) goto Here;
                        }

                        for (int i = 0; i < commandsWithArguments.Count(); i++)
                        {
                            if (command!.Length == new Regex($"{commandsWithArguments.ElementAt(i)} \\[\\d+\\]").Match(command).Value.Length) goto Here;
                            if (command.Length == new Regex($"{commandsWithArguments.ElementAt(i)} \\[[^\\[\\]]*\\]").Match(command).Value.Length) goto Here;
                        }

                        Console.WriteLine("Команда не распознана");
                        Console.Write(new string(' ', 10) + "=> ");
                        Thread.Sleep(1000);

                        requestForInput = false;
                    }
                Here:
                    isCommandExists = true;
                    isCommandHandling = true;
                    requestForInput = false;
                }
            }
        }
    }
}