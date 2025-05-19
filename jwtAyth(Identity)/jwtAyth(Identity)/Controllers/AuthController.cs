using jwtAuth_Identity_.Entities;
using jwtAuth_Identity_.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static jwtAuth_Identity_.Model.AuthDto;

namespace jwtAuth_Identity_.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole<long>> _roleManager;
        private readonly JWTService _jwtService;
        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole<long>> roleManager, JWTService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDTO model)
        {
            var existingPhoneNumber = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == model.PhoneNumber);
            if (existingPhoneNumber != null)
            {
                return BadRequest(new AuthResponseDto { Success = false, Message = "Phone number already exists" });
            }

            var adminRole = new IdentityRole<long>()
            {
                Name = "User",
            };
            await _roleManager.CreateAsync(adminRole);

            var user = new ApplicationUser()
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, adminRole.Name);
                var token = await _jwtService.GenerateToken(user);
                return Ok(new AuthResponseDto
                {
                    Success = true,
                    Message = "User registered successfully",
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber
                    }
                });
            }
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));

            return BadRequest(new AuthResponseDto { Success = false, Message = errors });
        }

        [HttpPost("Login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized(new AuthResponseDto { Success = false, Message = "همچین ایمیلی وجود نداره" });
            }
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (result.Succeeded)
            {
                var token = await _jwtService.GenerateToken(user);
                return Ok(new AuthResponseDto
                {
                    Success = true,
                    Message = ".تبریک با موفقیت وارد شدید",
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber
                    }
                });
            }

            return BadRequest(new AuthResponseDto { Success = false, Message = "رمز عبور صحیح نمیباشد" });
        }
        [Authorize]
        [HttpPost("Logout")]
        public async Task<ActionResult<AuthResponseDto>> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new AuthResponseDto { Success = true, Message = "Logout successful" });
        }

        [Authorize]
        [HttpGet("Showprofile")]
        public async Task<ActionResult<UserDto>> GetProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound(new AuthResponseDto { Success = false, Message = "User not found" });
            }
            return Ok(new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber
            });
        }

        [Authorize]
        [HttpPut("ChangePassword")]
        public async Task<ActionResult<AuthResponseDto>>ChangePassword(ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "اطلاعات وارد شده معتبر نیست."
                });
            }

            if (model.CurrentPassword == model.NewPassword)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "رمز عبور جدید نباید با رمز عبور فعلی یکسان باشد."
                });
            }

            var user = await _userManager.GetUserAsync(User);
            if(user == null)
            {
                return NotFound(new AuthResponseDto { Success = false, Message = "User not found" });
            }
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            
            if (result.Succeeded)
            {
                return Ok(new AuthResponseDto { Success = true, Message = "Password changed successfully" });
            }
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return BadRequest(new AuthResponseDto { Success = false, Message = errors });
        }

        [Authorize]
        [HttpPut("UpdateProfile")]
        public async Task<ActionResult<AuthResponseDto>> UpdateProfile(UserDto model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound(new AuthResponseDto { Success = false, Message = "User not found" });
            }
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Ok(new AuthResponseDto
                {
                    Success = true,
                    Message = "Profile updated successfully",
                    User = new UserDto
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber
                    }
                });
            }
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return BadRequest(new AuthResponseDto { Success = false, Message = errors });
        }

        [Authorize]
        [HttpDelete("DeleteAccount")]
        public async Task<ActionResult<AuthResponseDto>> DeleteAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound(new AuthResponseDto { Success = false, Message = "User not found" });
            }
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Ok(new AuthResponseDto { Success = true, Message = "Account deleted successfully" });
            }
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return BadRequest(new AuthResponseDto { Success = false, Message = errors });
        }


    }
}

