using JustTip.Core.DTOs;
using JustTip.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace JustTip.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TipsController(ITipCalculationService tipCalculationService) : ControllerBase
{
    [HttpGet("daily")]
    public async Task<ActionResult<IReadOnlyList<DailyTipInputDto>>> GetDailyTips([FromQuery] DateOnly weekStart)
    {
        var tips = await tipCalculationService.GetDailyTipsAsync(weekStart);
        return Ok(tips);
    }

    [HttpPost("daily")]
    public async Task<ActionResult> SaveDailyTips([FromBody] IEnumerable<DailyTipInputDto> tips)
    {
        await tipCalculationService.SaveDailyTipsAsync(tips);
        return Ok();
    }

    [HttpGet("payout")]
    public async Task<ActionResult<WeeklyPayoutDto>> GetWeeklyPayout([FromQuery] DateOnly weekStart)
    {
        var payout = await tipCalculationService.CalculateWeeklyPayoutAsync(weekStart);
        return Ok(payout);
    }
}
