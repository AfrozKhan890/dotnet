using System;
using System.ComponentModel.DataAnnotations;

namespace SymphonyLimited.Models
{
    public class CenterLocation
    {
        public int Id { get; set; }
        public string CenterName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Timings { get; set; } = string.Empty;
    }
}
