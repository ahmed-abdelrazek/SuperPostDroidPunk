using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace WebApplicationPostTest.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Todo> TodoList { get; set; }
    }
}
