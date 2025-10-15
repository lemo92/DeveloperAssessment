using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Domain.Entities;
using TaskTracker.Infrastructure;

namespace TaskTracker.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _db;

        public TasksController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] TaskTracker.Domain.Entities.TaskStatus? status, [FromQuery] DateTime? dueBefore, [FromQuery] Guid? assignee)
        {
            var query = _db.Tasks.AsQueryable();
            if (status.HasValue) query = query.Where(t => t.Status == status);
            if (dueBefore.HasValue) query = query.Where(t => t.DueDate <= dueBefore);
            if (assignee.HasValue) query = query.Where(t => t.AssignedUserId == assignee);
            var items = await query.Include(t => t.AssignedUser).ToListAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _db.Tasks.Include(t => t.AssignedUser).FirstOrDefaultAsync(t => t.Id == id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TaskItem request)
        {
            request.Id = Guid.NewGuid();
            request.CreatedDate = DateTime.UtcNow;
            _db.Tasks.Add(request);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = request.Id }, request);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TaskItem update)
        {
            var existing = await _db.Tasks.FindAsync(id);
            if (existing == null) return NotFound();
            existing.Title = update.Title;
            existing.Description = update.Description;
            existing.Status = update.Status;
            existing.DueDate = update.DueDate;
            existing.AssignedUserId = update.AssignedUserId;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _db.Tasks.FindAsync(id);
            if (existing == null) return NotFound();
            _db.Tasks.Remove(existing);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}


