using JustTip.Core.DTOs;
using JustTip.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JustTip.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController(IEmployeeRepository employeeRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll()
    {
        var employees = await employeeRepository.GetAllAsync();
        return Ok(employees.Select(e => new EmployeeDto(e.Id, e.Name)));
    }
}
