using System;
using System.ComponentModel.DataAnnotations;
namespace SymphonyLimited.Models
{
    public class AboutInfo
    {
        public int Id { get; set; }
        public string InstitutionName { get; set; } = string.Empty;
        public string Vision { get; set; } = string.Empty;
        public string Mission { get; set; } = string.Empty;
        public string CoreValues { get; set; } = string.Empty;
        public string FounderMessage { get; set; } = string.Empty;
        public string FounderName { get; set; } = string.Empty;
        public string History { get; set; } = string.Empty;
        public string InstitutionImagePath { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}