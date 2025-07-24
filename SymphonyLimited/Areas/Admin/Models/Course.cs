using System;
using System.Collections.Generic;

namespace SymphonyLimited.Areas.Admin.Models
{
    public class Course
{
    public int Id { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public bool IsNew { get; set; }
    public string TopicsCovered { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.Now; 


    public ICollection<Admission> Admissions { get; set; } = new List<Admission>();
      
    }
}
