using Entity.Entities;
using Microsoft.AspNetCore.Http;

namespace DomainService.Interfaces;

public interface IUploadFileService
{
    Guid UploadFile(CollectionEnum collectionType, List<IFormFile> files, bool toAzure);
    Guid UploadImage(CollectionEnum collectionType, IFormFile file, bool toAzure);
}