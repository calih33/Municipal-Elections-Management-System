// This file contains the admin CandidatesController class for the Municipal Elections Management System.
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
public class CandidatesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IMunicipalityAccessService _access;
    private readonly ImageStorageService _images;

    public CandidatesController(ApplicationDbContext context, IMunicipalityAccessService access, ImageStorageService images)
    {
        _context = context;
        _access = access;
        _images = images;
    }

    // GET: Admin/Candidates
    public async Task<IActionResult> Index()
    {
        // only this admin's municipalities
        var ids = await _access.GetManageableMunicipalityIdsAsync(User);

        var candidates = await _context.Candidates
            .Include(c => c.Position)
                .ThenInclude(p => p!.Municipality)
            .Where(c => ids.Contains(c.Position!.MunicipalityId!.Value))
            .OrderBy(c => c.Position!.Municipality!.Name)
            .ThenBy(c => c.Position!.Name)
            .ThenBy(c => c.Name)
            .ToListAsync();

        return View(candidates);
    }

    // GET: Admin/Candidates/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var candidate = await FindCandidateAsync(id.Value);
        if (candidate == null)
        {
            return NotFound();
        }

        if (!await _access.CanManageAsync(User, candidate.Position!.MunicipalityId!.Value))
        {
            return Forbid();
        }

        return View(candidate);
    }

    // GET: Admin/Candidates/Create
    public async Task<IActionResult> Create()
    {
        await LoadPositionDropdownAsync(null);
        return View(new Candidate());
    }

    // POST: Admin/Candidates/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,PositionId,Profile,Website,Email,PhotoFile")] Candidate candidate)
    {
        // server check for the photo
        var photoError = _images.CheckImage(candidate.PhotoFile);
        if (photoError != null)
        {
            ModelState.AddModelError("PhotoFile", photoError);
        }

        if (ModelState.IsValid)
        {
            // check the admin can manage this position
            if (!await CanManagePositionAsync(candidate.PositionId!.Value))
            {
                return Forbid();
            }

            candidate.DateCreated = DateTime.UtcNow;

            if (candidate.PhotoFile != null)
            {
                candidate.PhotoFileName = await _images.SaveAsync(candidate.PhotoFile, "candidates");
            }

            _context.Add(candidate);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"{candidate.Name} was added.";
            return RedirectToAction(nameof(Index));
        }

        await LoadPositionDropdownAsync(candidate.PositionId);
        return View(candidate);
    }

    // GET: Admin/Candidates/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var candidate = await FindCandidateAsync(id.Value);
        if (candidate == null)
        {
            return NotFound();
        }

        if (!await _access.CanManageAsync(User, candidate.Position!.MunicipalityId!.Value))
        {
            return Forbid();
        }

        await LoadPositionDropdownAsync(candidate.PositionId);
        return View(candidate);
    }

    // POST: Admin/Candidates/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("CandidateId,Name,PositionId,Profile,Website,Email,PhotoFile")] Candidate candidate)
    {
        if (id != candidate.CandidateId)
        {
            return NotFound();
        }

        var existing = await FindCandidateAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        // check current municipality
        if (!await _access.CanManageAsync(User, existing.Position!.MunicipalityId!.Value))
        {
            return Forbid();
        }

        // server check for the photo
        var photoError = _images.CheckImage(candidate.PhotoFile);
        if (photoError != null)
        {
            ModelState.AddModelError("PhotoFile", photoError);
        }

        if (ModelState.IsValid)
        {
            // and the position picked on the form
            if (!await CanManagePositionAsync(candidate.PositionId!.Value))
            {
                return Forbid();
            }

            // copy form values so DateCreated and PhotoFileName aren't wiped
            existing.Name = candidate.Name;
            existing.PositionId = candidate.PositionId;
            existing.Profile = candidate.Profile;
            existing.Website = candidate.Website;
            existing.Email = candidate.Email;

            // replace the old photo
            if (candidate.PhotoFile != null)
            {
                _images.Delete(existing.PhotoFileName);
                existing.PhotoFileName = await _images.SaveAsync(candidate.PhotoFile, "candidates");
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = $"{existing.Name} was updated.";
            return RedirectToAction(nameof(Index));
        }

        // keep showing the current photo
        candidate.PhotoFileName = existing.PhotoFileName;
        await LoadPositionDropdownAsync(candidate.PositionId);
        return View(candidate);
    }

    // GET: Admin/Candidates/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var candidate = await FindCandidateAsync(id.Value);
        if (candidate == null)
        {
            return NotFound();
        }

        if (!await _access.CanManageAsync(User, candidate.Position!.MunicipalityId!.Value))
        {
            return Forbid();
        }

        return View(candidate);
    }

    // POST: Admin/Candidates/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var candidate = await FindCandidateAsync(id);

        if (candidate != null)
        {
            if (!await _access.CanManageAsync(User, candidate.Position!.MunicipalityId!.Value))
            {
                return Forbid();
            }

            _images.Delete(candidate.PhotoFileName);

            _context.Candidates.Remove(candidate);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"{candidate.Name} was deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    // candidate with position and municipality
    private async Task<Candidate?> FindCandidateAsync(int id)
    {
        return await _context.Candidates
            .Include(c => c.Position)
                .ThenInclude(p => p!.Municipality)
            .FirstOrDefaultAsync(c => c.CandidateId == id);
    }

    // can this admin manage the position?
    private async Task<bool> CanManagePositionAsync(int positionId)
    {
        var position = await _context.Positions.FindAsync(positionId);
        return position != null && await _access.CanManageAsync(User, position.MunicipalityId!.Value);
    }

    // position dropdown, e.g. "Pitt Meadows - Mayor"
    private async Task LoadPositionDropdownAsync(int? selectedId)
    {
        var ids = await _access.GetManageableMunicipalityIdsAsync(User);

        ViewData["PositionId"] = new SelectList(
            await _context.Positions
                .Where(p => ids.Contains(p.MunicipalityId!.Value))
                .OrderBy(p => p.Municipality!.Name)
                .ThenBy(p => p.Name)
                .Select(p => new { p.PositionId, Label = p.Municipality!.Name + " - " + p.Name })
                .ToListAsync(),
            "PositionId", "Label", selectedId);
    }
}
