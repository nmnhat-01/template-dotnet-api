using System.Data.Entity;
using Common.UnitOfWork.UnitOfWorkPattern;
using DomainService.Interfaces;
using DomainService.Interfaces.TemplateAPI;
using Entity.Entities;
using Microsoft.Extensions.Caching.Memory;
using Model.RequestModel;
using Model.ResponseModel;

namespace Infrastructure.Implements;

public class UserActivityService :BaseService, IUserActivityService
{
    private readonly IUserService _userService;

    public UserActivityService(IUserService userService, IUnitOfWork unitOfWork, IMemoryCache memoryCache) : base(unitOfWork, memoryCache)
    {
        _userService = userService;
    }

    public async Task<List<UserActivityResponse>> GetListActivityOfUser(Guid userId)
    {
        var res = await _unitOfWork.Repository<SysUserActivity>()
            .Where(s => s.UserId == userId)
            .Join(_unitOfWork.Repository<SysActivity>(),
                userActivity => userActivity.ActivityId, activity => activity.Id,
                (userActivity, activity) => new
                {
                    UserActivity = userActivity,
                    activity.ActivityName,
                    activity.Code
                })
            .Select(s => new UserActivityResponse
            {
                Id = s.UserActivity.Id,
                UserId = s.UserActivity.UserId,
                ActivityId = s.UserActivity.ActivityId,
                ActivityName = s.ActivityName,
                Code = s.Code,
                C = s.UserActivity.C,
                R = s.UserActivity.R,
                U = s.UserActivity.U,
                D = s.UserActivity.D,
                Description = s.UserActivity.Description,
                Note = s.UserActivity.Note,
                CreatedUser = s.UserActivity.CreatedUser,
                UpdatedUser = s.UserActivity.UpdatedUser,
                CreatedName = s.UserActivity.CreatedName,
                UpdatedName = s.UserActivity.UpdatedName,
                CreatedDate = s.UserActivity.CreatedDate,
                UpdatedDate = s.UserActivity.UpdatedDate
            }).ToListAsync();

        return res;
    }

    public async Task<bool> UpdateActivityOfUser(Guid userId, UpdateUserActivityRequest model)
    {
        /*var user = await _userService.GetById(model.UserId);
        var caller = await _userService.GetById(userId);*/
        var listUserActivity = new List<SysUserActivity>();
        var created = DateTime.Now;
        foreach (var item in model.UserActivityRequests)
        {
            var userActivity = new SysUserActivity
            {
                UserId = model.UserId,
                ActivityId = item.ActivityId,
                C = item.C,
                R = item.R,
                U = item.U,
                D = item.D,
                Description = item.Description,
                Note = item.Note,
                CreatedDate = created,
                //Creator = caller.UserName,
                CreatedUser = userId
            };
            listUserActivity.Add(userActivity);
        }

        var data = await _unitOfWork.Repository<SysUserActivity>().Where(s => s.UserId == model.UserId).ToListAsync();
        if (data != null && data.Count > 0) _unitOfWork.Repository<SysUserActivity>().RemoveRange(data);

        _unitOfWork.Repository<SysUserActivity>().AddRange(listUserActivity);
        var res = await _unitOfWork.SaveChangesAsync();
        return res > 0;
    }
}