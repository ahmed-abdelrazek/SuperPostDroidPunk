using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplicationPostTest.Models;

namespace WebApplicationPostTest.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Todo> TodoList { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Todo>()
                .HasOne(p => p.User)
                .WithMany(b => b.TodoList)
                .HasForeignKey(p => p.UserId);

            base.OnModelCreating(builder);
        }
    }
}
