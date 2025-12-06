using JustTip.Core.Entities;
using JustTip.Core.Interfaces;
using JustTip.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JustTip.Infrastructure.Repositories;

public class EmployeeRepository(AppDbContext context) : IEmployeeRepository
{
    public async Task<Employee?> GetByIdAsync(int id)
    {
        return await context.Employees.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        return await context.Employees.OrderBy(e => e.Name).ToListAsync();
    }
}
