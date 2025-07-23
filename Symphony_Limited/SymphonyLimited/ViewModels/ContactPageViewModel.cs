using SymphonyLimited.Models;
using System.Collections.Generic;

namespace SymphonyLimited.ViewModels
{
    public class ContactPageViewModel
    {
        public IEnumerable<CenterLocation> Centers { get; set; } = new List<CenterLocation>();
        public ContactForm ContactForm { get; set; } = new ContactForm();
    }
}
