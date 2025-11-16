namespace Entity.Entities;

public class Device : BaseEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UDID { get; set; } = null!;
    public string? OSVersion { get; set; }
    public string? OSName { get; set; }
    public string? DeviceType { get; set; }
    public string? DeviceName { get; set;}
    public string? DeviceDescription { get; set; }
    public bool IsActive { get; set; } = false;

    public string RefreshToken { get; set; } = null!;
    public DateTime RfTokenCreateTime { get; set; }
    public DateTime RfTokenExpiryTime { get; set; }
    public DateTime? RfTokenRevokedTime { get; set; }
    public string RfTokenCreatedByIp { get; set; } = null!;
    public string? RfTokenRevokedByIp { get; set; } = null!;

    public virtual SysUser User { get; set; } = default!;
}