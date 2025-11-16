using Model.RequestModel;
using Model.ResponseModel;

namespace DomainService.Interfaces;

public interface IUserRoleService
{
    Task<List<UserRoleResponse>> GetListUserRole(string keyword, int pageIndex, int pageSize);
    Task<UserRoleResponse> GetUserRoleById(Guid userRoleId);
    Task<Guid> AddUserRole(Guid userId, UserRoleRequest model);
    Task<bool> UpdateUserRole(Guid userId, UpdateUserRoleRequest model);
    Task<bool> DeleteUserRole(Guid userId, Guid userRoleId);
}