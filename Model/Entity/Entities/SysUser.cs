namespace Entity.Entities;

public class SysUser : BaseEntity
{
    public Guid UserId { get; set; }

    public string? UserName { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? FullName { get; set; }

    public string? AvatarUrlSmall { get; set; }

    public string? AvatarUrlMedium { get; set; }

    public string? AvatarUrlBig { get; set; }

    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
}
