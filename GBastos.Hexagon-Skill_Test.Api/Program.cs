using GBastos.Hexagon_Skill_Test.Api.Data;
using GBastos.Hexagon_Skill_Test.Api.Messaging.Brokers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var provider = builder.Configuration["DatabaseProvider"];
builder.Services.AddDbContext<UsuarioDbContext>(options =>
{
    if (provider == "SqlServer")
    {
        var connectionString = builder.Configuration.GetConnectionString("SqlServer")
            ?? throw new ArgumentNullException("ConnectionStrings:SqlServer não encontrada.");
        var password = Environment.GetEnvironmentVariable("SA_PASSWORD")
            ?? throw new Exception("SA_PASSWORD não definida.");
        connectionString = connectionString.Replace("{PASSWORD}", password);
        options.UseSqlServer(connectionString);
    }
    else
    {
        var connectionString = builder.Configuration.GetConnectionString("Sqlite")
            ?? throw new ArgumentNullException("ConnectionStrings:Sqlite não encontrada.");
        options.UseSqlite(connectionString);
    }
});

var keyString = Environment.GetEnvironmentVariable("JWT_KEY")
    ?? builder.Configuration["Jwt:Key"]
    ?? throw new ArgumentNullException("JWT Key não encontrada.");
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

builder.Services.AddSingleton<RabbitMQPublisher>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Digite: Bearer {seu token}"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

DotNetEnv.Env.Load();

var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")!;
var saPassword = Environment.GetEnvironmentVariable("SA_PASSWORD");
var sqlitePath = Environment.GetEnvironmentVariable("SQLITE_PATH");
var rabbitHost = Environment.GetEnvironmentVariable("RabbitMQ__HostName");
var rabbitQueue = Environment.GetEnvironmentVariable("RabbitMQ__QueueName");

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

var publisher = app.Services.GetRequiredService<RabbitMQPublisher>();
app.MapUsuarioEndpoints(publisher);

app.MapPost("/login", (UserLogin user) =>
{
    if (user.Username != "Hexagon" || user.Password != "senhaHexagon")
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