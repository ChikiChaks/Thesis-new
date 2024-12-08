using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using System.Linq;
using System.Threading.Tasks;
using TriangleDbRepository;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Prog3_WebApi_Javascript.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : Controller
    {
        private readonly DbRepository _repository;
        private readonly ILogger<HomeController> _logger;

        public HomeController(DbRepository repository, ILogger<HomeController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet("getUserId")]
        public IActionResult GetUserId()
        {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
            {
                return Unauthorized(); // User is not logged in
            }

            return Ok(userId);
        }

        [HttpGet]
        public IActionResult Login()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("GoogleResponse")]
        public async Task<IActionResult> GoogleResponse()
        {
            try
            {
                var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                if (!result.Succeeded)
                {
                    _logger.LogError("Google authentication failed: {Error}", result.Failure?.Message);
                    return BadRequest("Google authentication failed: " + result.Failure?.Message + (result.Principal == null).ToString());
                }

                var claims = result.Principal.Identities.FirstOrDefault()?.Claims.Select(claim => new
                {
                    claim.Issuer,
                    claim.OriginalIssuer,
                    claim.Type,
                    claim.Value
                }).ToList();

                var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                if (email == null)
                {
                    _logger.LogError("Google authentication failed: email claim not found.");
                    return BadRequest("Google authentication failed: email claim not found.");
                }

                var userId = await GetOrCreateUserIdAsync(email);
                if (userId == 0)
                {
                    return BadRequest("Failed to create or retrieve user.");
                }

                HttpContext.Session.SetInt32("userId", userId);

                var responseData = await GetUserResponseDataAsync(userId);
                TempData["UserData"] = JsonConvert.SerializeObject(responseData);

                return Redirect("~/homepage.html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google authentication response.");
                return StatusCode(500, "Internal server error during Google authentication response.");
            }
        }

        private async Task<int> GetOrCreateUserIdAsync(string email)
        {
            var user = (await _repository.GetRecordsAsync<User>("SELECT * FROM Users WHERE Username = @Email", new { Email = email })).FirstOrDefault();
            if (user == null)
            {
                user = new User
                {
                    Username = email,
                    Stars = 0,
                    CurrentAvatar = 1 // Default avatar
                };
                var userId = await _repository.AddUserAsync(user.Username, null, null);
                user.UserId = userId;

                await _repository.SaveDataAsync("INSERT INTO Progress (UserId) VALUES (@UserId)", new { UserId = userId });

                _logger.LogInformation($"New user created with email: {email}");
            }
            return user.UserId;
        }

        private async Task<object> GetUserResponseDataAsync(int userId)
        {
            var user = (await _repository.GetRecordsAsync<User>("SELECT * FROM Users WHERE UserId = @UserId", new { UserId = userId })).FirstOrDefault();
            var progress = (await _repository.GetRecordsAsync<Progress>("SELECT * FROM Progress WHERE UserId = @UserId", new { UserId = userId })).FirstOrDefault();
            var avatar = (await _repository.GetRecordsAsync<Avatar>("SELECT * FROM Avatars WHERE AvatarId = @AvatarId", new { AvatarId = user.CurrentAvatar })).FirstOrDefault() ?? new Avatar
            {
                AvatarName = "Amigo1",
                CssStyle = ""
            };

            return new
            {
                username = user.Username,
                stars = user.Stars,
                avatar = avatar.AvatarName,
                avatarCss = avatar.CssStyle,
                level1 = progress?.Level1 ?? 0,
                level2 = progress?.Level2 ?? 0,
                level3 = progress?.Level3 ?? 0,
                level4 = progress?.Level4 ?? 0,
                level5 = progress?.Level5 ?? 0,
                level6 = progress?.Level6 ?? 0,
                level7 = progress?.Level7 ?? 0,
                level8 = progress?.Level8 ?? 0,
                level9 = progress?.Level9 ?? 0,
                level10 = progress?.Level10 ?? 0
            };
        }
    }
}
