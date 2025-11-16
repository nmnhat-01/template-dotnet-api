namespace Entity.Entities;

public class SysRole : BaseEntity
{
    public Guid Id { get; set; }

    public string? RoleName { get; set; }

    public string? Description { get; set; }

    public string? Note { get; set; }

    public int? RoleType { get; set; }
}