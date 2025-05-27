using Microsoft.AspNetCore.Mvc;
using BranchManagement.API.Models;
using BranchManagement.API.Services;
using BranchManagement.API.Interfaces;
using Microsoft.Extensions.Logging;
using BranchManagement.API.Exceptions;  // הוספת ה־using עבור חריגות מותאמות
using Microsoft.AspNetCore.Authorization;
namespace BranchManagement.API.Controllers;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BranchesController : ControllerBase
{
    private readonly IBranchService _service;
    private readonly ILogger<BranchesController> _logger;

    public BranchesController(IBranchService service, ILogger<BranchesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<Branch>>> Get()
    {
        // במקרה של שגיאה, נזרוק חריגה וה־Middleware יטפל בה
        var branches = await _service.GetAllAsync();
        if (branches == null)
        {
            throw new NotFoundException("No branches found.");
        }

        return Ok(branches);
    }

    [HttpPost]
    public async Task<ActionResult<Branch>> Create(Branch branch)
    {
        // נזרוק חריגה במקרה של שגיאה
        if (branch == null || string.IsNullOrEmpty(branch.Name))
        {
            throw new ArgumentException("Invalid branch data provided.");
        }

        var created = await _service.CreateAsync(branch);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }
}
