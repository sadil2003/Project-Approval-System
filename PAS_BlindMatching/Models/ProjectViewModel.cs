namespace PAS_BlindMatching.Models
{
    public class ProjectViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Abstract { get; set; }
        public string TechStack { get; set; }
        public string ResearchArea { get; set; }
        public string Status { get; set; }
        public string? SupervisorName { get; set; }

        // Identity reveal fields
        public string? StudentName { get; set; }
        public string? StudentEmail { get; set; }

        // Group members (deserialized for display)
        public List<GroupMemberInfo>? GroupMembers { get; set; }
    }

    public class GroupMemberInfo
    {
        public string Name { get; set; }
        public string UserId { get; set; }
    }
}
