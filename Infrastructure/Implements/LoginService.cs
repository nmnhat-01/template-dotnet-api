using Common.Authorization;
using Common.Authorization.Utils;
using Common.Constant;
using Common.Settings;
using Common.UnitOfWork.UnitOfWorkPattern;
using Common.Utils;
using DomainService.Interfaces;
using Entity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Model.RequestModel;
using Model.ResponseModel;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Implements
{
    public class LoginService : BaseService, ILoginService
    {
        private IJwtUtils _jwtUtils;
        private readonly StrJWT _strJwt;
        private readonly AppSettings _appSettings;

        public LoginService(IUnitOfWork unitOfWork, IMemoryCache memoryCache, IJwtUtils jwtUtils,
            IOptions<StrJWT> strJwt, IOptions<AppSettings> appSettings) : base(unitOfWork, memoryCache)
        {
            _jwtUtils = jwtUtils;
            this._strJwt = strJwt.Value;
            this._appSettings = appSettings.Value;
        }

        public LoginResponse Login(LoginRequest model, DeviceInfoRequest deviceInfo, string ipAddress)
        {
            var user = _unitOfWork.Repository<SysUser>().FirstOrDefault(s => s.UserName == model.UserName && s.IsDelete == null || s.IsDelete == false);

            if (user == null) throw new AppException("Username is not found!!! Please enter again ");
            PasswordHasher<string> pw = new PasswordHasher<string>();
            if (pw.VerifyHashedPassword(model.UserName, user.Password, model.Password) == PasswordVerificationResult.Failed)
            {
                throw new AppException("Password is wrong!!! Please enter again ");
            }
            if (pw.VerifyHashedPassword(model.UserName, user.Password, model.Password) == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.Password = pw.HashPassword(user.UserName, model.Password);
                _unitOfWork.Repository<SysUser>().Update(user);
                _unitOfWork.SaveChangesAsync();
            }

            var device = _unitOfWork.Repository<Device>().FirstOrDefault(s => s.UDID == deviceInfo.UDID);

            var jwtToken = _jwtUtils.GenerateToken(user.UserId, user.UserName, deviceInfo.UDID);
            string? skey = _strJwt.Key;
            string? issuer = _strJwt.Issuer;
            string? audience = _strJwt.Audience;
            var refreshToken = _jwtUtils.GenerateRefreshToken(user.UserId, user.UserName, deviceInfo.UDID, skey,
                issuer, audience, ipAddress);

            if (device == null)
            {
                device = new Device
                {
                    UserId = user.UserId,
                    UDID = deviceInfo.UDID,
                    OSVersion = deviceInfo.OSVersion,
                    OSName = deviceInfo.OSName,
                    DeviceType = deviceInfo.DeviceType,
                    DeviceName = deviceInfo.DeviceName,
                    DeviceDescription = deviceInfo.DeviceDescription,
                    IsActive = true,
                    RefreshToken = refreshToken.Token,
                    RfTokenCreateTime = refreshToken.CreateTime,
                    RfTokenExpiryTime = refreshToken.Expires,
                    RfTokenCreatedByIp = refreshToken.CreatedByIp,

                    CreatedDate = DateTime.Now,
                    CreatedUser = user.UserId,
                };
                _unitOfWork.Repository<Device>().AddAsync(device);
            }
            else
            {
                device.UserId = user.UserId;
                device.IsActive = true;
                device.RefreshToken = refreshToken.Token;
                device.RfTokenCreateTime = refreshToken.CreateTime;
                device.RfTokenExpiryTime = refreshToken.Expires;
                device.RfTokenCreatedByIp = refreshToken.CreatedByIp;
                device.RfTokenRevokedTime = null;
                device.RfTokenRevokedByIp = null;
                device.IsDelete = false;

                device.UpdatedDate = DateTime.Now;
                device.UpdatedUser = user.UserId;

                _unitOfWork.Repository<Device>().Update(device);
            }
            _unitOfWork.Dispose();

            var retUser = new LoginResponse();
            retUser.userId = user.UserId;
            retUser.SetToken(jwtToken);
            retUser.SetRefreshToken(refreshToken.Token);
            return retUser;
        }

        public LoginResponse GetNewTokenByRefreshToken(RefreshTokenRequest model, DeviceInfoRequest deviceInfo, string ipAddress)
        {
            var user = _unitOfWork.Repository<SysUser>().Include(s => s.Devices).SingleOrDefault(s =>
                s.IsDelete != true && s.Devices.Any(t => t.UDID == deviceInfo.UDID && t.IsActive && !t.IsDelete && (t.RefreshToken == model.RefreshToken)));

            // return null if no user found with token
            if (user == null) throw new KeyNotFoundException(StatusMessage.DataNotFound);

            var device = user.Devices.Single(x => x.UDID == deviceInfo.UDID && x.RefreshToken == model.RefreshToken);

            // return null if token is no longer active
            if (device.RfTokenExpiryTime <= DateTime.UtcNow || device.RfTokenRevokedTime != null ||
                device.RfTokenRevokedByIp != null || device.RfTokenCreatedByIp != ipAddress)
                throw new AppException(StatusMessage.DataInputInvalid);

            // replace old refresh token with a new one and save
            string? skey = _strJwt.Key;
            string? issuer = _strJwt.Issuer;
            string? audience = _strJwt.Audience;
            var newRefreshToken = _jwtUtils.GenerateRefreshToken(user.UserId, user.UserName, deviceInfo.UDID, skey, issuer, audience, ipAddress);
            device.RefreshToken = newRefreshToken.Token;
            device.RfTokenCreateTime = newRefreshToken.CreateTime;
            device.RfTokenExpiryTime = newRefreshToken.Expires;
            device.RfTokenCreatedByIp = newRefreshToken.CreatedByIp;
            device.RfTokenRevokedTime = null;
            device.RfTokenRevokedByIp = null;
            _unitOfWork.Repository<Device>().Update(device);
            _unitOfWork.Repository<SysUser>().Update(user);
            _unitOfWork.SaveChangesAsync();
            _unitOfWork.Dispose();

            var jwtToken = _jwtUtils.GenerateToken(user.UserId, user.UserName, deviceInfo.UDID);
            var res = new LoginResponse();
            res.SetToken(jwtToken);
            res.SetRefreshToken(newRefreshToken.Token);

            return res;
        }

        public bool RevokeToken(RefreshTokenRequest model, DeviceInfoRequest deviceInfo, string ipAddress)
        {
            var user = _unitOfWork.Repository<SysUser>().Include(s => s.Devices).SingleOrDefault(
                s => s.IsDelete != true && s.Devices.Any(t => t.UDID == deviceInfo.UDID && (t.RefreshToken == model.RefreshToken)));

            // return false if no user found with token
            if (user == null) throw new KeyNotFoundException("Token not found");

            var device = user.Devices.Single(x => x.UDID == deviceInfo.UDID && x.RefreshToken == model.RefreshToken);

            // return false if token is not active
            if (device.RfTokenExpiryTime <= DateTime.UtcNow || device.RfTokenRevokedTime != null ||
                device.RfTokenRevokedByIp != null || device.RfTokenCreatedByIp != ipAddress)
                throw new AppException("Token already expires");

            // revoke token and save
            device.RfTokenRevokedTime = DateTime.UtcNow;
            device.RfTokenRevokedByIp = ipAddress;
            _unitOfWork.Repository<Device>().Update(device);
            _unitOfWork.Repository<SysUser>().Update(user);
            _unitOfWork.SaveChangesAsync();
            _unitOfWork.Dispose();

            return true;
        }

        public bool SendOTPLoginToPhone(SendOTPLoginRequest model, DeviceInfoRequest deviceInfo)
        {
            if (!RegexUtilities.IsValidPhone(model.UserPhone)) throw new AppException("Phone is wrong!");
            var otpCode = Utils.GenerateOneTimeOTP();
            var modelOtp = new ModelOtp
            {
                Code = otpCode,
                UDID = deviceInfo.UDID
            };

            Dictionary<string, int> dataBlackList;
            int numSent = 0;

            if (_memoryCache.TryGetValue(CacheKey.BlackListSms, out dataBlackList))
            {
                if (dataBlackList.TryGetValue(model.UserPhone, out numSent) && numSent >= 3)
                    throw new AppException("Too many sms sent, please try again in 24 hours!");
            }
            else
            {
                dataBlackList = new Dictionary<string, int>();
            }

            var res = SmsUtils.SendOTPToPhone(model.UserPhone, otpCode, _appSettings.SmsToken, _appSettings.SmsServiceUrl);
            if (res)
            {
                numSent += 1;
                dataBlackList[model.UserPhone] = numSent;
                _memoryCache.Set(CacheKey.BlackListSms, dataBlackList, CacheTime.BlackList);
            }

            _memoryCache.Set(model.UserPhone, modelOtp, CacheTime.OTP);
            return res;
        }

        public LoginResponse LoginByOTP(LoginByOTPRequest model, DeviceInfoRequest deviceInfo, string ipAddress)
        {
            //Check phone to get user
            var user = _unitOfWork.Repository<SysUser>()
                .FirstOrDefault(s => s.UserName == model.UserPhone && (s.IsDelete == null || s.IsDelete == false));//s.UserName == model.UserPhone -> s.phone == model.userPhone
            if (user == null) throw new AppException("Phone is not found or not register!");

            //Check otp
            ModelOtp otpSaved;
            if (!_memoryCache.TryGetValue(model.UserPhone, out otpSaved)) throw new AppException("Phone is wrong or OTP was expired, please re-enter the OTP");
            otpSaved.NumCheck += 1;
            if (otpSaved.NumCheck > 3) throw new AppException("OTP has been entered too many times, please re-enter the OTP");
            if (otpSaved.Expire < DateTime.Now) throw new AppException("OTP was expired, please re-enter the OTP");
            if (otpSaved.Code != model.OTP) throw new AppException("OTP is wrong!");
            if (otpSaved.UDID != deviceInfo.UDID) throw new AppException("Device is wrong!");

            //Response
            var device = _unitOfWork.Repository<Device>().FirstOrDefault(s => s.UDID == deviceInfo.UDID);

            string? skey = _strJwt.Key;
            string? issuer = _strJwt.Issuer;
            string? audience = _strJwt.Audience;
            var refreshToken = _jwtUtils.GenerateRefreshToken(user.UserId, user.UserName, deviceInfo.UDID, skey,
                issuer, audience, ipAddress);

            if (device == null)
            {
                device = new Device
                {
                    UserId = user.UserId,
                    UDID = deviceInfo.UDID,
                    OSVersion = deviceInfo.OSVersion,
                    OSName = deviceInfo.OSName,
                    DeviceType = deviceInfo.DeviceType,
                    DeviceName = deviceInfo.DeviceName,
                    DeviceDescription = deviceInfo.DeviceDescription,
                    IsActive = true,
                    RefreshToken = refreshToken.Token,
                    RfTokenCreateTime = refreshToken.CreateTime,
                    RfTokenExpiryTime = refreshToken.Expires,
                    RfTokenCreatedByIp = refreshToken.CreatedByIp,

                    CreatedDate = DateTime.Now,
                    CreatedUser = user.UserId,
                };
                _unitOfWork.Repository<Device>().AddAsync(device);
            }
            else
            {
                device.UserId = user.UserId;
                device.IsActive = true;
                device.RefreshToken = refreshToken.Token;
                device.RfTokenCreateTime = refreshToken.CreateTime;
                device.RfTokenExpiryTime = refreshToken.Expires;
                device.RfTokenCreatedByIp = refreshToken.CreatedByIp;
                device.RfTokenRevokedTime = null;
                device.RfTokenRevokedByIp = null;
                device.IsDelete = false;

                device.UpdatedDate = DateTime.Now;
                device.UpdatedUser = user.UserId;

                _unitOfWork.Repository<Device>().Update(device);
            }
            _unitOfWork.Dispose();

            var jwtToken = _jwtUtils.GenerateToken(user.UserId, user.UserName, deviceInfo.UDID);
            var res = new LoginResponse();
            res.SetToken(jwtToken);
            res.SetRefreshToken(refreshToken.Token);

            return res;
        }

        public string GetQRLogin(DeviceInfoRequest deviceInfo)
        {
            var qr = Guid.NewGuid().ToString();
            var qrCode = Utils.Encode64(qr);
            var modelQr = new ModelOtp
            {
                Code = qrCode,
                UDID = deviceInfo.UDID
            };
            _memoryCache.Set(qr, modelQr, CacheTime.QrCode);
            return qrCode;
        }

        public bool VerifyQRCode(Guid userId, LoginByQrCodeRequest model)
        {
            var qr = Utils.Decode64(model.QrCode);
            var user = _unitOfWork.Repository<SysUser>().FirstOrDefault(s =>
                s.UserId == userId && (s.IsDelete == null || s.IsDelete == false));//s.Phone == userPhone
            if (user == null) throw new AppException("User is not found!");

            //Check otp
            ModelOtp modelQr;
            if (!_memoryCache.TryGetValue(qr, out modelQr)) throw new AppException("Phone is wrong or OTP was expired, please re-enter the OTP");
            modelQr.NumCheck += 1;
            if (modelQr.NumCheck > 3) throw new AppException("OTP has been entered too many times, please re-enter the OTP");
            if (modelQr.Expire < DateTime.Now) throw new AppException("OTP was expired, please re-enter the OTP");
            if (modelQr.Code != model.QrCode) throw new AppException("OTP is wrong!");

            modelQr.userVerifiedId = user.UserId;
            modelQr.IsVerify = true;
            return true;
        }

        //Nhac front end tang t/g time out call api
        public async Task<LoginResponse> WaitVerifyQrCode(LoginByQrCodeRequest model, DeviceInfoRequest deviceInfo, string ipAddress)
        {
            var qr = Utils.Decode64(model.QrCode);
            while (!CheckVerifyQrCode(qr, deviceInfo.UDID))
            {
                await Task.Delay(2000);
            }

            ModelOtp modelQr;
            if (!_memoryCache.TryGetValue(qr, out modelQr)) throw new AppException("QRCode was expired");
            //Check phone to get user
            var user = _unitOfWork.Repository<SysUser>()
                .FirstOrDefault(s => s.UserId == modelQr.userVerifiedId && (s.IsDelete == null || s.IsDelete == false));//s.Phone == userPhone
            if (user == null) throw new AppException("Phone is not found or not register!");

            //Response
            var device = _unitOfWork.Repository<Device>().FirstOrDefault(s => s.UDID == deviceInfo.UDID);

            string? skey = _strJwt.Key;
            string? issuer = _strJwt.Issuer;
            string? audience = _strJwt.Audience;
            var refreshToken = _jwtUtils.GenerateRefreshToken(user.UserId, user.UserName, deviceInfo.UDID, skey,
                issuer, audience, ipAddress);

            if (device == null)
            {
                device = new Device
                {
                    UserId = user.UserId,
                    UDID = deviceInfo.UDID,
                    OSVersion = deviceInfo.OSVersion,
                    OSName = deviceInfo.OSName,
                    DeviceType = deviceInfo.DeviceType,
                    DeviceName = deviceInfo.DeviceName,
                    DeviceDescription = deviceInfo.DeviceDescription,
                    IsActive = true,
                    RefreshToken = refreshToken.Token,
                    RfTokenCreateTime = refreshToken.CreateTime,
                    RfTokenExpiryTime = refreshToken.Expires,
                    RfTokenCreatedByIp = refreshToken.CreatedByIp,

                    CreatedDate = DateTime.Now,
                    CreatedUser = user.UserId,
                };
                _unitOfWork.Repository<Device>().AddAsync(device);
            }
            else
            {
                device.UserId = user.UserId;
                device.IsActive = true;
                device.RefreshToken = refreshToken.Token;
                device.RfTokenCreateTime = refreshToken.CreateTime;
                device.RfTokenExpiryTime = refreshToken.Expires;
                device.RfTokenCreatedByIp = refreshToken.CreatedByIp;
                device.RfTokenRevokedTime = null;
                device.RfTokenRevokedByIp = null;
                device.IsDelete = false;

                device.UpdatedDate = DateTime.Now;
                device.UpdatedUser = user.UserId;

                _unitOfWork.Repository<Device>().Update(device);
            }
            _unitOfWork.Dispose();

            var jwtToken = _jwtUtils.GenerateToken(user.UserId, user.UserName, deviceInfo.UDID);
            var res = new LoginResponse();
            res.SetToken(jwtToken);
            res.SetRefreshToken(refreshToken.Token);

            return res;
        }

        public void ClearBlackListSms(ClearBlackListSmsRequest model)
        {
            var keyBlackList = "BlackListSms";
            if (model.UserPhone != null)
            {
                Dictionary<string, int> dataBlackList;
                if (_memoryCache.TryGetValue(keyBlackList, out dataBlackList))
                {
                    dataBlackList.Remove(model.UserPhone);
                }
            }
            else
                _memoryCache.Remove(keyBlackList);
        }

        private bool CheckVerifyQrCode(string keyGetQrCode, string UDID)
        {
            //Check otp
            ModelOtp qrSaved;
            if (!_memoryCache.TryGetValue(keyGetQrCode, out qrSaved)) throw new AppException("QRCode was expired");
            if (qrSaved.Expire < DateTime.Now) throw new AppException("QRCode was expired");
            if (qrSaved.UDID != UDID) throw new AppException("Wrong device!");

            return qrSaved.IsVerify;
        }
    }

    public class ModelOtp
    {
        [Required]
        public string Code { get; set; }
        public DateTime Expire { get; set; } = DateTime.Now.AddMinutes(5);
        [Required]
        public string UDID { get; set; }
        public int NumCheck { get; set; } = 0;
        public bool IsVerify { get; set; } = false;
        public Guid userVerifiedId { get; set; } = Guid.Empty;
    }
}