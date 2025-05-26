namespace BranchManagement.API.Models;

public class Branch
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string OpeningHours { get; set; } = string.Empty;
}
