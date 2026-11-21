using Hive.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HiveFile = Hive.Domain.Entities.File;

namespace Hive.Infrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions", table =>
        {
            table.HasCheckConstraint(
                "CK_Permissions_ExactlyOneTarget",
                "([CollectionId] IS NOT NULL AND [FileId] IS NULL) OR ([CollectionId] IS NULL AND [FileId] IS NOT NULL)");

            table.HasCheckConstraint(
                "CK_Permissions_TargetMatchesPermissionType",
                "([CollectionId] IS NOT NULL AND [PermissionType] BETWEEN 1 AND 4) OR ([FileId] IS NOT NULL AND [PermissionType] BETWEEN 5 AND 7)");
        });

        builder.HasKey(x => x.PermissionId);

        builder.Property(x => x.PermissionType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.GrantedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(x => x.WorkspaceMember)
            .WithMany()
            .HasForeignKey(x => x.WorkspaceMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.GrantedByMember)
            .WithMany()
            .HasForeignKey(x => x.GrantedByMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Collection)
            .WithMany()
            .HasForeignKey(x => x.CollectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.File)
            .WithMany()
            .HasForeignKey(x => x.FileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.WorkspaceMemberId, x.CollectionId, x.PermissionType })
            .IsUnique()
            .HasFilter("[CollectionId] IS NOT NULL");

        builder.HasIndex(x => new { x.WorkspaceMemberId, x.FileId, x.PermissionType })
            .IsUnique()
            .HasFilter("[FileId] IS NOT NULL");

        builder.HasIndex(x => x.GrantedByMemberId);
    }
}
