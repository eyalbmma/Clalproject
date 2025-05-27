using Microsoft.EntityFrameworkCore;
using BranchManagement.API.Models;
using System.Collections.Generic;

namespace BranchManagement.API.Data;

public class BranchDbContext : DbContext
{
    public BranchDbContext(DbContextOptions<BranchDbContext> options) : base(options) { }

    public DbSet<Branch> Branches => Set<Branch>();
}
