using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using MyCSharp.HttpUserAgentParser;
using MyCSharp.HttpUserAgentParser.AspNetCore;
using RestSharp;
using RestSharp.Authenticators;
using Science.Data.Contexts;
using Science.Domain.Models;
using Science.DTO.Auth.Requests;
using Science.DTO.Auth.Responses;
using Science.DTO.User.Requests;
using Science.Service.Interfaces;
using Serilog;
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
        private readonly IConfiguration configuration;
        private readonly IUserAgentService userAgentService;
        private readonly IHttpUserAgentParserAccessor httpUserAgentParserAccessor;
        private readonly ICorrelationIdGenerator correlationIdGenerator;
        private readonly IMapper mapper;

        public AuthController(UserManager<User> userManager,
                              RoleManager<IdentityRole> roleManager,
                              TokenValidationParameters validationParameters,
                              AppDbContext dbContext,
                              IConfiguration configuration,
                              IUserAgentService userAgentService,
                              IHttpUserAgentParserAccessor httpUserAgentParserAccessor,
                              ICorrelationIdGenerator correlationIdGenerator,
                              IMapper mapper)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.configuration = configuration;
            this.userAgentService = userAgentService;
            this.httpUserAgentParserAccessor = httpUserAgentParserAccessor;
            this.correlationIdGenerator = correlationIdGenerator;
            this.mapper = mapper;
            this.validationParameters = validationParameters;
            this.dbContext = dbContext;
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

                var username_exist = await userManager.FindByNameAsync(request.UserName);

                if (username_exist != null)
                {
                    return BadRequest(new AuthResult()
                    {
                        Error = "Username has already registered",
                        Result = false
                    });
                }

                var new_user = mapper.Map<User>(request);

                var is_created = await userManager.CreateAsync(new_user, request.Password);

                if (is_created.Succeeded)
                {
                    //User-Agent REGISTRATION
                    string userAgent = Request.Headers[HeaderNames.UserAgent].ToString();

                    UserAgent newUserAgent = UserAgentParser(userAgent, new_user.Id);

                    await userAgentService.CreateAsync(newUserAgent);

                    await userAgentService.SaveChangesAsync();


                    //EMAIL CONFIRMATION
                    string code = await userManager.GenerateEmailConfirmationTokenAsync(new_user);

                    string email_body = "Please confirm your email address <a href=\"#URL#\">Click here</a>";

                    string callback_url = Request.Scheme + "://" + Request.Host + Url.Action("ConfirmEmail", "Auth", 
                                       new { userId = new_user.Id, code = code });

                    string body = email_body.Replace("#URL#", System.Text.Encodings.Web.HtmlEncoder.Default.Encode(callback_url));

                    // SEND EMAIL
                    //SendEmail(body, new_user.Email);


                    //GENERATE JWT 
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
            Log.Information("CorrelationId {correlationId}: User login request", correlationIdGenerator.Get());

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

                if (user_exist.LockoutEnd > DateTimeOffset.Now)
                {
                    return BadRequest(new AuthResult()
                    {
                        Result = false,
                        Error = $"Your account has been blocked until {user_exist.LockoutEnd}"
                    });
                }

                bool is_correct = await userManager.CheckPasswordAsync(user_exist, request.Password);

                if(is_correct) 
                {
                    if (user_exist.AccessFailedCount > 0)
                    {
                        await userManager.ResetAccessFailedCountAsync(user_exist);
                    }

                    if(user_exist.LockoutEnd != null)
                    {
                        user_exist.LockoutEnd = null;

                        await userManager.UpdateAsync(user_exist);
                    }

                    AuthResult result = await GenerateJwtToken(user_exist);

                    return Ok(result);
                }
                else
                {
                    await userManager.AccessFailedAsync(user_exist);

                    return BadRequest(new AuthResult()
                    {
                        Result = false,
                        Error = "Email or password is incorrect"
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

            byte[] key = Encoding.UTF8.GetBytes(configuration.GetSection("JwtConfig:Secret").Value);

            var claims = await GetAllValidClaims(user);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
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
                RefreshToken = refreshToken.Token,
                Result = true
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

        private async Task<List<Claim>> GetAllValidClaims(User user)
        {
            IdentityOptions options = new IdentityOptions();

            List<Claim> claims = new ()
            {
                new Claim("Id", user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString())
            };

            // Getting the claims that we have assigned to the user
            IList<Claim> userClaims = await userManager.GetClaimsAsync(user);
            claims.AddRange(userClaims);

            // Get the user role and add it to the claims
            IList<string> userRoles = await userManager.GetRolesAsync(user);

            foreach(string userRole in userRoles)
            {
                IdentityRole? role = await roleManager.FindByNameAsync(userRole);

                if(role != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, userRole));

                    IList<Claim> roleClaims = await roleManager.GetClaimsAsync(role);

                    foreach(Claim roleClaim in roleClaims)
                    {
                        claims.Add(roleClaim);
                    }
                }
            }

            return claims;
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

        private UserAgent UserAgentParser(string userAgent, string userId)
        {
            HttpUserAgentInformation userAgentInfo = HttpUserAgentParser.Parse(userAgent);

            UserAgent newUserAgent = new UserAgent()
            {
                UserId = userId,
                Type = userAgentInfo.Type,
                PlatformName = userAgentInfo.Platform.GetValueOrDefault().Name,
                PlatformType = userAgentInfo.Platform.GetValueOrDefault().PlatformType,
                Name = userAgentInfo.Name,
                Version = userAgentInfo.Version,
                MobileDeviceType = userAgentInfo.MobileDeviceType,
                Added_At = DateTime.Now,
            };

            return newUserAgent;
        }
    }
}
