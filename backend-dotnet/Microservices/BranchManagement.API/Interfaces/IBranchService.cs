using BranchManagement.API.Models;

namespace BranchManagement.API.Interfaces;

public interface IBranchService
{
    Task<List<Branch>> GetAllAsync();
    Task<Branch> CreateAsync(Branch branch);
}
