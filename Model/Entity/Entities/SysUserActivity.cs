namespace Entity.Entities;

public class SysUserActivity : BaseEntity
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public Guid? ActivityId { get; set; }

    public bool? C { get; set; }

    public bool? R { get; set; }

    public bool? U { get; set; }

    public bool? D { get; set; }

    public string? Description { get; set; }

    public string? Note { get; set; }
}