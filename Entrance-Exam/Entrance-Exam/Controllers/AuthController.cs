using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EntranceExam.Services.Model;
using EntranceExam.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EntranceExam.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IBlacklistTokenService _blacklistTokenService;
        private readonly ITokenService _tokenService;
        public AuthController(IAuthService authService, IBlacklistTokenService blacklistTokenService, ITokenService tokenService)
        {
            _authService = authService;
            _blacklistTokenService = blacklistTokenService;
            _tokenService = tokenService;
        }

        [HttpGet]
        [Route("user-information")]
        [Authorize]
        public async Task<IActionResult> GetUserInformation() {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized();
                var userId = int.Parse(userIdClaim.Value);
                var user = await _authService.GetUserByIdAsync(userId);
                if (user == null)
                    return NotFound("User not found");
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Route("sign-up")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Invalid request");
                }

                var response = await _authService.SignUpAsync(request);
                if (response == null)
                {
                    return BadRequest("Sign-up failed");
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Route("sign-in")]
        public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Invalid request");
                }
                var response = await _authService.SignInAsync(request);
                if (response == null)
                {
                    return BadRequest("Sign-in failed");
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("sign-out")]
        [Authorize]
        public async Task<IActionResult> SignOut()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized();

                var userId = int.Parse(userIdClaim.Value);

                await _tokenService.RemoveRefreshTokensAsync(userId);

                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var expires = jwtToken.ValidTo;

                await _blacklistTokenService.AddToBlacklistAsync(token, expires);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var result = await _tokenService.RefreshTokenAsync(request.RefreshToken);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Refresh token not found.");
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
