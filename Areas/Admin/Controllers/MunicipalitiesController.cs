using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Municipal_Elections_Management_System.Data;
using Municipal_Elections_Management_System.Models;
using Municipal_Elections_Management_System.Services;

namespace Municipal_Elections_Management_System.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = AppRoles.AdminPolicy)]
public class MunicipalitiesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IMunicipalityAccessService _access;
    private readonly ImageStorageService _images;
    private readonly RoleManager<IdentityRole> _roleManager;

    public MunicipalitiesController(ApplicationDbContext context, IMunicipalityAccessService access,
        ImageStorageService images, RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _access = access;
        _images = images;
        _roleManager = roleManager;
    }

    // GET: Admin/Municipalities
    public async Task<IActionResult> Index()
    {
        // only this admin's municipalities
        var ids = await _access.GetManageableMunicipalityIdsAsync(User);

        var municipalities = await _context.Municipalities
            .Include(m => m.Positions)
            .Where(m => ids.Contains(m.MunicipalityId))
            .OrderBy(m => m.Name)
            .ToListAsync();

        return View(municipalities);
    }

    // GET: Admin/Municipalities/Details/5
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

        // not this admin's municipality
        if (!await _access.CanManageAsync(User, municipality.MunicipalityId))
        {
            return Forbid();
        }

        return View(municipality);
    }

    // GET: Admin/Municipalities/Create
    // super admin only
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public IActionResult Create()
    {
        return View(new Municipality { ElectionDate = new DateTime(2026, 10, 17) });
    }

    // POST: Admin/Municipalities/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> Create([Bind("Name,Province,ElectionDate,Description,LogoFile")] Municipality municipality)
    {
        // admin group, e.g. "Surrey Admin"
        var roleName = AppRoles.ForMunicipality(municipality.Name);

        // lower case so "surrey" and "Surrey" count as the same name
        var name = municipality.Name.Trim().ToLower();
        var role = roleName.ToLower();

        if (await _context.Municipalities.AnyAsync(m => m.Name.ToLower() == name))
        {
            ModelState.AddModelError("Name", "A municipality with this name already exists.");
        }
        else if (await _context.Municipalities.AnyAsync(m => m.AdminRoleName.ToLower() == role))
        {
            // a renamed municipality can still use this admin group name
            ModelState.AddModelError("Name", $"The admin group \"{roleName}\" is already used by another municipality.");
        }

        // server check for the logo
        var logoError = _images.CheckImage(municipality.LogoFile);
        if (logoError != null)
        {
            ModelState.AddModelError("LogoFile", logoError);
        }

        if (ModelState.IsValid)
        {
            municipality.AdminRoleName = roleName;

            if (municipality.LogoFile != null)
            {
                municipality.LogoFileName = await _images.SaveAsync(municipality.LogoFile, "logos");
            }

            _context.Add(municipality);
            await _context.SaveChangesAsync();

            // create the admin group role (same as the seed identity lecture)
            if (await _roleManager.FindByNameAsync(roleName) == null)
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }

            TempData["Message"] = $"{municipality.Name} was created.";
            return RedirectToAction(nameof(Index));
        }

        return View(municipality);
    }

    // GET: Admin/Municipalities/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var municipality = await _context.Municipalities.FindAsync(id);
        if (municipality == null)
        {
            return NotFound();
        }

        // not this admin's municipality
        if (!await _access.CanManageAsync(User, municipality.MunicipalityId))
        {
            return Forbid();
        }

        return View(municipality);
    }

    // POST: Admin/Municipalities/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("MunicipalityId,Name,Province,ElectionDate,Description,LogoFile")] Municipality municipality)
    {
        if (id != municipality.MunicipalityId)
        {
            return NotFound();
        }

        // check again, don't trust the form id
        if (!await _access.CanManageAsync(User, id))
        {
            return Forbid();
        }

        var existing = await _context.Municipalities.FindAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        // lower case so "surrey" and "Surrey" count as the same name
        var name = municipality.Name.Trim().ToLower();
        if (await _context.Municipalities.AnyAsync(m => m.Name.ToLower() == name && m.MunicipalityId != id))
        {
            ModelState.AddModelError("Name", "A municipality with this name already exists.");
        }

        // server check for the logo
        var logoError = _images.CheckImage(municipality.LogoFile);
        if (logoError != null)
        {
            ModelState.AddModelError("LogoFile", logoError);
        }

        if (ModelState.IsValid)
        {
            // copy form values so AdminRoleName and LogoFileName aren't wiped
            existing.Name = municipality.Name;
            existing.Province = municipality.Province;
            existing.ElectionDate = municipality.ElectionDate;
            existing.Description = municipality.Description;

            // new logo picked, save it
            var oldLogo = existing.LogoFileName;
            if (municipality.LogoFile != null)
            {
                existing.LogoFileName = await _images.SaveAsync(municipality.LogoFile, "logos");
            }

            await _context.SaveChangesAsync();

            // delete the old logo only after the save worked
            if (municipality.LogoFile != null)
            {
                _images.Delete(oldLogo);
            }

            TempData["Message"] = $"{existing.Name} was updated.";
            return RedirectToAction(nameof(Index));
        }

        // keep showing the current logo
        municipality.LogoFileName = existing.LogoFileName;
        return View(municipality);
    }

    // GET: Admin/Municipalities/Delete/5
    // super admin only
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> Delete(int? id)
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

    // POST: Admin/Municipalities/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var municipality = await _context.Municipalities
            .Include(m => m.Positions)
                .ThenInclude(p => p.Candidates)
            .FirstOrDefaultAsync(m => m.MunicipalityId == id);

        if (municipality != null)
        {
            _context.Municipalities.Remove(municipality);
            await _context.SaveChangesAsync();

            // positions and candidates got deleted too, remove their photos and the logo
            foreach (var candidate in municipality.Positions.SelectMany(p => p.Candidates))
            {
                _images.Delete(candidate.PhotoFileName);
            }
            _images.Delete(municipality.LogoFileName);

            // delete its admin group
            var role = await _roleManager.FindByNameAsync(municipality.AdminRoleName);
            if (role != null)
            {
                await _roleManager.DeleteAsync(role);
            }

            TempData["Message"] = $"{municipality.Name} was deleted.";
        }

        return RedirectToAction(nameof(Index));
    }
}
