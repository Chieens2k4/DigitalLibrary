using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DigitalLibrary.DTOs;
using DigitalLibrary.Models;
using DigitalLibrary.Services;

namespace DigitalLibrary.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtTokenService jwtTokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Register(RegisterDto registerDto)
        {
            try
            {
                // Kiểm tra email đã tồn tại
                var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    return BadRequest(new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Email đã được sử dụng"
                    });
                }

                // Tạo user mới với role Student (RoleId = 4)
                var user = new ApplicationUser
                {
                    UserName = registerDto.Email,
                    Email = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    DateOfBirth = registerDto.DateOfBirth,
                    Gender = registerDto.Gender,
                    CreatedAt = DateTime.Now,
                    IsActive = true,
                    EmailConfirmed = true // Auto confirm for now
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (!result.Succeeded)
                {
                    return BadRequest(new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = string.Join(", ", result.Errors.Select(e => e.Description))
                    });
                }

                // Assign Student role
                await _userManager.AddToRoleAsync(user, "Student");

                // Tạo token
                var token = await _jwtTokenService.GenerateTokenAsync(user);

                var roles = await _userManager.GetRolesAsync(user);

                var response = new LoginResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email ?? "",
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    RoleName = roles.FirstOrDefault() ?? "Student",
                    Token = token
                };

                return Ok(new ApiResponse<LoginResponseDto>
                {
                    Success = true,
                    Message = "Đăng ký thành công",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<LoginResponseDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login(LoginDto loginDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);

                if (user == null)
                {
                    return Unauthorized(new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Email hoặc mật khẩu không chính xác"
                    });
                }

                if (!user.IsActive)
                {
                    return Unauthorized(new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Tài khoản đã bị khóa"
                    });
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);

                if (!result.Succeeded)
                {
                    if (result.IsLockedOut)
                    {
                        return Unauthorized(new ApiResponse<LoginResponseDto>
                        {
                            Success = false,
                            Message = "Tài khoản đã bị khóa do đăng nhập sai quá nhiều lần"
                        });
                    }

                    return Unauthorized(new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Email hoặc mật khẩu không chính xác"
                    });
                }

                var token = await _jwtTokenService.GenerateTokenAsync(user);
                var roles = await _userManager.GetRolesAsync(user);

                var response = new LoginResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email ?? "",
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    RoleName = roles.FirstOrDefault() ?? "",
                    Token = token
                };

                return Ok(new ApiResponse<LoginResponseDto>
                {
                    Success = true,
                    Message = "Đăng nhập thành công",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<LoginResponseDto>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // POST: api/Auth/check-email
        [HttpPost("check-email")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckEmail([FromBody] string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                var exists = user != null;

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = exists ? "Email đã tồn tại" : "Email khả dụng",
                    Data = exists
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }
    }
}