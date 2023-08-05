using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Science.Data.Contexts;
using Science.Domain.Models;
using Science.DTO.Auth.Requests;
using Science.DTO.Auth.Responses;
using Science.DTO.User.Requests;
using Science.Service.Interfaces;
using Science.Service.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Science.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly TokenValidationParameters validationParameters;
        private readonly AppDbContext dbContext;
        private readonly ILogger<AuthController> logger;
        private readonly IConfiguration configuration;
        private readonly ICorrelationIdGenerator correlationIdGenerator;
        private readonly IMapper mapper;

        public AuthController(UserManager<User> userManager,
                              RoleManager<IdentityRole> roleManager,
                              TokenValidationParameters validationParameters,
                              AppDbContext dbContext,
                              IConfiguration configuration,
                              ICorrelationIdGenerator correlationIdGenerator,
                              ILogger<AuthController> logger,
                              IMapper mapper)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.configuration = configuration;
            this.correlationIdGenerator = correlationIdGenerator;
            this.mapper = mapper;
            this.validationParameters = validationParameters;
            this.dbContext = dbContext;
            this.logger = logger;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request)
        {
            if(ModelState.IsValid)
            {
                var user_exist = await userManager.FindByEmailAsync(request.Email);

                if (user_exist != null)
                {
                    return BadRequest(new AuthResult()
                    {
                        Error = "User has already registered",
                        Result = false
                    });
                }

                var new_user = mapper.Map<User>(request);

                var is_created = await userManager.CreateAsync(new_user, request.Password);

                if (is_created.Succeeded)
                {
                    AuthResult result = await GenerateJwtToken(new_user);

                    return Ok(result);
                }

                return BadRequest(new AuthResult()
                {
                    Error = "Server error",
                    Result = false
                });
            }

            return BadRequest(new AuthResult()
            {
                Error = "Inputs are not valid",
                Result = false
            });
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            logger.LogInformation("CorrelationId {correlationId}: User login request", correlationIdGenerator.Get());

            if (ModelState.IsValid)
            {
                var user_exist = await userManager.FindByEmailAsync(request.Email);

                if (user_exist == null)
                {
                    return BadRequest(new AuthResult()
                    {
                        Error = "User has not registered yet",
                        Result = false
                    });
                }

                bool is_correct = await userManager.CheckPasswordAsync(user_exist, request.Password);

                if (is_correct)
                {
                    AuthResult result = await GenerateJwtToken(user_exist);

                    return Ok(result);
                }
                else
                {
                    return BadRequest(new AuthResult()
                    {
                        Result = false,
                        Token = "Email or password incorrect"
                    });
                }
            }

            return BadRequest(new AuthResult()
            {
                Error = "Inputs are not valid",
                Result = false
            });
        }

        [HttpPost]
        [Route("addUserToRole")]
        public async Task<IActionResult> AddUserToRole(string email, string roleName)
        {
            var user = await userManager.FindByEmailAsync(email);

            if(user == null)
            {
                return BadRequest(new { error = "User does not exist" });
            }

            bool roleExist = await roleManager.RoleExistsAsync(roleName);

            if(!roleExist)
            {
                return BadRequest(new { error = $"The role {roleName} does not exist" });
            }

            bool IsInRole = await userManager.IsInRoleAsync(user, roleName);

            if(IsInRole)
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
        [Route("getUserRoles")]
        public async Task<IActionResult> GetUserRoles(string email)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return BadRequest(new { error = "User does not exist" });
            }

            var roles = await userManager.GetRolesAsync(user);

            return Ok(roles);
        }

        [HttpPost]
        [Route("removeUserFromRole")]
        public async Task<IActionResult> RemoveUserFromRole(string email, string roleName)
        {
            var user = await userManager.FindByEmailAsync(email);

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
                return Ok(new { result = $"User has not added to the role {roleName} yet" });
            }

            var actionResult = await userManager.RemoveFromRoleAsync(user, roleName);

            if (actionResult.Succeeded)
            {
                return Ok(new { result = "User has been removed from role successfully" });
            }

            return BadRequest(new { error = "Server error" });
        }

        [HttpPost]
        [Route("refreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest request)
        {
            if(ModelState.IsValid)
            {
                var result = VerifyAndGenereteToken(request);

                if(result == null)
                    return BadRequest(new AuthResult()
                    {
                        Error = "Invalid tokens",
                        Result = false
                    });

                return Ok(result);
            }

            return BadRequest(new AuthResult()
            {
                Error = "Invalid parameters",
                Result = false
            });
        }

        private async Task<AuthResult> GenerateJwtToken(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.UTF8.GetBytes(configuration.GetSection("JwtConfig:Secret").Value);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString())
                }),

                Expires = DateTime.UtcNow.Add(TimeSpan.Parse(configuration.GetSection("JwtConfig:ExpiryTimeFrame").Value)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            RefreshToken refreshToken = new()
            {
                JwtId = token.Id,
                Token = RandomStringGenerate(23),
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
                IsRevoked = false,
                IsUsed = false,
                UserId = user.Id,
            };

            await dbContext.RefreshTokens.AddAsync(refreshToken);

            await dbContext.SaveChangesAsync();

            return new AuthResult()
            {
                Token = jwtToken,
                RefreshToken = refreshToken.Token
            };
        }

        private async Task<AuthResult> VerifyAndGenereteToken(TokenRequest request)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            try
            {
                validationParameters.ValidateLifetime = false;

                var tokenVerification = jwtTokenHandler.ValidateToken(request.Token, validationParameters, out var validatedToken);
            
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
                    
                    if(result == false)
                    {
                        return null;
                    }
                }

                long utcExpiryDate = long.Parse(tokenVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                DateTime expiryDateTime = UnixTimeStampToDateTime(utcExpiryDate);

                if(expiryDateTime > DateTime.Now)
                    return new AuthResult()
                    {
                        Result = false,
                        Error = "Expired token"
                    };

                var storedToken = await dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == request.RefreshToken);

                if(storedToken == null)
                    return new AuthResult()
                    {
                        Result = false,
                        Error = "Invalid tokens"
                    };

                if (storedToken.IsUsed)
                    return new AuthResult()
                    {
                        Result = false,
                        Error = "Invalid tokens"
                    };

                if (storedToken.IsRevoked)
                    return new AuthResult()
                    {
                        Result = false,
                        Error = "Invalid tokens"
                    };

                var jti = tokenVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

                if(storedToken.JwtId != jti)
                    return new AuthResult()
                    {
                        Result = false,
                        Error = "Invalid tokens"
                    };

                if(storedToken.ExpiryDate < DateTime.UtcNow)
                    return new AuthResult()
                    {
                        Result = false,
                        Error = "Expired tokens"
                    };

                storedToken.IsUsed = true;

                dbContext.RefreshTokens.Update(storedToken);

                await dbContext.SaveChangesAsync();

                User dbUser = await userManager.FindByIdAsync(storedToken.UserId);

                return await GenerateJwtToken(dbUser);
            }

            catch (Exception ex) 
            {
                return new AuthResult()
                {
                    Result = false,
                    Error = "Server error"
                };
            }
        }

        private DateTime UnixTimeStampToDateTime(long utcExpiryDate)
        {
            DateTime dateTimeVal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            dateTimeVal.AddSeconds(utcExpiryDate).ToUniversalTime();  

            return dateTimeVal;
        }

        private string RandomStringGenerate(int length)
        {
            Random random = new Random();

            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz_";

            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}
