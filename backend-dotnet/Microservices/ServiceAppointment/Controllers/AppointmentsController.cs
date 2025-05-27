using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ServiceAppointment.Interfaces;
using ServiceAppointment.Models;
using ServiceAppointment.Models.Requests;
using System.Security.Claims;

namespace ServiceAppointment.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _service;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(IAppointmentService service, ILogger<AppointmentsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // יצירת תור חדש
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AppointmentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid appointment data.");

        // נרמול לזמן ברמת הדקה (שנייה = 0)
        var normalizedTime = new DateTime(
            request.ScheduledTime.Year,
            request.ScheduledTime.Month,
            request.ScheduledTime.Day,
            request.ScheduledTime.Hour,
            request.ScheduledTime.Minute,
            0
        );

        // בדיקה אם כבר קיים תור לאותה דקה באותו סניף
        bool isTaken = await _service.IsSlotTakenAsync(request.BranchId, normalizedTime);
        if (isTaken)
            return Conflict("An appointment already exists at this minute for the selected branch.");

        // שליפת מזהה המשתמש מתוך ה־JWT
        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier); // "sub" גם עובד לפעמים
        if (string.IsNullOrEmpty(customerId))
            return Unauthorized("User identity is missing.");

        // יצירת אובייקט חדש
        var appointment = new Appointment
        {
            CustomerId = customerId,
            BranchId = request.BranchId,
            ScheduledTime = normalizedTime,
            Status = request.Status ?? "Scheduled"
        };

        var created = await _service.CreateAsync(appointment);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // נתיב להצגת כל התורים של המשתמש המחובר
    [HttpGet("my-appointments")]
    public async Task<ActionResult<IEnumerable<Appointment>>> GetMyAppointments()
    {
        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(customerId))
            return Unauthorized("User identity is missing.");

        var appointments = await _service.GetByCustomerIdAsync(customerId);
        return Ok(appointments);
    }

    // נתיב להצגת תור לפי מזהה
    [HttpGet("{id}")]
    public async Task<ActionResult<Appointment>> GetById(int id)
    {
        var appt = await _service.GetByIdAsync(id);
        if (appt == null)
            return NotFound();

        return Ok(appt);
    }

    // נתיב להצגת תורים לפי סניף
    [HttpGet("by-branch/{branchId}")]
    public async Task<ActionResult<IEnumerable<Appointment>>> GetByBranch(string branchId)
    {
        if (string.IsNullOrEmpty(branchId))
            return BadRequest("Branch ID is required.");

        var appointments = await _service.GetByBranchIdAsync(branchId);
        return Ok(appointments);
    }

    // נתיב להחזרת כל התורים
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Appointment>>> GetAllAppointments()
    {
        var appointments = await _service.GetAllAsync();
        return Ok(appointments);
    }

    // נתיב למחיקת תור לפי מזהה
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAppointment(int id)
    {
        var result = await _service.CancelAsync(id);
        if (!result)
        {
            return NotFound("Appointment not found.");
        }
        return NoContent(); // מחזיר תשובה ריקה אחרי המחיקה
    }
}
