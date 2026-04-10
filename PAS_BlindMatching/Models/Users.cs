namespace PAS_BlindMatching.Models
{
    public class User
    {
        public int Id { get; set; }             // Primary key
        public string Name { get; set; }        // Full name
        public string Email { get; set; }       // Email
        public string Password { get; set; }    // Hashed password
        public string Role { get; set; }        // Student / Group / Supervisor / Admin
        public string? ResearchArea { get; set; } // Optional for Student/Group, Mandatory for Supervisor
    }

    public static class ResearchAreaList
    {
        public static readonly List<string> Areas = new()
        {
            "Artificial Intelligence",
            "Machine Learning",
            "Data Science & Analytics",
            "Cybersecurity",
            "Software Engineering",
            "Computer Networks",
            "Database Systems",
            "Human-Computer Interaction",
            "Cloud Computing",
            "Internet of Things (IoT)",
            "Blockchain Technology",
            "Computer Vision",
            "Natural Language Processing",
            "Embedded Systems",
            "Parallel & Distributed Computing",
            "Bioinformatics",
            "Robotics & Automation",
            "Web & Mobile Development",
            "Game Development",
            "Digital Signal Processing"
        };
    }
}
