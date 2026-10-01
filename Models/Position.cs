using System.ComponentModel.DataAnnotations.Schema;

namespace Municipal_Elections_Management_System.Models;

public class Position
{
    public int PositionId { get; set; }

    public enum PositionType
    {
        Mayor,
        Councillor,
        SchoolTrustee
    }

    // add the number of positions available for this type
    public int NumberOfPositions { get; set; }

    public PositionType? Type { get; set; }
    public string? Description { get; set; }
    public int? MunicipalityId { get; set; }

    [ForeignKey("MunicipalityId")]
    public Municipality? Municipality { get; set; }
}
