using AWS_S3.Data;
using AWS_S3.Data.Models;
using AWS_S3.Repository;
using AWS_S3.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

            return Ok("User registered successfully!");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized("Invalid credentials");

            if (user.Is2FAEnabled)
            {
                return Ok(new { message = "Enter 2FA code", Is2FAEnabled = true });
            }
            string token = GenerateToken(user);
            return Ok(token);
        }

        private string GenerateToken(ApplicationUser user)
        {
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
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("generate-2fa")]
        public IActionResult Generate2FA([FromBody] LoginModel model)
        {
            string secretKey = TrackUser.GenerateSecretKey();
            string qrCode = TrackUser.GenerateQrCode(secretKey, model.Email, "aws-s3");
            var user = _db.ApplicationUsers.Where(x => x.Email == model.Email).FirstOrDefault();
            if (user == null)
                return Unauthorized("Invalid credentials.");
            user.SecretKey = secretKey;
            user.Is2FAEnabled = true;
            _db.SaveChanges();
            return Ok(new { SecretKey = secretKey, QrCode = qrCode });
        }
        [HttpPost("validate-2fa")]
        public IActionResult Validate2FA([FromBody] TwoFactorAuth request)
        {
            var user = _db.ApplicationUsers.Where(x => x.Email == request.Email).FirstOrDefault();
            bool isValid = TrackUser.ValidateTOTP(user.SecretKey, request.Code);
            if (!isValid)
            {
                return Unauthorized("Invalid 2FA code.");
            }
            var token = GenerateToken(user);
            return Ok(token);
        }
    }
}
