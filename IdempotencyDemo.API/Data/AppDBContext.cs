using Microsoft.EntityFrameworkCore;
using IdempotencyDemo.API.Data.Models;

namespace IdempotencyDemo.API.Data
{
	public class AppDBContext : DbContext
	{
		public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

		public DbSet<User> Users { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<User>(builder =>
			{
				builder.HasKey(u => u.Id);

				builder.Property(e => e.CreatedAt)
					.HasDefaultValueSql("GETDATE()");
			});
		}
	}
}
