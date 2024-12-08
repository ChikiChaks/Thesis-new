using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using TriangleDbRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Prog3_WebApi_Javascript.Controllers
{
    [ApiController]
    [Route("api/user")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly DbRepository _repository;
        private readonly ILogger<UserController> _logger;

        public UserController(DbRepository repository, ILogger<UserController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet("homepage")]
        public async Task<IActionResult> GetHomepageData()
        {
            int? userId = GetUserIdFromSession();
            if (userId == null)
            {
                _logger.LogError("Invalid user ID.");
                return BadRequest(new { error = "Invalid user ID" });
            }

            _logger.LogInformation("Fetching user data for user ID: {UserId}", userId);

            var user = (await _repository.GetRecordsAsync<User>("SELECT * FROM Users WHERE UserId = @UserId", new { UserId = userId })).FirstOrDefault();
            if (user == null)
            {
                _logger.LogError("User not found. UserId: {UserId}", userId);
                return NotFound(new { error = "User not found" });
            }

            _logger.LogInformation("User found: {Username}", user.Username);

            var progress = (await _repository.GetRecordsAsync<Progress>("SELECT * FROM Progress WHERE UserId = @UserId", new { UserId = userId })).FirstOrDefault();
            _logger.LogInformation("User progress fetched.");

            var avatar = (await _repository.GetRecordsAsync<Avatar>("SELECT * FROM Avatars WHERE AvatarId = @AvatarId", new { AvatarId = user.CurrentAvatar })).FirstOrDefault();
            if (avatar == null)
            {
                _logger.LogWarning("Avatar not found. Defaulting to user.png.");
                avatar = new Avatar
                {
                    AvatarName = "Avatar1.png",
                    CssStyle = ""
                };
            }

            var responseData = new
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

            _logger.LogInformation("User data prepared for response.");

            return Ok(responseData);
        }

        [HttpPost("avatar")]
        public async Task<IActionResult> UpdateAvatar([FromBody] AvatarUpdateRequest request)
        {
            int? userId = GetUserIdFromSession();
            if (userId == null)
            {
                return BadRequest(new { error = "Invalid user ID" });
            }

            var user = (await _repository.GetRecordsAsync<User>("SELECT * FROM Users WHERE UserId = @UserId", new { UserId = userId })).FirstOrDefault();
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            var avatar = (await _repository.GetRecordsAsync<Avatar>("SELECT * FROM Avatars WHERE AvatarId = @AvatarId", new { AvatarId = request.AvatarId })).FirstOrDefault();
            if (avatar == null)
            {
                return NotFound(new { error = "Avatar not found" });
            }

            if (user.Stars < avatar.StarsRequired)
            {
                return BadRequest(new { error = "Not enough stars to unlock this avatar" });
            }

            var success = await _repository.SaveDataAsync("UPDATE Users SET CurrentAvatar = @AvatarId WHERE UserId = @UserId",
                                                          new { AvatarId = request.AvatarId, UserId = userId });

            if (!success)
            {
                _logger.LogError("Error updating avatar for UserId: {UserId}", userId);
                return StatusCode(500, new { error = "Error updating avatar." });
            }

            return Ok();
        }

        [HttpGet("avatars")]
        public async Task<IActionResult> GetAvatars()
        {
            var avatars = await _repository.GetRecordsAsync<Avatar>("SELECT * FROM Avatars", null);
            if (avatars == null || !avatars.Any())
            {
                _logger.LogError("No avatars found in the database.");
                return NotFound(new { error = "No avatars found." });
            }
            else
            {
                foreach (var avatar in avatars)
                {
                    _logger.LogInformation($"Avatar found: {avatar.AvatarName}");
                }
            }
            return Ok(avatars.Select(a => new {
                a.AvatarId,
                a.AvatarName,
                a.CssStyle,
                a.StarsRequired
            }));
        }

        [HttpGet("currentAvatar")]
        public async Task<IActionResult> GetCurrentAvatar()
        {
            _logger.LogInformation("Attempting to get current avatar");

            int? userId = GetUserIdFromSession();
            if (userId == null)
            {
                _logger.LogError("Invalid user ID");
                return BadRequest(new { error = "Invalid user ID" });
            }

            var user = (await _repository.GetRecordsAsync<User>("SELECT * FROM Users WHERE UserId = @UserId", new { UserId = userId })).FirstOrDefault();

            if (user == null)
            {
                _logger.LogError("User not found. UserId: {UserId}", userId);
                return NotFound(new { error = "User not found." });
            }

            _logger.LogInformation($"Current avatar ID for user {userId} is {user.CurrentAvatar}");
            return Ok(user.CurrentAvatar != 0 ? user.CurrentAvatar : 1);
        }

        [HttpPost("updateProgress")]
        public async Task<IActionResult> UpdateProgress([FromBody] ProgressUpdateRequest request)
        {
            int? userId = GetUserIdFromSession();
            if (userId == null)
            {
                return BadRequest(new { error = "Invalid user ID" });
            }

            var userProgress = (await _repository.GetRecordsAsync<Progress>("SELECT * FROM Progress WHERE UserId = @UserId", new { UserId = userId })).FirstOrDefault();

            if (userProgress == null)
            {
                return NotFound(new { error = "User progress not found" });
            }

            int currentLevelStars = userProgress.GetType().GetProperty($"Level{request.Level}")?.GetValue(userProgress) as int? ?? 0;

            if (request.Stars > currentLevelStars)
            {
                await _repository.SaveDataAsync($"UPDATE Progress SET Level{request.Level} = @Stars WHERE UserId = @UserId", new { Stars = request.Stars, UserId = userId });
                await _repository.SaveDataAsync("UPDATE Users SET Stars = Stars + @StarDifference WHERE UserId = @UserId", new { StarDifference = request.Stars - currentLevelStars, UserId = userId });
            }

            return Ok();
        }

        private int? GetUserIdFromSession()
        {
            _logger.LogInformation("Attempting to get user ID from session.");
            return HttpContext.Session.GetInt32("userId");
        }

        public class AvatarUpdateRequest
        {
            public int AvatarId { get; set; }
        }

        public class ProgressUpdateRequest
        {
            public int Level { get; set; }
            public int Stars { get; set; }
        }
    }
}
