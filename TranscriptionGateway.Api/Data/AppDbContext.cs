using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TranscriptionGateway.Api.Models;

namespace TranscriptionGateway.Api.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<TranscriptionJob> TranscriptionJobs => Set<TranscriptionJob>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.EmailConfirmed)
                .HasConversion<short>()
                .HasColumnType("NUMBER(1)");

            entity.Property(e => e.PhoneNumberConfirmed)
                .HasConversion<short>()
                .HasColumnType("NUMBER(1)");

            entity.Property(e => e.TwoFactorEnabled)
                .HasConversion<short>()
                .HasColumnType("NUMBER(1)");

            entity.Property(e => e.LockoutEnabled)
                .HasConversion<short>()
                .HasColumnType("NUMBER(1)");
        });

        builder.Entity<TranscriptionJob>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.JobId)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.OriginalFileName)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.TranscriptUrl)
                .HasMaxLength(1000);

            entity.HasIndex(x => x.JobId).IsUnique();

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}