// This file contains the CandidateController class for the Municipal Elections Management System.
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Municipal_Elections_Management_System.Data;

namespace Municipal_Elections_Management_System.Controllers;

public class CandidateController : Controller
{
    private readonly ApplicationDbContext _context;

    public CandidateController(ApplicationDbContext context)
    {
        _context = context;
    }

    [ActionName("Index")]
    public async Task<IActionResult> IndexAsync()
    {
        var candidates = await _context.Candidates
            .Include(candidate => candidate.Municipality)
            .AsNoTracking()
            .ToListAsync();

        return View(candidates);
    }
}