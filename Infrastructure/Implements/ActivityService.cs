using Common.Authorization;
using Common.Constant;
using Common.UnitOfWork.UnitOfWorkPattern;
using DomainService.Interfaces;
using DomainService.Interfaces.TemplateAPI;
using Entity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Model.RequestModel;
using Model.ResponseModel;

namespace Infrastructure.Implements;

public class ActivityService : BaseService, IActivityService
{
    private readonly IUserService _userService;
    public ActivityService(IUserService userService, IUnitOfWork unitOfWork, IMemoryCache memoryCache) : base(unitOfWork, memoryCache)
    {
        _userService = userService;
    }

    public async Task<List<ActivityResponse>> GetListActivity(string keyword, int pageIndex, int pageSize)
    {
        List<SysActivity> data;
        if (_memoryCache.TryGetValue(CacheKey.SysActivity, out data))
        {
            return data.Where(s =>
                    (s.ActivityName?.ToLower().Contains(keyword.ToLower()) ?? false) ||
                    (s.Code?.ToLower().Contains(keyword.ToLower()) ?? false))
                .Select(s => new ActivityResponse
                {
                    Id = s.Id,
                    Code = s.Code,
                    ActivityName = s.ActivityName,
                    Description = s.Description,
                    ApplicationName = s.ApplicationName,
                    Note = s.Note,
                    CreatedUser = s.CreatedUser,
                    UpdatedUser = s.UpdatedUser,
                    CreatedName = s.CreatedName,
                    UpdatedName = s.UpdatedName,
                    CreatedDate = s.CreatedDate,
                    UpdatedDate = s.UpdatedDate
                })
                .Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
        }

        data = await _unitOfWork.Repository<SysActivity>().ToListAsync();
        _memoryCache.Set(CacheKey.SysActivity, data, CacheTime.CommmonUncache);
        return data.Where(s =>
                (s.ActivityName?.ToLower().Contains(keyword.ToLower()) ?? false) ||
                (s.Code?.ToLower().Contains(keyword.ToLower()) ?? false))
            .Select(s => new ActivityResponse
            {
                Id = s.Id,
                Code = s.Code,
                ActivityName = s.ActivityName,
                Description = s.Description,
                ApplicationName = s.ApplicationName,
                Note = s.Note,
                CreatedUser = s.CreatedUser,
                UpdatedUser = s.UpdatedUser,
                CreatedName = s.CreatedName,
                UpdatedName = s.UpdatedName,
                CreatedDate = s.CreatedDate,
                UpdatedDate = s.UpdatedDate
            })
            .Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
    }

    public async Task<ActivityResponse> GetActivityById(Guid activityId)
    {
        var listActivity = await GetListActivity("", 1, int.MaxValue);
        var activity = listActivity.FirstOrDefault(s => s.Id == activityId);
        if (activity != null) return activity;
        throw new AppException(string.Format(CommonMessage.Message_DataNotFound, "Activity"));
    }

    public async Task<Guid> AddActivity(Guid userId, ActivityRequest model)
    {
        #region Validate

        var data = await GetListActivity("", 1, int.MaxValue);
        if (data.Any(s =>
                !string.IsNullOrEmpty(model.ActivityName) &&
                (s.Code?.ToLower().Equals(model.Code?.ToLower()) ?? false)))
            throw new AppException(string.Format(CommonMessage.Message_Exists, "Code"));
        if (data.Any(s => s.ActivityName.ToLower().Equals(model.ActivityName.ToLower())))
            throw new AppException(string.Format(CommonMessage.Message_Exists, "Activity Name"));

        #endregion

        #region Add

        //var creator = _userService.GetById(userId);
        var sysActivity = new SysActivity
        {
            Code = model.Code,
            ActivityName = model.ActivityName,
            Description = model.Description,
            ApplicationName = model.ApplicationName,
            Note = model.Note,
            IsDelete = false,
            CreatedDate = DateTime.Now,
            //Creator = creator,
            CreatedUser = userId
        };
        _unitOfWork.Repository<SysActivity>().Add(sysActivity);

        #endregion

        var res = await _unitOfWork.SaveChangesAsync();
        if (res > 0)
        {
            _memoryCache.Remove(CacheKey.SysActivity);
            return sysActivity.Id;
        }

        throw new AppException(StatusMessage.Failure);
    }

    public async Task<bool> UpdateActivity(Guid userId, UpdateActivityRequest model)
    {
        #region Validate

        var data = await GetListActivity("", 1, int.MaxValue);
        if (data.Any(s =>
                !string.IsNullOrEmpty(model.ActivityName) &&
                (s.Code?.ToLower().Equals(model.Code?.ToLower()) ?? false) && s.Id != model.Id))
            throw new AppException(string.Format(CommonMessage.Message_Exists, "Code"));
        if (data.Any(s =>
                !string.IsNullOrEmpty(model.ActivityName) &&
                (s.ActivityName?.ToLower().Equals(model.ActivityName?.ToLower()) ?? false) && s.Id != model.Id))
            throw new AppException(string.Format(CommonMessage.Message_Exists, "Activity Name"));

        #endregion

        #region Update

        var activity = await _unitOfWork.Repository<SysActivity>()
            .FirstOrDefaultAsync(s => s.Id == model.Id && s.IsDelete == false);
        if (activity == null) throw new AppException(string.Format(CommonMessage.Message_DataNotFound, "Permission"));
        //var modifier = _userService.GetById(userId);
        activity.Code = model.Code;
        activity.ActivityName = model.ActivityName;
        activity.Description = model.Description;
        activity.ActivityName = model.ActivityName;
        activity.Note = model.Note;
        activity.UpdatedDate = DateTime.Now;
        //permission.Modifier = modifier;
        activity.UpdatedUser = userId;

        #endregion

        var res = await _unitOfWork.SaveChangesAsync();
        _memoryCache.Remove(CacheKey.SysActivity);
        return res > 0;
    }

    public async Task<bool> DeleteActivity(Guid userId, Guid activityId)
    {
        #region Delete

        var activity = await _unitOfWork.Repository<SysActivity>()
            .FirstOrDefaultAsync(s => s.Id == activityId && s.IsDelete == false);
        if (activity == null) throw new AppException(string.Format(CommonMessage.Message_DataNotFound, "Permission"));
        //var modifier = _userService.GetById(userId);
        activity.IsDelete = true;
        activity.UpdatedDate = DateTime.Now;
        //permission.Modifier = modifier;
        activity.UpdatedUser = userId;

        #endregion

        var res = await _unitOfWork.SaveChangesAsync();
        _memoryCache.Remove(CacheKey.SysActivity);
        return res > 0;
    }
}