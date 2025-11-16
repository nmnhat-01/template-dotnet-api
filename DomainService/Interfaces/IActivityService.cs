using Model.RequestModel;
using Model.ResponseModel;

namespace DomainService.Interfaces;

public interface IActivityService
{
    Task<List<ActivityResponse>> GetListActivity(string keyword, int pageIndex, int pageSize);
    Task<ActivityResponse> GetActivityById(Guid activityId);
    Task<Guid> AddActivity(Guid userId, ActivityRequest model);
    Task<bool> UpdateActivity(Guid userId, UpdateActivityRequest model);
    Task<bool> DeleteActivity(Guid userId, Guid activityId);
}