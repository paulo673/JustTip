using JustTip.Core.DTOs;
using JustTip.Core.Exceptions;
using JustTip.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace JustTip.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RosterController(IRosterService rosterService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShiftDto>>> GetWeeklyRoster([FromQuery] DateOnly startDate)
    {
        var shifts = await rosterService.GetWeeklyRosterAsync(startDate);
        return Ok(shifts);
    }

    [HttpGet("shifts/{id}")]
    public async Task<ActionResult<ShiftDto>> GetShiftById(int id)
    {
        var shift = await rosterService.GetShiftByIdAsync(id);

        if (shift is null)
        {
            return NotFound();
        }

        return Ok(shift);
    }

    [HttpPost("shifts")]
    public async Task<ActionResult<ShiftDto>> CreateShift([FromBody] CreateShiftRequest request)
    {
        try
        {
            var shift = await rosterService.CreateShiftAsync(request);
            return CreatedAtAction(nameof(GetWeeklyRoster), new { startDate = shift.Date }, shift);
        }
        catch (ShiftOverlapException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("shifts/{id}")]
    public async Task<ActionResult<ShiftDto>> UpdateShift(int id, [FromBody] UpdateShiftRequest request)
    {
        try
        {
            var shift = await rosterService.UpdateShiftAsync(id, request);
            return Ok(shift);
        }
        catch (ShiftOverlapException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("shifts/{id}")]
    public async Task<ActionResult> DeleteShift(int id)
    {
        try
        {
            await rosterService.DeleteShiftAsync(id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
