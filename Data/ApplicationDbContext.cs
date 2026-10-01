using Microsoft.EntityFrameworkCore;

using Municipal_Elections_Management_System.Models;

namespace Municipal_Elections_Management_System.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Seed();
    }


    public DbSet<Municipality> Municipalities { get; set; }
    public DbSet<Candidate> Candidates { get; set; }
    public DbSet<Position> Positions { get; set; }

}
