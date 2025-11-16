namespace Entity.Entities;

public class SysActivity : BaseEntity
{
    public Guid Id { get; set; }

    public string? Code { get; set; }

    public string? ActivityName { get; set; }

    public string? Description { get; set; }

    public string? ApplicationName { get; set; }

    public string? Note { get; set; }
}