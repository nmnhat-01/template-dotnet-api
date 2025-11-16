namespace Entity.Entities;

public class SysMedia : BaseEntity
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = null!;
    public string? ContentType { get; set; }
    public string Path { get; set; } = null!;
    public Guid MediaCollectionId { get; set; }
    public bool IsExternal { get; set; }

    public virtual SysMediaCollection MediaCollection { get; set; }
}