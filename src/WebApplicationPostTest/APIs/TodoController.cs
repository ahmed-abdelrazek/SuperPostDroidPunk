using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplicationPostTest.Data;
using WebApplicationPostTest.Models;

namespace WebApplicationPostTest.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TodoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Todo
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Todo>>> GetTodoList()
        {
            var t = await _context.TodoList.ToListAsync();
            if (t.Count == 0)
            {
                t.Add(new Todo
                {
                    Name = "No Todo added start now"
                });
            }
            return t;
        }

        // GET: api/Todo/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Todo>> GetTodo(int id)
        {
            var todo = await _context.TodoList.FindAsync(id);

            return todo == null ? NotFound() : (ActionResult<Todo>)todo;
        }

        // PUT: api/Todo/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodo(int id, Todo todo)
        {
            if (id != todo.Id)
            {
                return BadRequest();
            }

            _context.Entry(todo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TodoExists(id))
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

        // POST: api/Todo
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Todo>> PostTodo(input todo)
        {
            var t = new Todo
            {
                Name = todo.Name,
                UserId = todo.UserId
            };
            _context.TodoList.Add(t);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTodo", new { id = t.Id }, t);
        }

        // DELETE: api/Todo/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Todo>> DeleteTodo(int id)
        {
            var todo = await _context.TodoList.FindAsync(id);
            if (todo == null)
            {
                return NotFound();
            }

            _context.TodoList.Remove(todo);
            await _context.SaveChangesAsync();

            return todo;
        }

        private bool TodoExists(int id)
        {
            return _context.TodoList.Any(e => e.Id == id);
        }
    }

    public class input
    {
        public string Name { get; set; }

        public string UserId { get; set; }
    }
}
