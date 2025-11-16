using Model.ResponseModel;

namespace Common.Authorization.Utils;

public interface IJwtUtils
{
    string GenerateToken(Guid userId, string userName, string UDID);
    Guid? ValidateToken(string token);
    //public RefreshToken GenerateRefreshToken(Guid userId, string ipAddress);
    RfTokenResponse GenerateRefreshToken(Guid userId, string userName, string UDID, string skey, string Issuer, string Audience, string ipAddress);
}