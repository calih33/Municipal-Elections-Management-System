// This file contains the MunicipalitiesApiController class for the Municipal Elections Management System.
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Municipal_Elections_Management_System.Data;
using Municipal_Elections_Management_System.Models.Api;

namespace Municipal_Elections_Management_System.Controllers.Api;

// public, no login needed
[ApiController]
[Route("api/municipalities")]
public class MunicipalitiesApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MunicipalitiesApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/municipalities
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MunicipalityDto>>> GetMunicipalities()
    {
        var municipalities = await _context.Municipalities
            .Include(m => m.Positions)
            .OrderBy(m => m.Name)
            .ToListAsync();

        return municipalities.Select(m => MunicipalityDto.From(m, Request)).ToList();
    }

    // GET: api/municipalities/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MunicipalityDto>> GetMunicipality(int id)
    {
        var municipality = await _context.Municipalities
            .Include(m => m.Positions)
            .FirstOrDefaultAsync(m => m.MunicipalityId == id);

        if (municipality == null)
        {
            return Problem(title: "Municipality not found.", statusCode: StatusCodes.Status404NotFound);
        }

        return MunicipalityDto.From(municipality, Request);
    }
}
