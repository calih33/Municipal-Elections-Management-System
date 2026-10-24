// This file contains the PositionsApiController class for the Municipal Elections Management System.
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Municipal_Elections_Management_System.Data;
using Municipal_Elections_Management_System.Models.Api;

namespace Municipal_Elections_Management_System.Controllers.Api;

// public, no login needed
[ApiController]
[Route("api/positions")]
public class PositionsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PositionsApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/positions?municipalityId=1
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PositionDto>>> GetPositions(int? municipalityId)
    {
        var positions = _context.Positions.Include(p => p.Municipality).AsQueryable();

        if (municipalityId != null)
        {
            positions = positions.Where(p => p.MunicipalityId == municipalityId);
        }

        var list = await positions
            .OrderBy(p => p.Municipality!.Name)
            .ThenBy(p => p.Name)
            .ToListAsync();

        return list.Select(p => PositionDto.From(p, p.Municipality!)).ToList();
    }

    // GET: api/positions/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PositionDto>> GetPosition(int id)
    {
        var position = await _context.Positions
            .Include(p => p.Municipality)
            .FirstOrDefaultAsync(p => p.PositionId == id);

        if (position == null)
        {
            return Problem(title: "Position not found.", statusCode: StatusCodes.Status404NotFound);
        }

        return PositionDto.From(position, position.Municipality!);
    }
}
