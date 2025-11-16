using Model.RequestModel;
using Model.ResponseModel;

namespace DomainService.Interfaces;

public interface IRoleActivityService
{
    Task<List<RoleActivityResponse>> GetListActivityOfRole(Guid roleId);
    Task<bool> UpdateActivityOfRole(Guid userId, UpdateRoleActivityRequest model);
}