
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hive.Domain.Entities;

public class File
{
    [Key]
    public Guid FileId { get; set; }

    [Required]
    public Guid CollectionId { get; set; }

    [ForeignKey(nameof(CollectionId))]
    public Collection Collection { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string OriginalFileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Extension { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    [MaxLength(500)]
    public string? StorageKey { get; set; }

    public int CurrentVersionNumber { get; set; } = 1;

    [Required]
    public Guid UploadedByMemberId { get; set; }

    [ForeignKey(nameof(UploadedByMemberId))]
    public WorkspaceMember UploadedByMember { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
