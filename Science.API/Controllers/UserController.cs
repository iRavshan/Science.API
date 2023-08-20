using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Science.Domain.Models;
using Science.Service.Interfaces;

namespace Science.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;

        public UserController(IUserService userService)
        {
            this.userService = userService;
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
    }
}
