using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Dtos.Role;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;

        public RolesController(
            RoleManager<IdentityRole> roleManager,
            UserManager<AppUser> userManager
        )
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<ActionResult> CreateRole(CreateRoleDto createRoleDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var roleExist = await _roleManager.RoleExistsAsync(createRoleDto.RoleName);

            if (roleExist)
                return BadRequest("Role already exist");

            var roleResult = await _roleManager.CreateAsync(
                new IdentityRole(createRoleDto.RoleName)
            );

            if (!roleResult.Succeeded)
                return BadRequest("Role Creation Failed");

            return Ok(new { message = "Role Created Successfully" });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleResponseDto>>> GetRoles()
        {
            var roles = await _roleManager
                .Roles.Select(r => new RoleResponseDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    TotalUser = _userManager.GetUsersInRoleAsync(r.Name!).Result.Count
                })
                .ToListAsync();
            return Ok(roles);
        }

        [HttpDelete("id")]
        public async Task<ActionResult> DeleteRike(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role is null)
            {
                return NotFound("Role not found");
            }

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest("Role deletion failed");
            }
            return Ok(new { message = "Role Deleted Successfully" });
        }

        [HttpPost("assign")]
        public async Task<ActionResult> AssignRole(RoleAssignDto roleAssignDto)
        {
            var user = await _userManager.FindByIdAsync(roleAssignDto.UserId);

            if (user is null)
                return NotFound("User not found");

            var role = await _roleManager.FindByIdAsync(roleAssignDto.RoleId);

            if (role is null)
                return NotFound("Role not found");

            var result = await _userManager.AddToRoleAsync(user, role.Name!);

            if (!result.Succeeded)
            {
                var error = result.Errors.FirstOrDefault();
                return BadRequest(error!.Description);
            }

            return Ok(new { essage = "Role Assigned Successfully" });
        }
    }
}
