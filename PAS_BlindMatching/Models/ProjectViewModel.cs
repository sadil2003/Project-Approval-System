namespace PAS_BlindMatching.Models
{
    public class ProjectViewModel
    {
        public int Id { get; set; }                 // Project ID
        public string Title { get; set; }           // Project title
        public string Abstract { get; set; }        // Project abstract
        public string TechStack { get; set; }       // Technology stack used
        public string ResearchArea { get; set; }    // Project research area
        public string Status { get; set; }          // Current status of the project
        public string? SupervisorName { get; set; } // Optional: assigned supervisor name



        // Identity reveal fields
        public string? StudentName { get; set; }
        public string? StudentEmail { get; set; }

    }

}