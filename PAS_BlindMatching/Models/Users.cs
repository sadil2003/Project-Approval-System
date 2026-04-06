namespace PAS_BlindMatching.Models
{
    public class User
    {
        public int Id { get; set; }             // Primary key
        public string Name { get; set; }        // Full name
        public string Email { get; set; }       // Email
        public string Password { get; set; }    // Hashed password
        public string Role { get; set; }        // Student / Supervisor / Admin
        public string? ResearchArea { get; set; } // Optional
    }
}