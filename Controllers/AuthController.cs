using AWS_S3.Data;
using AWS_S3.Data.Models;
using AWS_S3.Repository;
using AWS_S3.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AWS_S3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(UserManager<ApplicationUser> userManager, IConfiguration configuration, ApplicationDbContext db) : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IConfiguration _configuration = configuration;
        private readonly ApplicationDbContext _db = db;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var appuserID = _db.ApplicationUsers.OrderByDescending(x => x.AppUserID).Select(x => x.AppUserID).FirstOrDefault();
            appuserID = (appuserID == null || appuserID == 0) ? 1 : appuserID + 1;
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                CreatedBy = TrackUser.AppUserID(),
                CreatedDateTime = DateTimeOffset.Now,
                AppUserID = appuserID
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            string secretKey = TrackUser.GenerateSecretKey();
            string qrCode = TrackUser.GenerateQrCode(secretKey, model.Email, "aws-s3");
            user.SecretKey = secretKey;
            user.Is2FAEnabled = true;
            _db.SaveChanges();
            return Ok(new { SecretKey = secretKey, QrCode = qrCode });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized("Invalid credentials");

            bool isValid = TrackUser.ValidateTOTP(user.SecretKey, model.Code);
            if (!isValid)
            {
                return Unauthorized("Invalid 2FA code.");
            }

            var authClaims = new List<Claim>
            {
                new(ClaimTypes.Email, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("AppUserID",user.AppUserID.ToString())
                //new(ClaimTypes.Role, assignRole),
            };
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:DurationInMinutes"])),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );
            return Ok(new JwtSecurityTokenHandler().WriteToken(token));
        }

        #region Encrypt/Decrypt
        [HttpPost("saveAwsCredential")]
        public async Task SaveAwsCredential(AWSConfigurationViewModel model)
        {
            var encSecretAccessKey = TrackUser.Encrypt(model.SecretAccessKey);
            var encAccessKeyID = TrackUser.Encrypt(model.AccessKeyID);

            // Save to database
            var systemconfig = new AWSConfiguration
            {
                AccessKeyID = encAccessKeyID,
                SecretAccessKey = encSecretAccessKey,
                Bucket = model.Bucket,
                Region = model.Region,
            };
            _db.AWSConfigurations.Add(systemconfig);
            await _db.SaveChangesAsync();
        }

        [HttpGet("getAwsCredential/{isAdmin:bool}")]
        public async Task<IActionResult> GetAwsCredential(bool isAdmin)
        {
            if (!isAdmin)
                throw new Exception("Secret not found");

            var res = await _db.AWSConfigurations.FirstOrDefaultAsync();

            var awsConfig = new AWSConfigurationViewModel
            {
                SecretAccessKey = TrackUser.Decrypt(res.SecretAccessKey),
                AccessKeyID = TrackUser.Decrypt(res.AccessKeyID),
                Bucket = res.Bucket,
                Region = res.Region
            };
            return Ok(awsConfig);
        }

        #endregion
    }
}
