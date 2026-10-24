using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Municipal_Elections_Management_System.Data;
using Municipal_Elections_Management_System.Models;
using Municipal_Elections_Management_System.Services;

namespace Municipal_Elections_Management_System.Controllers;

public class VotingGuideController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly VotingGuideService _votingGuide;

    public VotingGuideController(ApplicationDbContext context, VotingGuideService votingGuide)
    {
        _context = context;
        _votingGuide = votingGuide;
    }

    // GET: VotingGuide
    public async Task<IActionResult> Index()
    {
        return View(await GetSelectedCandidatesAsync());
    }

    // GET: VotingGuide/Print
    public async Task<IActionResult> Print()
    {
        // azure is in UTC, use vancouver time
        ViewData["PrintDate"] = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "America/Vancouver");

        return View(await GetSelectedCandidatesAsync());
    }

    // POST: VotingGuide/Add/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int id, string? returnUrl)
    {
        var candidate = await _context.Candidates.FirstOrDefaultAsync(c => c.CandidateId == id);
        if (candidate == null)
        {
            return NotFound();
        }

        // save id in the cookie
        _votingGuide.Add(id);
        TempData["Message"] = $"{candidate.Name} was added to your voting guide.";

        return RedirectBack(returnUrl);
    }

    // POST: VotingGuide/Remove/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int id, string? returnUrl)
    {
        _votingGuide.Remove(id);
        TempData["Message"] = "Candidate removed from your voting guide.";

        return RedirectBack(returnUrl);
    }

    // POST: VotingGuide/Clear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        _votingGuide.Clear();
        TempData["Message"] = "Your voting guide is now empty.";

        return RedirectToAction(nameof(Index));
    }

    // go back to the page the user was on (our site only)
    private IActionResult RedirectBack(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    // picked candidates with position and municipality
    private async Task<List<Candidate>> GetSelectedCandidatesAsync()
    {
        var ids = _votingGuide.GetIds();

        return await _context.Candidates
            .Include(c => c.Position)
                .ThenInclude(p => p!.Municipality)
            .Where(c => ids.Contains(c.CandidateId))
            .OrderBy(c => c.Position!.Municipality!.Name)
            .ThenBy(c => c.Position!.Name)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }
}
