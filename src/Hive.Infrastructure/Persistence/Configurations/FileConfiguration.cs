using Hive.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HiveFile = Hive.Domain.Entities.File;

namespace Hive.Infrastructure.Persistence.Configurations;

public sealed class FileConfiguration : IEntityTypeConfiguration<HiveFile>
{
    public void Configure(EntityTypeBuilder<HiveFile> builder)
    {
        builder.ToTable("Files");

        builder.HasKey(x => x.FileId);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.OriginalFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Extension)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.StorageKey)
            .HasMaxLength(500);

        builder.Property(x => x.SizeBytes)
            .IsRequired();

        builder.Property(x => x.CurrentVersionNumber)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(x => x.Collection)
            .WithMany()
            .HasForeignKey(x => x.CollectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UploadedByMember)
            .WithMany()
            .HasForeignKey(x => x.UploadedByMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CollectionId);
        builder.HasIndex(x => x.UploadedByMemberId);
    }
}
