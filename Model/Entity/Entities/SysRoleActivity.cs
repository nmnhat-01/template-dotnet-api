namespace Entity.Entities;

public class SysRoleActivity : BaseEntity
{
    public Guid Id { get; set; }

    public Guid? RoleId { get; set; }

    public Guid? ActivityId { get; set; }

    public bool? C { get; set; }

    public bool? R { get; set; }

    public bool? U { get; set; }

    public bool? D { get; set; }

    public string? Description { get; set; }

    public string? Note { get; set; }
}