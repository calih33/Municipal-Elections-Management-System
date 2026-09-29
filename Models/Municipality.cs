using System.ComponentModel.DataAnnotations.Schema;

namespace Municipal_Elections_Management_System.Models;

public class Municipality
{
    public int MunicipalityId { get; set; }
    public string? Name { get; set; }
    public DateTime? ElectionDate { get; set; }

    public string? Description { get; set; }

    public string? LogoImage { get; set; }

}
