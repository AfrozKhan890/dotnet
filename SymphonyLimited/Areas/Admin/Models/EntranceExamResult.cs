namespace SymphonyLimited.Areas.Admin.Models
{
    public class EntranceExamResult
    {
        public int Id { get; set; }
        public string StudentName { get; set; }
        public string RollNumber { get; set; }
        public int Marks { get; set; }
        public string ClassAllotted { get; set; }
        public decimal FeeAmount { get; set; }
        public DateTime PaymentDueDate { get; set; }
        public DateTime ExamDate { get; set; }

    }
}
