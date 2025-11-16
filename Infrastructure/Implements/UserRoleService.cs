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

public class UserRoleService : BaseService, IUserRoleService
{
    private readonly IUserService _userService;

    public UserRoleService(IUserService userService, IUnitOfWork unitOfWork, IMemoryCache memoryCache) : base(unitOfWork, memoryCache)
    {
        _userService = userService;
    }

    public async Task<List<UserRoleResponse>> GetListUserRole(string keyword, int pageIndex, int pageSize)
    {
        List<SysUserRole> data;
        if (_memoryCache.TryGetValue(CacheKey.SysUserRole, out data))
        {
            return data.Join(_unitOfWork.Repository<SysUser>(),
                    userRole => userRole.UserId, user => user.UserId,
                    (userRole, user) => new
                    {
                        UserRole = userRole,
                        UserName = user.UserName
                    })
                .Join(_unitOfWork.Repository<SysRole>(),
                    userRole => userRole.UserRole.RoleId, role => role.Id,
                    (userRole, role) => new
                    {
                        UserRole = userRole.UserRole,
                        UserName = userRole.UserName,
                        RoleName = role.RoleName
                    })
                .Where(s =>
                    (s.RoleName?.ToLower().Contains(keyword.ToLower()) ?? false) &&
                    (s.UserName?.ToLower().Contains(keyword.ToLower()) ?? false))
                .Select(s => new UserRoleResponse
                {
                    Id = s.UserRole.Id,
                    UserId = s.UserRole.UserId,
                    RoleId = s.UserRole.RoleId,
                    RoleName = s.RoleName ?? "",
                    UserName = s.UserName ?? "",
                    Description = s.UserRole.Description,
                    Note = s.UserRole.Note,
                    CreatedUser = s.UserRole.CreatedUser,
                    UpdatedUser = s.UserRole.UpdatedUser,
                    CreatedName = s.UserRole.CreatedName,
                    UpdatedName = s.UserRole.UpdatedName,
                    CreatedDate = s.UserRole.CreatedDate,
                    UpdatedDate = s.UserRole.UpdatedDate
                })
                .Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
        }

        data = await _unitOfWork.Repository<SysUserRole>().ToListAsync();
        _memoryCache.Set(CacheKey.SysUserRole, data, CacheTime.CommmonUncache);
        return data.Join(_unitOfWork.Repository<SysUser>(),
                userRole => userRole.UserId, user => user.UserId,
                (userRole, user) => new
                {
                    UserRole = userRole,
                    UserName = user.UserName
                })
            .Join(_unitOfWork.Repository<SysRole>(),
                userRole => userRole.UserRole.RoleId, role => role.Id,
                (userRole, role) => new
                {
                    UserRole = userRole.UserRole,
                    UserName = userRole.UserName,
                    RoleName = role.RoleName
                })
            .Where(s =>
                s.RoleName.ToLower().Contains(keyword.ToLower()) &&
                s.UserName.ToLower().Contains(keyword.ToLower()))
            .Select(s => new UserRoleResponse
            {
                Id = s.UserRole.Id,
                UserId = s.UserRole.UserId,
                RoleId = s.UserRole.RoleId,
                RoleName = s.RoleName,
                UserName = s.UserName,
                Description = s.UserRole.Description,
                Note = s.UserRole.Note,
                CreatedUser = s.UserRole.CreatedUser,
                UpdatedUser = s.UserRole.UpdatedUser,
                CreatedName = s.UserRole.CreatedName,
                UpdatedName = s.UserRole.UpdatedName,
                CreatedDate = s.UserRole.CreatedDate,
                UpdatedDate = s.UserRole.UpdatedDate
            })
            .Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
    }

    public async Task<UserRoleResponse> GetUserRoleById(Guid userRoleId)
    {
        var listUserRole = await GetListUserRole("", 1, int.MaxValue);
        var userRole = listUserRole.FirstOrDefault(s => s.Id == userRoleId);
        if (userRole != null) return userRole;
        throw new AppException(string.Format(CommonMessage.Message_DataNotFound, "User Role"));
    }

    public async Task<Guid> AddUserRole(Guid userId, UserRoleRequest model)
    {
        #region Validate

        var data = await _unitOfWork.Repository<SysUserRole>()
            .Where(s => s.UserId == model.UserId && s.RoleId == model.RoleId).ToListAsync();
        if (data != null)
        {
            _unitOfWork.Repository<SysUserRole>().RemoveRange(data);
        }
        #endregion

        #region Add

        //var creator = _userService.GetById(userId);
        var userRole = new SysUserRole
        {
            RoleId = model.RoleId,
            UserId = model.UserId,
            Description = model.Description,
            Note = model.Note,
            CreatedDate = DateTime.Now,
            //Creator = creator,
            CreatedUser = userId
        };
        _unitOfWork.Repository<SysUserRole>().Add(userRole);

        #endregion

        var res = await _unitOfWork.SaveChangesAsync();
        if (res > 0)
        {
            _memoryCache.Remove(CacheKey.SysUserRole);
            return userRole.Id;
        }

        throw new AppException(StatusMessage.Failure);
    }

    public async Task<bool> UpdateUserRole(Guid userId, UpdateUserRoleRequest model)
    {
        #region Validate

        var data = await _unitOfWork.Repository<SysUserRole>()
            .Where(s => s.UserId == model.UserId && s.RoleId == model.RoleId).ToListAsync();
        if (data != null)
        {
            _unitOfWork.Repository<SysUserRole>().RemoveRange(data);
        }

        #endregion

        #region Add

        //var creator = _userService.GetById(userId);
        var userRole = new SysUserRole
        {
            RoleId = model.RoleId,
            UserId = model.UserId,
            Description = model.Description,
            Note = model.Note,
            CreatedDate = DateTime.Now,
            //Creator = creator,
            CreatedUser = userId
        };
        _unitOfWork.Repository<SysUserRole>().Add(userRole);

        #endregion

        var res = await _unitOfWork.SaveChangesAsync();
        _memoryCache.Remove(CacheKey.SysUserRole);
        return res > 0;
    }

    public async Task<bool> DeleteUserRole(Guid userId, Guid userRoleId)
    {
        var data = await _unitOfWork.Repository<SysUserRole>()
            .FirstOrDefaultAsync(s => s.Id == userRoleId);
        if (data != null)
        {
            data.IsDelete = true;
            data.UpdatedDate = DateTime.Now;
            data.UpdatedUser = userId;
            _unitOfWork.Repository<SysUserRole>().Remove(data);
        }

        var res = await _unitOfWork.SaveChangesAsync();
        _memoryCache.Remove(CacheKey.SysUserRole);
        return res > 0;
    }
}