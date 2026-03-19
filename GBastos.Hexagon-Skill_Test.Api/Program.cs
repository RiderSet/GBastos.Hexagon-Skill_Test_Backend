using GBastos.Hexagon_Skill_Test.Api.Data;
using GBastos.Hexagon_Skill_Test.Api.Endpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Conn")
    ?? throw new ArgumentNullException("ConnectionString 'Conn' desconhecida.");

var provider = builder.Configuration["DatabaseProvider"];

builder.Services.AddDbContext<UsuarioDbContext>(options =>
{
    if (provider == "Sqlite")
        options.UseSqlite(builder.Configuration.GetConnectionString("Conn"));
    else
        options.UseSqlServer(builder.Configuration.GetConnectionString("Conn"));
});

var keyString = builder.Configuration["Jwt:Key"]
    ?? throw new ArgumentNullException("Jwt:Key não encontrada.");
var key = Encoding.ASCII.GetBytes(keyString);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();

app.MapUsuarioEndpoints();

app.MapPost("/login", (UserLogin user) =>
{
    if (user.Username != "GBastos" || user.Password != "123456")
        return Results.Unauthorized();

    var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new System.Security.Claims.ClaimsIdentity(new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.Username)
        }),
        Expires = DateTime.UtcNow.AddHours(1),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return Results.Ok(new { token = tokenHandler.WriteToken(token) });
});

app.Run();

record UserLogin(string Username, string Password);