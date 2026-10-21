// This file contains the MunicipalitiesController class for the Municipal Elections Management System.
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Municipal_Elections_Management_System.Data;

namespace Municipal_Elections_Management_System.Controllers;

public class MunicipalitiesController : Controller
{
    private readonly ApplicationDbContext _context;

    public MunicipalitiesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Municipalities
    public async Task<IActionResult> Index()
    {
        // include the positions so the card can list them
        var municipalities = await _context.Municipalities
            .Include(m => m.Positions)
            .OrderBy(m => m.Name)
            .ToListAsync();

        return View(municipalities);
    }

    // GET: Municipalities/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var municipality = await _context.Municipalities
            .Include(m => m.Positions)
                .ThenInclude(p => p.Candidates)
            .FirstOrDefaultAsync(m => m.MunicipalityId == id);

        if (municipality == null)
        {
            return NotFound();
        }

        return View(municipality);
    }
}
