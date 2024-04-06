using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using API.Dtos.Account;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtService _jwtService;

        public AccountController(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IJwtService jwtService
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new AppUser
            {
                Email = registerDto.Email,
                FullName = registerDto.FullName,
                UserName = registerDto.Email
            };
            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            if (registerDto.Roles is null)
            {
                await _userManager.AddToRoleAsync(user, "User");
            }
            else
            {
                foreach (var role in registerDto.Roles)
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }

            return Ok(
                new AuthResponseDto { IsSuccess = true, Message = "Account Created Successfully" }
            );
        }
        
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user is null)
            {
                return Unauthorized(
                    new AuthResponseDto
                    {
                        IsSuccess = false,
                        Message = "User not found wit this email"
                    }
                );
            }
            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!result)
                return Unauthorized(
                    new AuthResponseDto { IsSuccess = false, Message = "Invalid Password" }
                );

            var token = _jwtService.GenerateToken(user);

            return Ok(
                new AuthResponseDto
                {
                    Token = token,
                    IsSuccess = true,
                    Message = "Login Success"
                }
            );
        }


        [HttpGet("detail")]
        public async Task<ActionResult<UserDetailDto>> GetUserDetail()
        {
            // Obtener el ID del usuario actualmente autenticado
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Buscar el usuario en la base de datos usando el UserManager
            var user = await _userManager.FindByIdAsync(currentUserId!);

            // Verificar si el usuario existe
            if (user is null)
            {
                // Si el usuario no se encuentra, devolver un 404 (Not Found) con un mensaje
                return NotFound(
                    new AuthResponseDto { IsSuccess = false, Message = "User Not Found" }
                );
            }

            // Si el usuario se encuentra, devolver sus detalles en un objeto UserDetailDto
            return Ok(
                new UserDetailDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Roles = [.. await _userManager.GetRolesAsync(user)],
                    PhoneNumber = user.PhoneNumber,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    AccessFailedCount = user.AccessFailedCount
                }
            );
        }

        [HttpGet()]
        public async Task<ActionResult<IEnumerable<UserDetailDto>>> GetUsers()
        {
            var users = await _userManager
                .Users.Select(u => new UserDetailDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FullName,
                    Roles = _userManager.GetRolesAsync(u).Result.ToArray()
                })
                .ToListAsync();
            return Ok(users);
        }
    }
}
