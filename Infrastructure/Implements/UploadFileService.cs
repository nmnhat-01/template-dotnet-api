using Common.Authorization;
using Common.Settings;
using Common.UnitOfWork.UnitOfWorkPattern;
using Common.Utils;
using DomainService.Interfaces;
using Entity.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Infrastructure.Implements;

public class UploadFileService : BaseService, IUploadFileService
{
    private readonly AppSettings _appSettings;
    public UploadFileService(IUnitOfWork unitOfWork, IMemoryCache memoryCache, IOptions<AppSettings> appSettings) : base(unitOfWork, memoryCache)
    {
        this._appSettings = appSettings.Value;
    }

    public Guid UploadFile(CollectionEnum collectionType, List<IFormFile> files, bool toAzure)
    {
        var mediaCollection = new SysMediaCollection
        {
            Id = Guid.NewGuid(),
            CollectionType = collectionType,
        };
        var listMedia = new List<SysMedia>();
        foreach (var file in files)
        {
            var media = new SysMedia
            {
                Id = Guid.NewGuid(),
                FileName = file.Name,
                ContentType = file.ContentType,
                Path = toAzure
                    ? Utils.UploadFileToAzure(file, _appSettings.StrStorageFileAzure, _appSettings.StrContainerRefer)
                    : Utils.UploadFileToLocal(file, _appSettings.FolderUploadFile),
                MediaCollectionId = mediaCollection.Id,
                IsExternal = Path.HasExtension(file.FileName)
            };
            listMedia.Add(media);
        }

        mediaCollection.Medias = listMedia;
        _unitOfWork.Repository<SysMediaCollection>().Add(mediaCollection);
        _unitOfWork.SaveChangesAsync();

        return mediaCollection.Id;
    }

    public Guid UploadImage(CollectionEnum collectionType, IFormFile file, bool toAzure)
    {
        var mediaCollection = new SysMediaCollection
        {
            Id = Guid.NewGuid(),
            CollectionType = collectionType,
        };
        var listMedia = new List<SysMedia>();
        var listUrl = toAzure
            ? Utils.UploadImageToAzure(file, _appSettings.StrStorageFileAzure, _appSettings.StrContainerRefer,
                new ModelUrlImage())
            : Utils.UploadImageToLocal(file, _appSettings.FolderUploadFile, new ModelUrlImage());
        if (listUrl.Count() != 3) throw new AppException("Upload error!");
        
        foreach (var url in listUrl)
        {
            var media = new SysMedia
            {
                Id = Guid.NewGuid(),
                FileName = file.Name,
                ContentType = file.ContentType,
                Path = url,
                MediaCollectionId = mediaCollection.Id,
                IsExternal = Path.HasExtension(file.FileName)
            };
            listMedia.Add(media);
        }
        mediaCollection.Medias = listMedia;
        _unitOfWork.Repository<SysMediaCollection>().Add(mediaCollection);
        _unitOfWork.SaveChangesAsync();

        return mediaCollection.Id;
    }
}