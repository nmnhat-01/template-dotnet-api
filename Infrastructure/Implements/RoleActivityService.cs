using Common.UnitOfWork.UnitOfWorkPattern;
using DomainService.Interfaces;
using Entity.Entities;
using Microsoft.Extensions.Caching.Memory;
using Model.RequestModel;
using Model.ResponseModel;
using System.Data.Entity;

namespace Infrastructure.Implements;

public class RoleActivityService : BaseService, IRoleActivityService
{

    public RoleActivityService(IUnitOfWork unitOfWork, IMemoryCache memoryCache) : base(unitOfWork, memoryCache)
    {
    }

    public async Task<List<RoleActivityResponse>> GetListActivityOfRole(Guid roleId)
    {
        var res = await _unitOfWork.Repository<SysRoleActivity>()
            .Where(s => s.RoleId == roleId)
            .Join(_unitOfWork.Repository<SysActivity>(),
                roleActivity => roleActivity.ActivityId, activity => activity.Id,
                (roleActivity, activity) => new
                {
                    RoleActivity = roleActivity,
                    activity.ActivityName,
                    activity.Code
                })
            .Select(s => new RoleActivityResponse
            {
                Id = s.RoleActivity.Id,
                RoleId = s.RoleActivity.RoleId,
                ActivityId = s.RoleActivity.ActivityId,
                ActivityName = s.ActivityName,
                Code = s.Code,
                C = s.RoleActivity.C,
                R = s.RoleActivity.R,
                U = s.RoleActivity.U,
                D = s.RoleActivity.D,
                Description = s.RoleActivity.Description,
                Note = s.RoleActivity.Note,
                CreatedUser = s.RoleActivity.CreatedUser,
                UpdatedUser = s.RoleActivity.UpdatedUser,
                CreatedName = s.RoleActivity.CreatedName,
                UpdatedName = s.RoleActivity.UpdatedName,
                CreatedDate = s.RoleActivity.CreatedDate,
                UpdatedDate = s.RoleActivity.UpdatedDate
            }).ToListAsync();

        return res;
    }

    public async Task<bool> UpdateActivityOfRole(Guid userId, UpdateRoleActivityRequest model)
    {
        //var caller = await _userService.GetById(userId);
        var listRoleActivity = new List<SysRoleActivity>();
        var created = DateTime.Now;
        foreach (var item in model.RoleActivityRequests)
        {
            var roleActivity = new SysRoleActivity
            {
                RoleId = model.RoleId,
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
            listRoleActivity.Add(roleActivity);
        }

        var data = await _unitOfWork.Repository<SysRoleActivity>().Where(s => s.RoleId == model.RoleId).ToListAsync();
        if (data != null && data.Count > 0) _unitOfWork.Repository<SysRoleActivity>().RemoveRange(data);

        _unitOfWork.Repository<SysRoleActivity>().AddRange(listRoleActivity);
        var res = await _unitOfWork.SaveChangesAsync();
        return res > 0;
    }
}