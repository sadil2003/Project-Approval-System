namespace PAS_BlindMatching.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Abstract { get; set; }
        public string TechStack { get; set; }
        public string ResearchArea { get; set; }
        public string Status { get; set; } = "Pending";

        public int StudentId { get; set; }   // Stores both Student and Group user IDs
        public int? SupervisorId { get; set; }

        // Group members JSON: [{"Name":"Ali","UserId":"12345"}, ...]
        public string? GroupMembersJson { get; set; }

        // Navigation properties
        public virtual User Student { get; set; }
        public virtual User? Supervisor { get; set; }
    }
}
