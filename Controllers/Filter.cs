using Marathon_Bet.Models;
using System.Collections.Generic;
using System.Linq;

namespace Marathon_Bet.Controllers
{
    public static class Filter
    {
        public static List<Event> GetNecessaryEvents()
        {
            List<Event> necessaryEvents = new List<Event>(Program.AllEvents);

            if (Program.Settings.FilterSettings is not null && Program.Settings.FilterSettings.GeneralFilterIsNeed)
            {
                if (Program.Settings.FilterSettings.LeagueFilterIsNeed) FilterByLeagueAndTitle(FilterOption.League, Program.Settings.FilterSettings.LeagueValue, necessaryEvents);
                if (Program.Settings.FilterSettings.TitleFilterIsNeed) FilterByLeagueAndTitle(FilterOption.Title, Program.Settings.FilterSettings.TitleValue, necessaryEvents);
                if (Program.Settings.FilterSettings.ScoreFilterIsNeed) FilterByScore(necessaryEvents);
                if (Program.Settings.FilterSettings.PeriodFilterIsNeed) FilterByPeriod(necessaryEvents);
                if (Program.Settings.FilterSettings.TimeFilterIsNeed) FilterByTime(necessaryEvents);
                if (Program.Settings.FilterSettings.CoefficientFilterIsNeed) FilterByCoefficient(necessaryEvents);
            }

            return necessaryEvents;
        }
        private static void FilterByLeagueAndTitle(FilterOption option, string? filterCriteria, List<Event> necessaryEvents)
        {
            List<Event> filterResult = new List<Event>();

            string[] splitFilterCriteria;

            if (!filterCriteria!.Contains('*'))
            {
                splitFilterCriteria = new string[1];
                splitFilterCriteria[0] = filterCriteria.Replace('[', ' ').Replace(']', ' ').Trim();
            }
            else
            {
                splitFilterCriteria = filterCriteria.Split('*');

                for (int i = 0; i < splitFilterCriteria.Length; i++)
                    splitFilterCriteria[i] = splitFilterCriteria[i].Replace('[', ' ').Replace(']', ' ').Trim();
            }

            for (int i = 0; i < splitFilterCriteria.Length; i++)
            {
                switch (option)
                {
                    case FilterOption.League:
                        filterResult = (from item 
                                        in necessaryEvents 
                                        where item.League == splitFilterCriteria[i]
                                        select item).Distinct().ToList();
                        break;
                    case FilterOption.Title:
                        filterResult = (from item 
                                        in necessaryEvents 
                                        where item.Title == splitFilterCriteria[i]
                                        select item).Distinct().ToList();
                        break;
                    default:
                        break;
                }
            }

            necessaryEvents.Clear();

            foreach (Event item in filterResult)
                necessaryEvents.Add(item);
        }
        private static void FilterByScore(List<Event> necessaryEvents)
        {
            List<Event> filterResult = new List<Event>();

            string filterCriteria = Program.Settings.FilterSettings.ScoreValue!;
            int[] splitFilterCriteria = new int[8];
            int count = 0;

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        splitFilterCriteria[count] = int.Parse(filterCriteria.Replace('[', ' ').Replace(']', ' ').Trim().Split("||")[i].Split("-")[j].Split(":")[k].Trim());
                        count++;
                    }
                }
            }

            filterResult = (from item 
                            in necessaryEvents 
                            where (((splitFilterCriteria[0] <= int.Parse(item.Score!.Split(':')[0]) && int.Parse(item.Score.Split(':')[0]) <= splitFilterCriteria[2]) && 
                                    (splitFilterCriteria[1] <= int.Parse(item.Score.Split(':')[1]) && int.Parse(item.Score.Split(':')[1]) <= splitFilterCriteria[3])) || 
                                   ((splitFilterCriteria[4] <= int.Parse(item.Score.Split(':')[0]) && int.Parse(item.Score.Split(':')[0]) <= splitFilterCriteria[6]) && 
                                    (splitFilterCriteria[5] <= int.Parse(item.Score.Split(':')[1]) && int.Parse(item.Score.Split(':')[1]) <= splitFilterCriteria[7]))) 
                            select item).Distinct().ToList();

            necessaryEvents.Clear();

            foreach (Event item in filterResult)
                necessaryEvents.Add(item);
        }
        private static void FilterByPeriod(List<Event> necessaryEvents)
        {
            List<Event> filterResult = new List<Event>();

            string filterCriteria = Program.Settings.FilterSettings.PeriodValue!;
            int[] splitFilterCriteria = new int[2];

            for (int i = 0; i < 2; i++)
            {
                splitFilterCriteria[i] = int.Parse(filterCriteria!.Replace('[', ' ').Replace(']', ' ').Trim().Split("-")[i].Trim());
            }

            filterResult = (from item 
                            in necessaryEvents 
                            where (splitFilterCriteria[0] <= int.Parse(item.Period!) && int.Parse(item.Period!) <= splitFilterCriteria[1]) 
                            select item).Distinct().ToList();

            necessaryEvents.Clear();

            foreach (Event item in filterResult)
                necessaryEvents.Add(item);
        }
        private static void FilterByTime(List<Event> necessaryEvents)
        {
            List<Event> filterResult = new List<Event>();

            string filterCriteria = Program.Settings.FilterSettings.TimeValue!;
            int[] splitFilterCriteria = new int[4];
            int count = 0;

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    splitFilterCriteria[count] = int.Parse(filterCriteria.Replace('[', ' ').Replace(']', ' ').Trim().Split("-")[i].Split(":")[j].Trim());
                    count++;
                }
            }

            filterResult = (from item
                            in necessaryEvents
                            where (item.Time == "Пер." || 
                                  (splitFilterCriteria[0] < int.Parse(item.Time!.Split(':')[0]) && int.Parse(item.Time.Split(':')[0]) < splitFilterCriteria[2]) || 
                                  (splitFilterCriteria[0] == int.Parse(item.Time.Split(':')[0]) && int.Parse(item.Time.Split(':')[0]) == splitFilterCriteria[2] && 
                                   splitFilterCriteria[1] <= int.Parse(item.Time.Split(':')[1]) && int.Parse(item.Time.Split(':')[1]) <= splitFilterCriteria[3]) || 
                                  (splitFilterCriteria[0] == int.Parse(item.Time.Split(':')[0]) && splitFilterCriteria[1] <= int.Parse(item.Time.Split(':')[1]) && int.Parse(item.Time.Split(':')[1]) <= 59) || 
                                  (int.Parse(item.Time.Split(':')[0]) == splitFilterCriteria[2] && 0 <= int.Parse(item.Time.Split(':')[1]) && int.Parse(item.Time.Split(':')[1]) <= splitFilterCriteria[3]))
                            select item).Distinct().ToList();

            necessaryEvents.Clear();

            foreach (Event item in filterResult)
                necessaryEvents.Add(item);
        }
        private static void FilterByCoefficient(List<Event> necessaryEvents)
        {
            List<Event> filterResult = new List<Event>();

            string filterCriteria = Program.Settings.FilterSettings.CoefficientValue!;
            double[] splitFilterCriteria = new double[2];

            for (int i = 0; i < 2; i++)
            {
                splitFilterCriteria[i] = double.Parse(filterCriteria.Replace('.', ',').Replace('[', ' ').Replace(']', ' ').Trim().Split("-")[i].Trim());
            }

            filterResult = (from item 
                            in necessaryEvents 
                            where (splitFilterCriteria[0] <= double.Parse(item.Coefficient!.Replace('.', ',')) && double.Parse(item.Coefficient.Replace('.', ',')) <= splitFilterCriteria[1]) 
                            select item).Distinct().ToList();

            necessaryEvents.Clear();

            foreach (Event item in filterResult)
                necessaryEvents.Add(item);
        }
    }
}