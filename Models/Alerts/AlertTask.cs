using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FalconsRoost.Models.Alerts
{
    public class AlertTask
    {
        public Guid Id { get; set; }
        public List<AlertMessage> AlertMessages { get; set; } = new List<AlertMessage>();
        public AlertType AlertType { get; set; }
        public Recurrence RecurrenceUnit { get; set; }
        public int RecurrenceInterval { get; set; }
        public int DayToRunOn { get; set; }
        public int HourStartTime { get; set; }
        public int HourEndTime { get; set; }
        public DateTime LastRun { get; set; }
        public DateTime NextRun { get; set; }
        public bool Enabled { get; set; }
        public bool RunOnce { get; set; }
        public bool RunOnStart { get; set; }
        public bool CurrentlyRunning { get; set; } = false;

        public bool ShouldRun()
        {
            var centralTimeZone = TimeZoneInfo.FindSystemTimeZoneById(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Central Standard Time" : "America/Chicago");
            var centralTime = TimeZoneInfo.ConvertTime(DateTime.Now, centralTimeZone);
            if (Enabled && !CurrentlyRunning && (RunOnStart || (centralTime.Hour >= HourStartTime && centralTime.Hour < HourEndTime && (DayToRunOn == -1 || (int)centralTime.DayOfWeek == DayToRunOn))))
            {
                return true;
            }
            return RunOnce;
        }
    }

    public enum AlertType
    {
        MCSNCBD,
        MCSRatio
    }

    public enum Recurrence
    {
        Second,
        Minute,
        Daily,
        Weekly,
        Monthly,
        Yearly
    }
}
