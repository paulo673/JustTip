using JustTip.Core.Entities;

namespace JustTip.Core.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(int id);
}
