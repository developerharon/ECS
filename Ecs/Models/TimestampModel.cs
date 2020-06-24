using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Ecs.Models
{
    public class TimestampModel
    {
        public string EmployeeId { get; set; }
        public DateTime TimeStampTime { get; set; }
        public Location TimeStampLocation { get; set; }
    }
}
