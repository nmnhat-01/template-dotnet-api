using Model.RequestModel;
using Model.ResponseModel;

namespace DomainService.Interfaces;

public interface ILoginService
{
    LoginResponse Login(LoginRequest model, DeviceInfoRequest deviceInfo, string ipAddress);
    LoginResponse GetNewTokenByRefreshToken(RefreshTokenRequest model, DeviceInfoRequest deviceInfo, string ipAddress);
    bool RevokeToken(RefreshTokenRequest model, DeviceInfoRequest deviceInfo, string ipAddress);
    bool SendOTPLoginToPhone(SendOTPLoginRequest model, DeviceInfoRequest deviceInfo);
    LoginResponse LoginByOTP(LoginByOTPRequest model, DeviceInfoRequest deviceInfo, string ipAddress);
    string GetQRLogin(DeviceInfoRequest deviceInfo);
    bool VerifyQRCode(Guid userId, LoginByQrCodeRequest model);
    Task<LoginResponse> WaitVerifyQrCode(LoginByQrCodeRequest model, DeviceInfoRequest deviceInfo, string ipAddress);
    void ClearBlackListSms(ClearBlackListSmsRequest model);
}