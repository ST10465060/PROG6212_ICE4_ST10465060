using Microsoft.EntityFrameworkCore;

namespace ClaimDataManager;

// The EF Core session with the SQLite database.
public class ClaimContext : DbContext
{
    // The table that EF Core maps the Claim class to.
    public DbSet<Claim> Claims => Set<Claim>();

    // The database file sits next to the executable so the project
    // still runs after the ZIP is extracted, no path changes needed.
    public static readonly string DbPath =
        Path.Combine(AppContext.BaseDirectory, "claims.db");

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TotalAmount is calculated in C#, so tell EF Core not to
        // create a column for it.
        modelBuilder.Entity<Claim>().Ignore(c => c.TotalAmount);
    }
}
