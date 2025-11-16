using Entity.Entities;

namespace DomainService.Interfaces.TemplateAPI;

public interface IUserService
{
    List<SysUser> GetList();
    SysUser GetById(Guid id);
}