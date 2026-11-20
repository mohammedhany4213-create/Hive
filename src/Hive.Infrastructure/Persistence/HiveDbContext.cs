using Hive.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using HiveFile = Hive.Domain.Entities.File;

namespace Hive.Infrastructure.Persistence;

public sealed class HiveDbContext : DbContext
{
    public HiveDbContext(DbContextOptions<HiveDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<WorkspaceMember> WorkspaceMembers => Set<WorkspaceMember>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<HiveFile> Files => Set<HiveFile>();
    public DbSet<FileVersion> FileVersions => Set<FileVersion>();
    public DbSet<Permission> Permissions => Set<Permission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HiveDbContext).Assembly);
    }
}
