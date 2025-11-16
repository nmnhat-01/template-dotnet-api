namespace Entity;

public class BaseEntity
{
    public DateTime CreatedDate { get; set; }
    public Guid CreatedUser { get; set; }
    public string? CreatedName { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public Guid? UpdatedUser { get; set; }
    public string? UpdatedName { get; set; }
    public bool IsDelete { get; set; } = false;
}