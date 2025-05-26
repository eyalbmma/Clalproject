using BranchManagement.API.Data;
using BranchManagement.API.Interfaces;
using BranchManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BranchManagement.API.Services;

public class BranchService : IBranchService
{
    private readonly BranchDbContext _context;

    public BranchService(BranchDbContext context)
    {
        _context = context;
    }

    public async Task<List<Branch>> GetAllAsync()
    {
        return await _context.Branches.ToListAsync();
    }

    public async Task<Branch> CreateAsync(Branch branch)
    {
        branch.Id = Guid.NewGuid();
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync();
        return branch;
    }
}
