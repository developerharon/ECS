using Microsoft.EntityFrameworkCore;
using System;

namespace Ecs.Entities
{
    [Owned]
    public class TimeStamp
    {
        public DateTime In { get; set; }
        public bool InWhileOnPremises { get; set; }
        public DateTime? Out { get; set; }
        public bool? OutWhileOnPremises { get; set; }
        public bool IsActive { get; set; }
    }
}