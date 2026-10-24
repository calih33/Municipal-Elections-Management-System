using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Municipal_Elections_Management_System.Models;

namespace Municipal_Elections_Management_System.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Municipality> Municipalities => Set<Municipality>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<SavedCandidate> SavedCandidates => Set<SavedCandidate>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Municipality>(entity =>
        {
            entity.HasIndex(m => m.Name).IsUnique();
            entity.HasIndex(m => m.AdminRoleName).IsUnique();
        });

        builder.Entity<Position>(entity =>
        {
            entity.HasOne(p => p.Municipality)
                .WithMany(m => m.Positions)
                .HasForeignKey(p => p.MunicipalityId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // A municipality can't have two positions with the same name.
            entity.HasIndex(p => new { p.MunicipalityId, p.Name }).IsUnique();
        });

        builder.Entity<Candidate>(entity =>
        {
            entity.HasOne(c => c.Position)
                .WithMany(p => p.Candidates)
                .HasForeignKey(c => c.PositionId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(c => c.Name);
        });

        builder.Entity<SavedCandidate>(entity =>
        {
            entity.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.Candidate)
                .WithMany()
                .HasForeignKey(s => s.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);

            // A user can save each candidate only once.
            entity.HasIndex(s => new { s.UserId, s.CandidateId }).IsUnique();
        });
    }
}
