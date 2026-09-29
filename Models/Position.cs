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

    public PositionType? Type { get; set; }

    public string? MunicipalityId { get; set; }

    [ForeignKey("MunicipalityId")]
    public Municipality? Municipality { get; set; }
}
