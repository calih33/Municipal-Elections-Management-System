// This file contains the SavedCandidatesApiController class (mobile voting guide) for the Municipal Elections Management System.
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Municipal_Elections_Management_System.Data;
using Municipal_Elections_Management_System.Models;
using Municipal_Elections_Management_System.Models.Api;

namespace Municipal_Elections_Management_System.Controllers.Api;

// signed-in mobile users only; every query is limited to the user in the token
[ApiController]
[Route("api/savedcandidates")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class SavedCandidatesApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SavedCandidatesApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    // every token we issue has a "sub" claim with the user's id
    private string CurrentUserId =>
        User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? throw new InvalidOperationException("Token has no sub claim.");

    // GET: api/savedcandidates
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SavedCandidateDto>>> GetSavedCandidates()
    {
        var userId = CurrentUserId;
        var saved = await _context.SavedCandidates
            .Where(s => s.UserId == userId)
            .Include(s => s.Candidate)
                .ThenInclude(c => c!.Position)
                    .ThenInclude(p => p!.Municipality)
            .OrderBy(s => s.Candidate!.Position!.Municipality!.Name)
            .ThenBy(s => s.Candidate!.Position!.Name)
            .ThenBy(s => s.Candidate!.Name)
            .ToListAsync();

        return saved.Select(s => SavedCandidateDto.From(s, Request)).ToList();
    }

    // POST: api/savedcandidates  { "candidateId": 5 }
    [HttpPost]
    public async Task<ActionResult<SavedCandidateDto>> SaveCandidate(SaveCandidateRequest request)
    {
        var candidate = await _context.Candidates
            .Include(c => c.Position)
                .ThenInclude(p => p!.Municipality)
            .FirstOrDefaultAsync(c => c.CandidateId == request.CandidateId);

        if (candidate == null)
        {
            return Problem(title: "Candidate not found.", statusCode: StatusCodes.Status404NotFound);
        }

        var userId = CurrentUserId;
        if (await _context.SavedCandidates.AnyAsync(s => s.UserId == userId && s.CandidateId == candidate.CandidateId))
        {
            return Problem(title: "This candidate is already in your voting guide.", statusCode: StatusCodes.Status409Conflict);
        }

        var saved = new SavedCandidate { UserId = userId, CandidateId = candidate.CandidateId, Candidate = candidate };
        _context.SavedCandidates.Add(saved);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, SavedCandidateDto.From(saved, Request));
    }

    // DELETE: api/savedcandidates/5  (5 is the savedCandidateId)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> RemoveSavedCandidate(int id)
    {
        // another user's id looks the same as a missing one
        var userId = CurrentUserId;
        var saved = await _context.SavedCandidates
            .FirstOrDefaultAsync(s => s.SavedCandidateId == id && s.UserId == userId);

        if (saved == null)
        {
            return Problem(title: "Saved candidate not found.", statusCode: StatusCodes.Status404NotFound);
        }

        _context.SavedCandidates.Remove(saved);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
