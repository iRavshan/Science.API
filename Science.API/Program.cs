using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyCSharp.HttpUserAgentParser.AspNetCore.DependencyInjection;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using Science.Data.Contexts;
using Science.Data.Interfaces;
using Science.Data.Repositories;
using Science.Domain.Models;
using Science.Service.Interfaces;
using Science.Service.Services;
using Science.Utility.Configurations;
using Science.Utility.MappingProfiles;
using Science.Utility.Middlewares;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DatabaseConnection")));

builder.Services.AddAutoMapper(config =>
{
    config.AddProfile<UserProfile>();
    config.AddProfile<BookProfile>();
});


builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));

byte[] JwtSecretKey = Encoding.ASCII.GetBytes(builder.Configuration["JwtConfig:Secret"]); 

TokenValidationParameters tokenValidationParameters = new ()
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(JwtSecretKey),
    ValidateIssuer = false,
    ValidateAudience = false,
    ValidateLifetime = true,
    RequireExpirationTime = false
};

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(jwt => {
    jwt.SaveToken = true;
    jwt.TokenValidationParameters = tokenValidationParameters;
});

builder.Services.AddSingleton(tokenValidationParameters);


builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;

    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(1);

}).AddEntityFrameworkStores<AppDbContext>();


builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IUserAgentRepository, UserAgentRepository>();


builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IUserAgentService, UserAgentService>();
builder.Services.AddScoped<ICorrelationIdGenerator, CorrelationIdGenerator>();


builder.Services.AddHttpUserAgentCachedParser().AddHttpUserAgentParserAccessor();


string MyAllowSpecificOrigins = "myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      builder =>
                      {
                          builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                      });
});


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/dailyLog-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors(MyAllowSpecificOrigins);

app.MapControllers();

app.Run();
