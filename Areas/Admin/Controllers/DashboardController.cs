using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Municipal_Elections_Management_System.Data;
using Municipal_Elections_Management_System.Services;

namespace Municipal_Elections_Management_System.Areas.Admin.Controllers;

// admins only
[Area("Admin")]
[Authorize(Policy = AppRoles.AdminPolicy)]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IMunicipalityAccessService _access;

    public DashboardController(ApplicationDbContext context, IMunicipalityAccessService access)
    {
        _context = context;
        _access = access;
    }

    // GET: Admin/Dashboard
    public async Task<IActionResult> Index()
    {
        // super admin gets all, others only their own
        var ids = await _access.GetManageableMunicipalityIdsAsync(User);

        ViewData["MunicipalityCount"] = ids.Count;

        ViewData["PositionCount"] = await _context.Positions
            .CountAsync(p => ids.Contains(p.MunicipalityId!.Value));

        ViewData["CandidateCount"] = await _context.Candidates
            .CountAsync(c => ids.Contains(c.Position!.MunicipalityId!.Value));

        return View();
    }
}
