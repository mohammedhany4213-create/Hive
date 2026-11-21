
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hive.Domain.Entities;

public class Collection
{
    [Key]
    public Guid CollectionId { get; set; }

    [Required]
    public Guid WorkspaceId { get; set; }

    [ForeignKey(nameof(WorkspaceId))]
    public Workspace Workspace { get; set; } = null!;

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public Guid CreatedByMemberId { get; set; }

    [ForeignKey(nameof(CreatedByMemberId))]
    public WorkspaceMember CreatedByMember { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
