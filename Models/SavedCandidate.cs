using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Municipal_Elections_Management_System.Models;

// A candidate a mobile app user saved to their voting guide.
// The public website's guide is cookie based and does not use this table.
public class SavedCandidate
{
    public int SavedCandidateId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public IdentityUser? User { get; set; }

    public int CandidateId { get; set; }

    public Candidate? Candidate { get; set; }

    public DateTime DateSaved { get; set; } = DateTime.UtcNow;
}
