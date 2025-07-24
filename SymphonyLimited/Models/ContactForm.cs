using System;
using System.ComponentModel.DataAnnotations;

namespace SymphonyLimited.Models
{
    public class ContactForm
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;
        public DateTime SubmissionDate { get; set; } = DateTime.Now;

    }
}
