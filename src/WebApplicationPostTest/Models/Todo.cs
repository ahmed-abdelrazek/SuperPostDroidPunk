using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplicationPostTest.Models
{
    [Table("TodoList")]
    public class Todo
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}
