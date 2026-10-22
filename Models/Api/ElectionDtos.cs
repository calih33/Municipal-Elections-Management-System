using System.ComponentModel.DataAnnotations;

namespace Municipal_Elections_Management_System.Models.Api;

// What the API returns instead of the EF entities, so admin-only fields (AdminRoleName)
// never leave the server. Names follow the COMP4977 spec: Biography, PhotoUrl, LogoUrl.

public record MunicipalityDto(
    int MunicipalityId,
    string Name,
    string Province,
    DateOnly ElectionDate,
    string? Description,
    string? LogoUrl,
    IReadOnlyList<PositionDto> Positions)
{
    // needs Positions loaded
    public static MunicipalityDto From(Municipality m, HttpRequest request) => new(
        m.MunicipalityId,
        m.Name,
        m.Province,
        DateOnly.FromDateTime(m.ElectionDate),
        m.Description,
        ApiUrls.Upload(request, m.LogoFileName),
        m.Positions.OrderBy(p => p.Name).Select(p => PositionDto.From(p, m)).ToList());
}

public record PositionDto(
    int PositionId,
    string Name,
    int SeatsAvailable,
    int MunicipalityId,
    string MunicipalityName)
{
    public static PositionDto From(Position p, Municipality m) =>
        new(p.PositionId, p.Name, p.SeatsAvailable, m.MunicipalityId, m.Name);
}

// MunicipalityId comes from Position.Municipality; Candidate has no MunicipalityId column
public record CandidateDto(
    int CandidateId,
    string Name,
    string Biography,
    string? PhotoUrl,
    string? Website,
    string? Email,
    int PositionId,
    string PositionName,
    int MunicipalityId,
    string MunicipalityName)
{
    // needs Position.Municipality loaded
    public static CandidateDto From(Candidate c, HttpRequest request)
    {
        var position = c.Position ?? throw new InvalidOperationException("Load Candidate.Position before mapping.");
        var municipality = position.Municipality ?? throw new InvalidOperationException("Load Position.Municipality before mapping.");

        return new(
            c.CandidateId,
            c.Name,
            c.Profile,
            ApiUrls.Upload(request, c.PhotoFileName),
            c.Website,
            c.Email,
            position.PositionId,
            position.Name,
            municipality.MunicipalityId,
            municipality.Name);
    }
}

public record SavedCandidateDto(int SavedCandidateId, int CandidateId, DateTime DateSaved, CandidateDto Candidate)
{
    public static SavedCandidateDto From(SavedCandidate s, HttpRequest request)
    {
        var candidate = s.Candidate ?? throw new InvalidOperationException("Load SavedCandidate.Candidate before mapping.");

        // whole seconds in UTC ("2026-10-22T18:03:12Z") so Swift's .iso8601 decoder can read it
        var dateSaved = new DateTime(s.DateSaved.Ticks - s.DateSaved.Ticks % TimeSpan.TicksPerSecond, DateTimeKind.Utc);

        return new(s.SavedCandidateId, s.CandidateId, dateSaved, CandidateDto.From(candidate, request));
    }
}

// POST: api/savedcandidates
public class SaveCandidateRequest
{
    [Required]
    public int? CandidateId { get; set; }
}

public static class ApiUrls
{
    // full url for an uploaded image (served at /uploads), or null if there isn't one
    public static string? Upload(HttpRequest request, string? fileName) =>
        string.IsNullOrEmpty(fileName)
            ? null
            : $"{request.Scheme}://{request.Host}{request.PathBase}/uploads/{fileName}";
}
