using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FalconsRoost.Models
{
    public class LaunchLog
    {
        public Guid Id { get; set; }
        public string Version { get; set; }
        public DateTime TimeStamp { get; set; }
        public string? Message { get; set; }

        public LaunchLog()
        {
            Id = Guid.NewGuid();
            TimeStamp = DateTime.Now;
        }
    }
}
