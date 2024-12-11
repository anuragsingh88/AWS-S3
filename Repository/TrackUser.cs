using OtpNet;
using QRCoder;

namespace AWS_S3.Repository
{
    public class TrackUser
    {
        private static IHttpContextAccessor? _httpContextAccessor;
        public static void Initialize(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public static int AppUserID()
        {
            var appUserIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst("AppUserID");
            if (appUserIdClaim != null && int.TryParse(appUserIdClaim.Value, out int appUserId))
            {
                return appUserId;
            }
            return 0;
        }
        public static string GenerateSecretKey()
        {
            var key = KeyGeneration.GenerateRandomKey(20);
            return Base32Encoding.ToString(key);
        }
        public static string GenerateQrCode(string secretKey, string userEmail, string issuer)
        {
            string otpauthUrl = $"otpauth://totp/{issuer}:{userEmail}?secret={secretKey}&issuer={issuer}";

            using var qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(otpauthUrl, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeImage = qrCode.GetGraphic(20);
            return Convert.ToBase64String(qrCodeImage);
        }
        public static bool ValidateTOTP(string secretKey, string userInputCode)
        {
            var otp = new Totp(Base32Encoding.ToBytes(secretKey));
            return otp.VerifyTotp(userInputCode, out _, new VerificationWindow(2,2));
        }
    }
}
