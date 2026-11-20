
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hive.Domain.Entities;

public class FileVersion
{
    [Key]
    public Guid FileVersionId { get; set; }

    [Required]
    public Guid FileId { get; set; }

    [ForeignKey(nameof(FileId))]
    public File File { get; set; } = null!;

    [Required]
    public int VersionNumber { get; set; }

    [Required]
    [MaxLength(500)]
    public string StorageKey { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    [Required]
    [MaxLength(150)]
    public string ContentType { get; set; } = string.Empty;

    [Required]
    public Guid UploadedByMemberId { get; set; }

    [ForeignKey(nameof(UploadedByMemberId))]
    public WorkspaceMember UploadedByMember { get; set; } = null!;

    [MaxLength(500)]
    public string? ChangeNote { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
