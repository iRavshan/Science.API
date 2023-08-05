using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Science.Data.Contexts;
using Science.Domain.Models;
using Science.Service.Interfaces;
using Science.Service.Services;
using Science.Utility.Configurations;
using Science.Utility.MappingProfiles;
using Science.Utility.Middlewares;
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


builder.Services.AddIdentity<User, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddScoped<ICorrelationIdGenerator, CorrelationIdGenerator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
