using GBastos.Hexagon_Skill_Test.Api.Data;
using GBastos.Hexagon_Skill_Test.Api.Messaging.Brokers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);


// ======================
// SQL SERVER
// ======================

var connectionString = builder.Configuration.GetConnectionString("SqlServer")
    ?? throw new ArgumentNullException("ConnectionStrings:SqlServer não encontrada.");

builder.Services.AddDbContext<UsuarioDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});


// ======================
// JWT
// ======================

var keyString = Environment.GetEnvironmentVariable("JWT_KEY")
    ?? throw new ArgumentNullException("JWT_KEY não encontrada.");

var key = Encoding.UTF8.GetBytes(keyString);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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


// ======================
// RABBITMQ
// ======================

builder.Services.AddSingleton<RabbitMQPublisher>();


// ======================
// SWAGGER
// ======================

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
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


// ======================
// CORS
// ======================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});


var app = builder.Build();


// ======================
// MIDDLEWARE
// ======================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();


// ======================
// ENDPOINTS
// ======================

app.MapUsuarioEndpoints(key);


// ======================
// MIGRATIONS
// ======================

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UsuarioDbContext>();
    db.Database.Migrate();
}


app.Run();