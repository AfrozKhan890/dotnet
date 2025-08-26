using System;
using System.ComponentModel.DataAnnotations;

namespace SymphonyLimited.Models
{
    public class Admission
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Student Name")]
        public string StudentName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        [Required]
        public string Course { get; set; }

        [Required]
        [Display(Name = "Admission Date")]
        [DataType(DataType.Date)]
        public DateTime AdmissionDate { get; set; }

        public string? Message { get; set; }
    }
}
