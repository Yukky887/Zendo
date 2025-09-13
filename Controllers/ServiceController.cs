using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YukkyServiceWeb.Data;
using YukkyServiceWeb.Models;

namespace YukkyServiceWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ServiceController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/service
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Service>>> GetServices()
    {
        return await _context.Services.ToListAsync();
    }

    // GET: api/service/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Service>> GetService(int id)
    {
        var service = await _context.Services.FindAsync(id);

        if (service == null)
        {
            return NotFound();
        }

        return service;
    }

    // POST: api/service
    [HttpPost]
    public async Task<ActionResult<Service>> PostService(Service service)
    {
        _context.Services.Add(service);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetService", new { id = service.Id }, service);
    }

    // PUT: api/service/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutService(int id, Service service)
    {
        if (id != service.Id)
        {
            return BadRequest();
        }

        _context.Entry(service).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ServiceExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/service/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteService(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null)
        {
            return NotFound();
        }

        _context.Services.Remove(service);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ServiceExists(int id)
    {
        return _context.Services.Any(e => e.Id == id);
    }
}