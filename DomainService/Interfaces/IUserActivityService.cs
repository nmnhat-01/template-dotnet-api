using Model.RequestModel;
using Model.ResponseModel;

namespace DomainService.Interfaces;

public interface IUserActivityService
{
    Task<List<UserActivityResponse>> GetListActivityOfUser(Guid userId);
    Task<bool> UpdateActivityOfUser(Guid userId, UpdateUserActivityRequest model);
}