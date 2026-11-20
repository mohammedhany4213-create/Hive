
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hive.Domain.Entities;

public class Permission
{
    [Key]
    public Guid PermissionId { get; set; }

    [Required]
    public Guid WorkspaceMemberId { get; set; }

    [ForeignKey(nameof(WorkspaceMemberId))]
    public WorkspaceMember WorkspaceMember { get; set; } = null!;

    public Guid? CollectionId { get; set; }

    [ForeignKey(nameof(CollectionId))]
    public Collection? Collection { get; set; }

    public Guid? FileId { get; set; }

    [ForeignKey(nameof(FileId))]
    public File? File { get; set; }

    [Required]
    public PermissionType PermissionType { get; set; }

    [Required]
    public Guid GrantedByMemberId { get; set; }

    [ForeignKey(nameof(GrantedByMemberId))]
    public WorkspaceMember GrantedByMember { get; set; } = null!;

    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
}
