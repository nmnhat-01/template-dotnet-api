using Common.Constant;
using Common.UnitOfWork.UnitOfWorkPattern;
using DomainService.Interfaces.TemplateAPI;
using Entity.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Implements.TemplateAPIService;

public class UserService : BaseService, IUserService
{
    public UserService(IUnitOfWork unitOfWork, IMemoryCache memoryCache) : base(unitOfWork, memoryCache)
    {
    }

    public SysUser GetById(Guid id)
    {
        var data = _unitOfWork.Repository<SysUser>().FirstOrDefault(s => s.UserId == id);
        if (data == null)
            throw new KeyNotFoundException(StatusMessage.DataNotFound);
        return data;
    }

    public List<SysUser> GetList()
    {
        List<SysUser> data;
        if (_memoryCache.TryGetValue(CacheKey.ListUser, out data))
        {
            return data.Take(10).ToList();
        }
        else
        {
            data = _unitOfWork.Repository<SysUser>().ToList();
            _memoryCache.Set(CacheKey.ListUser, data, CacheTime.CommmonUncache);
            return data.Take(10).ToList();
        }
    }
}