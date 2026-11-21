
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hive.Domain.Entities;

public class WorkspaceMember
{
    [Key]
    public Guid MemberId { get; set; }

    [Required]
    public Guid WorkspaceId { get; set; }

    [ForeignKey(nameof(WorkspaceId))]
    public Workspace Workspace { get; set; } = null!;

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public MemberStatus Status { get; set; } = MemberStatus.Active;
}
