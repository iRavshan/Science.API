using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;

namespace Science.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRoleController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> roleManager;

        public UserRoleController(RoleManager<IdentityRole> roleManager)
        {
            this.roleManager = roleManager;
        }

        [HttpGet]
        [Route("getAllRoles")]
        public async Task<IActionResult> GetAllRoles()
        {
            List<IdentityRole> roles = await roleManager.Roles.ToListAsync();
            return Ok(roles);
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateRole(string name)
        {
            bool role_exist = await roleManager.RoleExistsAsync(name);

            if (role_exist)
            {
                return BadRequest(new { error = "Role already exist" });
            }

            var roleResult = await roleManager.CreateAsync(new IdentityRole(name));

            if (roleResult.Succeeded)
            {
                return Ok(new { result = $"The role {name} has been added successfully" });
            }

            return BadRequest(new { error = "Server error" });
        }

        [HttpDelete]
        [Route("delete")]
        public async Task<IActionResult> DeleteRole(string name)
        {
            bool role_exist = await roleManager.RoleExistsAsync(name);

            if (!role_exist)
            {
                return NotFound(new { error = "Role not found" });
            }

            var action_result = await roleManager.DeleteAsync(new IdentityRole(name));

            if (action_result.Succeeded)
            {
                return Ok(new { result = $"The role {name} has been deleted successfully" });
            }

            return BadRequest(new { error = "Server error" });
        }

        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> UpdateRoleName(string oldName, string newName)
        {
            bool role_exist = await roleManager.RoleExistsAsync(oldName);

            if (!role_exist)
            {
                return NotFound(new { error = $"The role {oldName} not found" });
            }

            //var action_result = await roleManager.UpdateAsync(new IdentityRole(name));

            //if (action_result.Succeeded)
            //{
            //    return Ok(new { result = $"The role {name} has been deleted successfully" });
            //}

            return BadRequest(new { error = "Server error" });
        }
    }
}
