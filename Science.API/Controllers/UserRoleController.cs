using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateRole(string name)
        {
            bool role_exist = await roleManager.RoleExistsAsync(name.ToLower());

            if (role_exist)
            {
                return BadRequest(new { error = "Role already exist" });
            }

            var roleResult = await roleManager.CreateAsync(new IdentityRole(name.ToLower()));

            if (roleResult.Succeeded)
            {
                return Ok(new { result = $"The role {name} has been added successfully" });
            }

            return BadRequest(new { error = "Server error" });
        }

        [HttpGet]
        [Route("getAll")]
        public async Task<IActionResult> GetAllRoles()
        {
            List<IdentityRole> roles = await roleManager.Roles.ToListAsync();
            return Ok(roles);
        }

        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> UpdateRoleName(string oldName, string newName)
        {
            IdentityRole? role = await roleManager.FindByNameAsync(oldName.ToLower());

            if (role == null)
            {
                return NotFound(new { error = $"The role {oldName} not found" });
            }

            if(oldName.ToLower().Equals(newName.ToLower()))
            {
                return BadRequest(new { error = "The new name is the same with old name" });
            }
            
            role.Name = newName.ToLower();
            role.NormalizedName = newName.ToUpper();

            var action_result = await roleManager.UpdateAsync(role);

            if (action_result.Succeeded)
            {
                return Ok(new { result = $"The role {oldName} has been updated successfully" });
            }

            return BadRequest(new { error = "Server error" });
        }

        [HttpDelete]
        [Route("delete")]
        public async Task<IActionResult> DeleteRole(string name)
        {
            IdentityRole? role = await roleManager.FindByNameAsync(name.ToLower());

            if (role == null)
            {
                return NotFound(new { error = "The role not found" });
            }

            var action_result = await roleManager.DeleteAsync(role);

            if (action_result.Succeeded)
            {
                return Ok(new { result = $"The role {name} has been deleted successfully" });
            }

            return BadRequest(new { error = "Server error" });
        }
    }
}
