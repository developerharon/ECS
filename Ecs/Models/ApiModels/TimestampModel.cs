using System;
using System.ComponentModel.DataAnnotations;
using Xamarin.Essentials;

namespace ECS.Models.ApiModels
{
    public class TimestampModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public DateTime ClockTime { get; set; }
        [Required]
        public Location ClockLocation { get; set; }
    }
}