using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class EmployeesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EmployeesController(ApplicationDbContext context)
    {
        _context = context;
    }
[Authorize]
[HttpGet("check-auth")]
public IActionResult CheckAuth()
{
    var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
    return Ok(new
    {
        IsAuthenticated = User.Identity.IsAuthenticated,
        Name = User.Identity.Name,
        Claims = claims
    });
}
    // GET: api/employees
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
    {
        return await _context.Employees.ToListAsync();
    }

    // GET: api/employees/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Employee>> GetEmployee(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
        {
            return NotFound();
        }
        return employee;
    }

    // POST: api/employees
    [HttpPost]
    public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
    {
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
    }

    // PUT: api/employees/5
    [HttpPut("{id}")]
    
    public async Task<IActionResult> PutEmployee(int id, Employee employee)
    {
        if (id != employee.Id)
        {
            return BadRequest();
        }

        _context.Entry(employee).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!EmployeeExists(id))
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

    // DELETE: api/employees/5
    [HttpDelete("{id}")]
    
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
        {
            return NotFound();
        }

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private bool EmployeeExists(int id)
    {
        return _context.Employees.Any(e => e.Id == id);
    }
    [HttpGet("search")]
public async Task<ActionResult<IEnumerable<Employee>>> Search(string? name, string? department)
{
    var employees = await _context.Employees
        .Where(e => (string.IsNullOrEmpty(name) || e.Name.Contains(name)) &&
                    (string.IsNullOrEmpty(department) || e.Department.Contains(department)))
        .ToListAsync();

    return Ok(employees);
}

[HttpGet("sort")]
public async Task<ActionResult<IEnumerable<Employee>>> Sort(string orderBy)
{
    var employees = _context.Employees.AsQueryable();

    employees = orderBy.ToLower() switch
    {
        "name" => employees.OrderBy(e => e.Name),
        "position" => employees.OrderBy(e => e.Position),
        "department" => employees.OrderBy(e => e.Department),
        _ => employees.OrderBy(e => e.Id),
       
    };

    return Ok(await employees.ToListAsync());
}
}