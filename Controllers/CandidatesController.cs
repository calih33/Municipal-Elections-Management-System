using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Municipal_Elections_Management_System.Data;

namespace Municipal_Elections_Management_System.Controllers;

public class CandidatesController : Controller
{
    private readonly ApplicationDbContext _context;

    public CandidatesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Candidates
    // filter values come from the url
    public async Task<IActionResult> Index(string? search, int? municipalityId, int? positionId)
    {
        var candidates = _context.Candidates
            .Include(c => c.Position)
                .ThenInclude(p => p!.Municipality)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            // lower case so "chen" finds "Chen"
            var name = search.Trim().ToLower();
            candidates = candidates.Where(c => c.Name.ToLower().Contains(name));
        }

        // filter by municipality
        if (municipalityId != null)
        {
            candidates = candidates.Where(c => c.Position!.MunicipalityId == municipalityId);
        }

        if (positionId != null)
        {
            candidates = candidates.Where(c => c.PositionId == positionId);
        }

        // filter dropdowns
        ViewData["MunicipalityId"] = new SelectList(
            await _context.Municipalities.OrderBy(m => m.Name).ToListAsync(),
            "MunicipalityId", "Name", municipalityId);

        var positions = _context.Positions.AsQueryable();
        // only that municipality's positions
        if (municipalityId != null)
        {
            positions = positions.Where(p => p.MunicipalityId == municipalityId);
        }

        ViewData["PositionId"] = new SelectList(
            await positions
                .OrderBy(p => p.Municipality!.Name).ThenBy(p => p.Name)
                .Select(p => new { p.PositionId, Label = p.Municipality!.Name + " - " + p.Name })
                .ToListAsync(),
            "PositionId", "Label", positionId);

        ViewData["Search"] = search;

        return View(await candidates
            .OrderBy(c => c.Position!.Municipality!.Name)
            .ThenBy(c => c.Position!.Name)
            .ThenBy(c => c.Name)
            .ToListAsync());
    }

    // GET: Candidates/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var candidate = await _context.Candidates
            .Include(c => c.Position)
                .ThenInclude(p => p!.Municipality)
            .FirstOrDefaultAsync(c => c.CandidateId == id);

        if (candidate == null)
        {
            return NotFound();
        }

        return View(candidate);
    }
}
