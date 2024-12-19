using OtpNet;
using QRCoder;
using System.Security.Cryptography;
using System.Text;

namespace AWS_S3.Repository
{
    public class TrackUser
    {
        private static IHttpContextAccessor? _httpContextAccessor;
        private static readonly string EncryptionKey = "5DDBD030-8BAF-4205-B01F-30D461292658";
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
            return otp.VerifyTotp(userInputCode, out _, new VerificationWindow(10,10));
        }

        #region Encrypt/Decrypt
        public static string Encrypt(string plainText)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(EncryptionKey.Substring(0, 16));
            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.IV = new byte[16]; // Initialization Vector (IV) with zeroes
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        public static string Decrypt(string cipherText)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(EncryptionKey.Substring(0, 16));
            byte[] buffer = Convert.FromBase64String(cipherText);
            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.IV = new byte[16]; // Initialization Vector (IV) with zeroes
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(buffer);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
        #endregion
    }
}
