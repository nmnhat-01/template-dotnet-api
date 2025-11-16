namespace Entity.Entities;

public class SysMediaCollection : BaseEntity
{
    public Guid Id { get; set; }
    public CollectionEnum CollectionType { get; set; }
    public bool Active { get; set; } = true;
    public virtual ICollection<SysMedia> Medias { get; set; } = new List<SysMedia>();
}

public enum CollectionEnum
{
    ImageAvatar = 1
}