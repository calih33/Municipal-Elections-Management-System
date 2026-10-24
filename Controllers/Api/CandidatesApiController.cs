// This file contains the CandidatesApiController class for the Municipal Elections Management System.
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Municipal_Elections_Management_System.Data;
using Municipal_Elections_Management_System.Models;
using Municipal_Elections_Management_System.Models.Api;

namespace Municipal_Elections_Management_System.Controllers.Api;

// public, no login needed
[ApiController]
[Route("api/candidates")]
public class CandidatesApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CandidatesApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/candidates?municipalityId=1&positionId=2
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CandidateDto>>> GetCandidates(int? municipalityId, int? positionId)
    {
        return await ListAsync(null, municipalityId, positionId);
    }

    // GET: api/candidates/search?name=chen (municipalityId / positionId filters work here too)
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<CandidateDto>>> SearchCandidates(string? name, int? municipalityId, int? positionId)
    {
        return await ListAsync(name, municipalityId, positionId);
    }

    // GET: api/candidates/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CandidateDto>> GetCandidate(int id)
    {
        var candidate = await _context.Candidates
            .Include(c => c.Position)
                .ThenInclude(p => p!.Municipality)
            .FirstOrDefaultAsync(c => c.CandidateId == id);

        if (candidate == null)
        {
            return Problem(title: "Candidate not found.", statusCode: StatusCodes.Status404NotFound);
        }

        return CandidateDto.From(candidate, Request);
    }

    // same filters as the public Candidates page
    private async Task<List<CandidateDto>> ListAsync(string? name, int? municipalityId, int? positionId)
    {
        IQueryable<Candidate> candidates = _context.Candidates
            .Include(c => c.Position)
                .ThenInclude(p => p!.Municipality);

        if (!string.IsNullOrWhiteSpace(name))
        {
            // lower case so "chen" finds "Chen"
            var search = name.Trim().ToLower();
            candidates = candidates.Where(c => c.Name.ToLower().Contains(search));
        }

        if (municipalityId != null)
        {
            candidates = candidates.Where(c => c.Position!.MunicipalityId == municipalityId);
        }

        if (positionId != null)
        {
            candidates = candidates.Where(c => c.PositionId == positionId);
        }

        var list = await candidates
            .OrderBy(c => c.Position!.Municipality!.Name)
            .ThenBy(c => c.Position!.Name)
            .ThenBy(c => c.Name)
            .ToListAsync();

        return list.Select(c => CandidateDto.From(c, Request)).ToList();
    }
}
