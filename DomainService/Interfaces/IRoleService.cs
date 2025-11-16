using Microsoft.WindowsAzure.Storage.File.Protocol;
using Model.RequestModel;
using Model.ResponseModel;

namespace DomainService.Interfaces;

public interface IRoleService
{
    Task<List<RoleResponse>> GetListRole(string keyword, int pageIndex, int pageSize);
    Task<RoleResponse> GetRoleById(Guid roleId);
    Task<Guid> AddRole(Guid userId, RoleRequest model);
    Task<bool> UpdateRole(Guid userId, UpdateRoleRequest model);
    Task<bool> DeleteRole(Guid userId, Guid roleId);
}