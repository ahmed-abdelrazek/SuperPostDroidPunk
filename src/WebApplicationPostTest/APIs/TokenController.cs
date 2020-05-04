using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebApplicationPostTest.Models;

namespace WebApplicationPostTest.APIs
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        public IConfiguration Configuration { get; }
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<UserAuthenticateModel> _logger;

        public TokenController(IConfiguration configuration,
            SignInManager<ApplicationUser> signInManager,
            ILogger<UserAuthenticateModel> logger,
            UserManager<ApplicationUser> userManager)
        {
            Configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("Auth")]
        public async Task<IActionResult> Auth([FromBody]UserAuthenticateModel input)
        {
            if (input.Email.IndexOf('@') > -1)
            {
                //Validate email format
                string emailRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                                       @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                                          @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
                Regex re = new Regex(emailRegex);
                if (!re.IsMatch(input.Email))
                {
                    ModelState.AddModelError("Email", "Email is not valid");
                }
            }
            else
            {
                //validate Username format
                string emailRegex = @"^[A-Za-z0-9]+(?:[ _-][A-Za-z0-9]+)*$";
                Regex re = new Regex(emailRegex);
                if (!re.IsMatch(input.Email))
                {
                    ModelState.AddModelError("Email", "Username is not valid");
                }
            }

            if (ModelState.IsValid)
            {
                if (input.Email.IndexOf('@') > -1)
                {
                    input.Email = await _userManager.Users.Where(r => r.Email == input.Email)
                        .Select(x => x.UserName).SingleOrDefaultAsync();
                }

                var result = await _signInManager.PasswordSignInAsync(input.Email, input.Password, false, false);

                if (result.Succeeded)
                {
                    var appUser = await _userManager.Users.SingleOrDefaultAsync(r => r.UserName == input.Email);
                    var token = await GenerateJwtToken(appUser);

                    return Ok(token);
                }

                return Unauthorized();
            }

            return ValidationProblem();
        }

        private async Task<TokenModel> GenerateJwtToken(ApplicationUser user)
        {
            TokenModel tm = new TokenModel();

            await Task.Run(() =>
            {
                var claims = new[]
                {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtToken:SecretKey"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var utcNow = DateTime.UtcNow;
                var expires = utcNow.AddDays(Convert.ToDouble(Configuration["JwtToken:JwtExpireDays"]));

                var token = new JwtSecurityToken(
                    issuer: Configuration["JwtToken:Issuer"],
                    audience: Configuration["JwtToken:Issuer"],
                    claims,
                    expires: expires,
                    signingCredentials: creds
                );

                var usDtfi = new CultureInfo("en-GB", false);

                tm = new TokenModel
                {
                    AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                    TokenType = "bearer",
                    Expirein = (expires - utcNow).TotalMilliseconds.ToString(),
                    UserName = user.UserName,
                    Issued = utcNow.ToString("ddd, dd MMM yyy HH:mm:ss", usDtfi) + " GMT",
                    Expires = expires.ToString("ddd, dd MMM yyy HH:mm:ss", usDtfi) + " GMT"
                };
            });

            return tm;
        }
    }
}