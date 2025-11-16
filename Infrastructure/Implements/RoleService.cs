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

public class RoleService : BaseService, IRoleService
{
    private readonly IUserService _userService;
    public RoleService(IUserService userService, IUnitOfWork unitOfWork, IMemoryCache memoryCache) : base(unitOfWork, memoryCache)
    {
        _userService = userService;
    }

    public async Task<List<RoleResponse>> GetListRole(string keyword, int pageIndex, int pageSize)
    {
        List<SysRole> data;
        if (_memoryCache.TryGetValue(CacheKey.SysRole, out data))
        {
            return data.Where(s =>
                    (s.RoleName?.ToLower().Contains(keyword.ToLower()) ?? false))
                .Select(s => new RoleResponse
                {
                    Id = s.Id,
                    RoleName = s.RoleName,
                    Description = s.Description,
                    Note = s.Note,
                    RoleType = s.RoleType,
                    CreatedUser = s.CreatedUser,
                    UpdatedUser = s.UpdatedUser,
                    CreatedName = s.CreatedName,
                    UpdatedName = s.UpdatedName,
                    CreatedDate = s.CreatedDate,
                    UpdatedDate = s.UpdatedDate
                })
                .Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
        }

        data = await _unitOfWork.Repository<SysRole>().ToListAsync();
        _memoryCache.Set(CacheKey.SysRole, data, CacheTime.CommmonUncache);
        return data.Where(s =>
                (s.RoleName?.ToLower().Contains(keyword.ToLower()) ?? false))
            .Select(s => new RoleResponse
            {
                Id = s.Id,
                RoleName = s.RoleName,
                Description = s.Description,
                Note = s.Note,
                RoleType = s.RoleType,
                CreatedUser = s.CreatedUser,
                UpdatedUser = s.UpdatedUser,
                CreatedName = s.CreatedName,
                UpdatedName = s.UpdatedName,
                CreatedDate = s.CreatedDate,
                UpdatedDate = s.UpdatedDate
            })
            .Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
    }

    public async Task<RoleResponse> GetRoleById(Guid roleId)
    {
        var listRole = await GetListRole("", 1, int.MaxValue);
        var role = listRole.FirstOrDefault(s => s.Id == roleId);
        if (role != null) return role;
        throw new AppException(string.Format(CommonMessage.Message_DataNotFound, "Role"));
    }

    public async Task<Guid> AddRole(Guid userId, RoleRequest model)
    {
        #region Validate

        var data = await GetListRole("", 1, int.MaxValue);
        if (data.Any(s =>
                !string.IsNullOrEmpty(model.RoleName) &&
                (s.RoleName?.ToLower().Equals(model.RoleName?.ToLower()) ?? false)))
            throw new AppException(string.Format(CommonMessage.Message_Exists, "Role Name"));

        #endregion

        #region Add

        //var creator = _userService.GetById(userId);
        var sysRole = new SysRole
        {
            RoleName = model.RoleName,
            Description = model.Description,
            Note = model.Note,
            RoleType = model.RoleType,
            IsDelete = false,
            CreatedDate = DateTime.Now,
            //Creator = creator,
            CreatedUser = userId
        };
        _unitOfWork.Repository<SysRole>().Add(sysRole);

        #endregion

        var res = await _unitOfWork.SaveChangesAsync();
        if (res > 0)
        {
            _memoryCache.Remove(CacheKey.SysRole);
            return sysRole.Id;
        }

        throw new AppException(StatusMessage.Failure);
    }

    public async Task<bool> UpdateRole(Guid userId, UpdateRoleRequest model)
    {
        #region Validate

        var data = await GetListRole("", 1, int.MaxValue);
        if (data.Any(s =>
                !string.IsNullOrEmpty(model.RoleName) &&
                (s.RoleName?.ToLower().Equals(model.RoleName?.ToLower()) ?? false) && s.Id != model.Id))
            throw new AppException(string.Format(CommonMessage.Message_Exists, "Role Name"));

        #endregion

        #region Update

        var role = await _unitOfWork.Repository<SysRole>()
            .FirstOrDefaultAsync(s => s.Id == model.Id && s.IsDelete == false);
        if (role == null) throw new AppException(string.Format(CommonMessage.Message_DataNotFound, "Role"));
        //var modifier = _userService.GetById(userId);
        role.RoleName = model.RoleName;
        role.Description = model.Description;
        role.Note = model.Note;
        role.RoleType = model.RoleType;
        role.UpdatedDate = DateTime.Now;
        //permission.Modifier = modifier;
        role.UpdatedUser = userId;

        #endregion

        var res = await _unitOfWork.SaveChangesAsync();
        _memoryCache.Remove(CacheKey.SysRole);
        return res > 0;
    }

    public async Task<bool> DeleteRole(Guid userId, Guid roleId)
    {
        #region Delete

        var role = await _unitOfWork.Repository<SysRole>()
            .FirstOrDefaultAsync(s => s.Id == roleId && s.IsDelete == false);
        if (role == null) throw new AppException(string.Format(CommonMessage.Message_DataNotFound, "Role"));
        //var modifier = _userService.GetById(userId);
        role.IsDelete = true;
        role.UpdatedDate = DateTime.Now;
        //permission.Modifier = modifier;
        role.UpdatedUser = userId;

        #endregion

        var res = await _unitOfWork.SaveChangesAsync();
        _memoryCache.Remove(CacheKey.SysRole);
        return res > 0;
    }
}