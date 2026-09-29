using System.ComponentModel.DataAnnotations.Schema;

namespace Municipal_Elections_Management_System.Models;

public class Candidate
{
    public int CandidateId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Position { get; set; }
    public string? URL { get; set; }
    public string? Image { get; set; }
    public string? PositionId { get; set; }

    [ForeignKey("PositionId")]
    public string? MunicipalityId { get; set; }

    [ForeignKey("MunicipalityId")]
    public Municipality? Municipality { get; set; }
}
