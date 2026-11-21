
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hive.Domain.Entities;

public class Workspace
{
    [Key]
    public Guid WorkspaceId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string JoinPasswordHash { get; set; } = string.Empty;

    [Required]
    public Guid OwnerUserId { get; set; }

    [ForeignKey(nameof(OwnerUserId))]
    public User Owner { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
