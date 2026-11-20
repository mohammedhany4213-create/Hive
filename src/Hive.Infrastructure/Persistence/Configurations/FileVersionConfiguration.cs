using Hive.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HiveFile = Hive.Domain.Entities.File;

namespace Hive.Infrastructure.Persistence.Configurations;

public sealed class FileVersionConfiguration : IEntityTypeConfiguration<FileVersion>
{
    public void Configure(EntityTypeBuilder<FileVersion> builder)
    {
        builder.ToTable("FileVersions");

        builder.HasKey(x => x.FileVersionId);

        builder.Property(x => x.VersionNumber)
            .IsRequired();

        builder.Property(x => x.StorageKey)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.ChangeNote)
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(x => x.File)
            .WithMany()
            .HasForeignKey(x => x.FileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.UploadedByMember)
            .WithMany()
            .HasForeignKey(x => x.UploadedByMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.FileId, x.VersionNumber })
            .IsUnique();

        builder.HasIndex(x => x.UploadedByMemberId);
    }
}
