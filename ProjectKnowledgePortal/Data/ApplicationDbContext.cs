using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectKnowledgePortal.Models.Domain;

namespace ProjectKnowledgePortal.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Video> Videos => Set<Video>();
    public DbSet<Script> Scripts => Set<Script>();
    public DbSet<ProjectImage> Images => Set<ProjectImage>();
    public DbSet<Link> Links => Set<Link>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasIndex(x => x.Name).IsUnique();
        });

        builder.Entity<Project>(entity =>
        {
            entity.ToTable("Projects");
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.Name);

            entity.HasOne(x => x.Category)
                .WithMany(x => x.Projects)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Document>(entity =>
        {
            entity.ToTable("Documents");
            entity.HasIndex(x => x.Name);
            entity.HasIndex(x => x.UploadedAtUtc);

            entity.HasOne(x => x.Project)
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Category)
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Video>(entity =>
        {
            entity.ToTable("Videos");
            entity.HasIndex(x => x.Name);
            entity.HasIndex(x => x.UploadedAtUtc);

            entity.HasOne(x => x.Project)
                .WithMany(x => x.Videos)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Category)
                .WithMany(x => x.Videos)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Script>(entity =>
        {
            entity.ToTable("Scripts");
            entity.HasIndex(x => x.Name);
            entity.HasIndex(x => x.UploadedAtUtc);

            entity.HasOne(x => x.Project)
                .WithMany(x => x.Scripts)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Category)
                .WithMany(x => x.Scripts)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<ProjectImage>(entity =>
        {
            entity.ToTable("Images");
            entity.HasIndex(x => x.Name);
            entity.HasIndex(x => x.UploadedAtUtc);

            entity.HasOne(x => x.Project)
                .WithMany(x => x.Images)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Category)
                .WithMany(x => x.Images)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Link>(entity =>
        {
            entity.ToTable("Links");
            entity.HasIndex(x => x.Title);
            entity.HasIndex(x => x.CreatedAtUtc);

            entity.HasOne(x => x.Project)
                .WithMany(x => x.Links)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Category)
                .WithMany(x => x.Links)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<ActivityLog>(entity =>
        {
            entity.ToTable("ActivityLogs");
            entity.HasIndex(x => x.ActivityType);
            entity.HasIndex(x => x.CreatedAtUtc);

            entity.HasOne(x => x.Project)
                .WithMany(x => x.ActivityLogs)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
