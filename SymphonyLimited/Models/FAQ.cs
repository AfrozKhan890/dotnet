using System;
using System.ComponentModel.DataAnnotations;

namespace SymphonyLimited.Models
{
    public class FAQ
    {
        public int Id { get; set; }

        public string Question { get; set; } 

        public string Answer { get; set; } 
    }
}
