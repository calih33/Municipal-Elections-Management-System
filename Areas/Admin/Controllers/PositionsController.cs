// This file contains the admin PositionsController class for the Municipal Elections Management System.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Municipal_Elections_Management_System.Data;
using Municipal_Elections_Management_System.Models;
using Municipal_Elections_Management_System.Services;

namespace Municipal_Elections_Management_System.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class PositionsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IMunicipalityAccessService _access;
    private readonly ImageStorageService _images;

    public PositionsController(ApplicationDbContext context, IMunicipalityAccessService access, ImageStorageService images)
    {
        _context = context;
        _access = access;
        _images = images;
    }

    // GET: Admin/Positions
    public async Task<IActionResult> Index()
    {
        // only this admin's municipalities
        var ids = await _access.GetManageableMunicipalityIdsAsync(User);

        var positions = await _context.Positions
            .Include(p => p.Municipality)
            .Include(p => p.Candidates)
            .Where(p => ids.Contains(p.MunicipalityId!.Value))
            .OrderBy(p => p.Municipality!.Name)
            .ThenBy(p => p.Name)
            .ToListAsync();

        return View(positions);
    }

    // GET: Admin/Positions/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var position = await _context.Positions
            .Include(p => p.Municipality)
            .Include(p => p.Candidates)
            .FirstOrDefaultAsync(p => p.PositionId == id);

        if (position == null)
        {
            return NotFound();
        }

        if (!await _access.CanManageAsync(User, position.MunicipalityId!.Value))
        {
            return Forbid();
        }

        return View(position);
    }

    // GET: Admin/Positions/Create
    public async Task<IActionResult> Create()
    {
        await LoadMunicipalityDropdownAsync(null);
        return View(new Position());
    }

    // POST: Admin/Positions/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,SeatsAvailable,MunicipalityId")] Position position)
    {
        if (ModelState.IsValid)
        {
            // check the admin can manage this municipality
            if (!await _access.CanManageAsync(User, position.MunicipalityId!.Value))
            {
                return Forbid();
            }

            if (await IsDuplicateAsync(position))
            {
                ModelState.AddModelError("Name", "This municipality already has a position with this name.");
            }
            else
            {
                _context.Add(position);
                await _context.SaveChangesAsync();

                TempData["Message"] = $"{position.Name} was created.";
                return RedirectToAction(nameof(Index));
            }
        }

        await LoadMunicipalityDropdownAsync(position.MunicipalityId);
        return View(position);
    }

    // GET: Admin/Positions/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var position = await _context.Positions.FindAsync(id);
        if (position == null)
        {
            return NotFound();
        }

        if (!await _access.CanManageAsync(User, position.MunicipalityId!.Value))
        {
            return Forbid();
        }

        await LoadMunicipalityDropdownAsync(position.MunicipalityId);
        return View(position);
    }

    // POST: Admin/Positions/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("PositionId,Name,SeatsAvailable,MunicipalityId")] Position position)
    {
        if (id != position.PositionId)
        {
            return NotFound();
        }

        var existing = await _context.Positions.FindAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        // check current municipality
        if (!await _access.CanManageAsync(User, existing.MunicipalityId!.Value))
        {
            return Forbid();
        }

        if (ModelState.IsValid)
        {
            // and the one picked on the form
            if (!await _access.CanManageAsync(User, position.MunicipalityId!.Value))
            {
                return Forbid();
            }

            if (await IsDuplicateAsync(position))
            {
                ModelState.AddModelError("Name", "This municipality already has a position with this name.");
            }
            else
            {
                existing.Name = position.Name;
                existing.SeatsAvailable = position.SeatsAvailable;
                existing.MunicipalityId = position.MunicipalityId;
                await _context.SaveChangesAsync();

                TempData["Message"] = $"{existing.Name} was updated.";
                return RedirectToAction(nameof(Index));
            }
        }

        await LoadMunicipalityDropdownAsync(position.MunicipalityId);
        return View(position);
    }

    // GET: Admin/Positions/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var position = await _context.Positions
            .Include(p => p.Municipality)
            .Include(p => p.Candidates)
            .FirstOrDefaultAsync(p => p.PositionId == id);

        if (position == null)
        {
            return NotFound();
        }

        if (!await _access.CanManageAsync(User, position.MunicipalityId!.Value))
        {
            return Forbid();
        }

        return View(position);
    }

    // POST: Admin/Positions/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var position = await _context.Positions
            .Include(p => p.Candidates)
            .FirstOrDefaultAsync(p => p.PositionId == id);

        if (position != null)
        {
            if (!await _access.CanManageAsync(User, position.MunicipalityId!.Value))
            {
                return Forbid();
            }

            // candidates get deleted too, remove their photos
            foreach (var candidate in position.Candidates)
            {
                _images.Delete(candidate.PhotoFileName);
            }

            _context.Positions.Remove(position);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"{position.Name} was deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    // municipality dropdown (this admin's only)
    private async Task LoadMunicipalityDropdownAsync(int? selectedId)
    {
        var ids = await _access.GetManageableMunicipalityIdsAsync(User);

        ViewData["MunicipalityId"] = new SelectList(
            await _context.Municipalities
                .Where(m => ids.Contains(m.MunicipalityId))
                .OrderBy(m => m.Name)
                .ToListAsync(),
            "MunicipalityId", "Name", selectedId);
    }

    // no two positions with the same name
    private async Task<bool> IsDuplicateAsync(Position position)
    {
        return await _context.Positions.AnyAsync(p =>
            p.MunicipalityId == position.MunicipalityId
            && p.Name == position.Name
            && p.PositionId != position.PositionId);
    }
}
