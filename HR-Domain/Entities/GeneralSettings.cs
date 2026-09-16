using HR_Domain.Common;
using System;

namespace HR_Domain.Entities
{
    public class GeneralSettings : BaseEntity
    {
        public decimal OvertimeHourRate { get; set; }
        public decimal DeductionHourRate { get; set; }
        public string WeeklyDaysOff { get; set; }
         //= $"{DayOfWeek.Friday},{DayOfWeek.Saturday}";
    }
}