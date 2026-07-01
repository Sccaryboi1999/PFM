using System;

namespace PersonalFinanceTracker.Models
{
    public class ReminderSettings
    {
        public ReminderInterval Interval { get; set; }
        public DateTime LastReviewedUtc { get; set; }
        public DateTime LastReminderShownUtc { get; set; }

        public ReminderSettings()
        {
            Interval = ReminderInterval.Weekly;
            LastReviewedUtc = DateTime.UtcNow;
            LastReminderShownUtc = DateTime.MinValue;
        }
    }
}
