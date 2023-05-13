using System;

namespace Marathon_Bet.Models
{
    [Serializable]
    public class FilterSettings
    {
        public FilterSettings()
        {
            GeneralFilterIsNeed = true;
            LeagueFilterIsNeed = false;
            TitleFilterIsNeed = false;
            ScoreFilterIsNeed = true;
            PeriodFilterIsNeed = true;
            TimeFilterIsNeed = true;
            CoefficientFilterIsNeed = true;
            LeagueValue = null;
            TitleValue = null;
            ScoreValue = null;
            PeriodValue = null;
            TimeValue = null;
            CoefficientValue = null;
        }

        private bool generalFilterIsNeed;
        private bool leagueFilterIsNeed;
        private bool titleFilterIsNeed;
        private bool scoreFilterIsNeed;
        private bool periodFilterIsNeed;
        private bool timeFilterIsNeed;
        private bool coefficientFilterIsNeed;
        private string? leagueValue;
        private string? titleValue;
        private string? scoreValue;
        private string? periodValue;
        private string? timeValue;
        private string? coefficientValue;

        public bool GeneralFilterIsNeed
        {
            get => generalFilterIsNeed;
            set
            {
                generalFilterIsNeed = value;
                if (!generalFilterIsNeed)
                {
                    LeagueFilterIsNeed = false;
                    TitleFilterIsNeed = false;
                    ScoreFilterIsNeed = false;
                    PeriodFilterIsNeed = false;
                    TimeFilterIsNeed = false;
                    CoefficientFilterIsNeed = false;
                    LeagueValue = null;
                    TitleValue = null;
                    ScoreValue = null;
                    PeriodValue = null;
                    TimeValue = null;
                    CoefficientValue = null;
                }
            }
        }
        public bool LeagueFilterIsNeed
        {
            get => leagueFilterIsNeed;
            set
            {
                leagueFilterIsNeed = value;
                if (!leagueFilterIsNeed) leagueValue = null;
            }
        }
        public bool TitleFilterIsNeed
        {
            get => titleFilterIsNeed;
            set
            {
                titleFilterIsNeed = value;
                if (!titleFilterIsNeed) TitleValue = null;
            }
        }
        public bool ScoreFilterIsNeed
        {
            get => scoreFilterIsNeed;
            set
            {
                scoreFilterIsNeed = value;
                if (!scoreFilterIsNeed) ScoreValue = null;
            }
        }
        public bool PeriodFilterIsNeed
        {
            get => periodFilterIsNeed;
            set
            {
                periodFilterIsNeed = value;
                if (!periodFilterIsNeed) PeriodValue = null;
            }
        }
        public bool TimeFilterIsNeed
        {
            get => timeFilterIsNeed;
            set
            {
                timeFilterIsNeed = value;
                if (!timeFilterIsNeed) TimeValue = null;
            }
        }
        public bool CoefficientFilterIsNeed
        {
            get => coefficientFilterIsNeed;
            set
            {
                coefficientFilterIsNeed = value;
                if (!coefficientFilterIsNeed) CoefficientValue = null;
            }
        }
        public string? LeagueValue
        {
            get => leagueValue;
            set => leagueValue = value is null ? "[]" : value;
        }
        public string? TitleValue
        {
            get => titleValue;
            set => titleValue = value is null ? "[]" : value;
        }
        public string? ScoreValue
        {
            get => scoreValue;
            set => scoreValue = value is null ? "[0:0 - 0:100 || 0:0 - 100:0]" : value;
        }
        public string? PeriodValue
        {
            get => periodValue;
            set => periodValue = value is null ? "[1 - 2]" : value;
        }
        public string? TimeValue
        {
            get => timeValue;
            set => timeValue = value is null ? "[00:00 - 30:00]" : value;
        }
        public string? CoefficientValue
        {
            get => coefficientValue;
            set => coefficientValue = value is null ? "[1.2 - 100.0]" : value;
        }
    }
}