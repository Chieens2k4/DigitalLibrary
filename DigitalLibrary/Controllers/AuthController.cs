using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalLibrary.Data;
using DigitalLibrary.DTOs;
using DigitalLibrary.Models;
using DigitalLibrary.Services;

namespace DigitalLibrary.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DigitalLibraryContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthController(
            DigitalLibraryContext context,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Register(RegisterDto registerDto)
        {
            try
            {
                // Kiểm tra email đã tồn tại
                if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                {
                    return BadRequest(new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Email đã được sử dụng"
                    });
                }

                // Tạo user mới với role Student (RoleId = 3)
                var user = new User
                {
                    Email = registerDto.Email,
                    PasswordHash = _passwordHasher.HashPassword(registerDto.Password),
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    DateOfBirth = registerDto.DateOfBirth,
                    Gender = registerDto.Gender,
                    RoleId = 3, // Student
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Load role
                await _context.Entry(user).Reference(u => u.Role).LoadAsync();

                // Tạo token
                var token = _jwtTokenService.GenerateToken(user);

                var response = new LoginResponseDto
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    RoleName = user.Role?.RoleName ?? "Student",
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
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

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

                if (!_passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    return Unauthorized(new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Email hoặc mật khẩu không chính xác"
                    });
                }

                var token = _jwtTokenService.GenerateToken(user);

                var response = new LoginResponseDto
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    RoleName = user.Role?.RoleName ?? "",
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
                var exists = await _context.Users.AnyAsync(u => u.Email == email);

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