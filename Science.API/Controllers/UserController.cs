using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Science.Domain.Models;
using Science.Service.Interfaces;
using System.Security.Claims;

namespace Science.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public UserController(IUserService userService,
                              UserManager<User> userManager,
                              RoleManager<IdentityRole> roleManager)
        {
            this.userService = userService;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        [HttpGet]
        [Route("getById")]
        public async Task<IActionResult> GetUser(string userId)
        {
            User? user = await userService.GetByIdAsync(userId);

            return Ok(user);
        }

        [HttpGet]
        [Route("getAll")]
        public async Task<IActionResult> GetAllUsers()
        {
            IEnumerable<User> users = await userService.GetAllUsersAsync();

            return Ok(users);
        }

        [HttpPost]
        [Route("addToRole")]
        public async Task<IActionResult> AddUserToRole(string userId, string roleName)
        {
            var user = await userService.GetByIdAsync(userId);

            if (user == null)
            {
                return BadRequest(new { error = "User does not exist" });
            }

            bool roleExist = await roleManager.RoleExistsAsync(roleName);

            if (!roleExist)
            {
                return BadRequest(new { error = $"The role {roleName} does not exist" });
            }

            bool IsInRole = await userManager.IsInRoleAsync(user, roleName);

            if (IsInRole)
            {
                return Ok(new { result = $"User has already added to the role {roleName}" });
            }

            var actionResult = await userManager.AddToRoleAsync(user, roleName);

            if (actionResult.Succeeded)
            {
                return Ok(new { result = "The user has been added to role successfully" });
            }

            return BadRequest(new { error = "Server error" });
        }

        [HttpGet]
        [Route("getRoles")]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            var user = await userService.GetByIdAsync(userId);

            if (user == null)
            {
                return BadRequest(new { error = "User does not exist" });
            }

            var roles = await userManager.GetRolesAsync(user);

            return Ok(roles);
        }

        [HttpGet]
        [Route("getClaims")]
        public async Task<IActionResult> GetUserClaims(string userId)
        {
            User user = await userService.GetByIdAsync(userId);

            if (user == null)
            {
                return BadRequest(new { error = "User does not exist" });
            }

            IList<Claim> userClaims = await userManager.GetClaimsAsync(user);

            return Ok(userClaims);
        }

        [HttpPost]
        [Route("addClaims")]
        public async Task<IActionResult> AddClaimsToUser(string userId, string claimName, string claimValue)
        {
            User user = await userService.GetByIdAsync(userId);

            if (user == null)
            {
                return BadRequest(new { error = "User does not exist" });
            }

            Claim userClaim = new Claim(claimName, claimValue);

            IdentityResult result = await userManager.AddClaimAsync(user, userClaim);

            if(result.Succeeded)
            {
                return Ok(new { result = $"User has a claim {claimName} added to them" });
            }

            return BadRequest(new { error = $"Unable to add claim {claimName} to the user " });
        }

        [HttpPost]
        [Route("removeFromRole")]
        public async Task<IActionResult> RemoveUserFromRole(string userId, string roleName)
        {
            var user = await userService.GetByIdAsync(userId);

            if (user == null)
            {
                return BadRequest(new { error = "User does not exist" });
            }

            bool roleExist = await roleManager.RoleExistsAsync(roleName);

            if (!roleExist)
            {
                return BadRequest(new { error = $"The role {roleName} does not exist" });
            }

            bool IsInRole = await userManager.IsInRoleAsync(user, roleName);

            if (!IsInRole)
            {
                return Ok(new { result = $"User has not added to the role {roleName} yet" });
            }

            var actionResult = await userManager.RemoveFromRoleAsync(user, roleName);

            if (actionResult.Succeeded)
            {
                return Ok(new { result = "User has been removed from the role successfully" });
            }

            return BadRequest(new { error = "Server error" });
        }
    }
}
