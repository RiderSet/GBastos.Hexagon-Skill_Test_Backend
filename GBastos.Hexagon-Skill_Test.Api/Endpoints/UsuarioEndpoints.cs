using GBastos.Hexagon_Skill_Test.Api.Data;
using GBastos.Hexagon_Skill_Test.Api.Endpoints;
using GBastos.Hexagon_Skill_Test.Api.Interfaces;
using GBastos.Hexagon_Skill_Test.Api.Messaging.Brokers;
using GBastos.Hexagon_Skill_Test.Api.Messaging.Outbox;
using GBastos.Hexagon_Skill_Test.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

public static class UsuarioEndpoints
{
    public static void MapUsuarioEndpoints(this WebApplication app, byte[] jwtKey)
    {
        var group = app.MapGroup("/api/auth");

        // Registro
        group.MapPost("/register", async (UserLogin user, UsuarioDbContext db) =>
        {
            var existente = await db.Usuarios
                .FirstOrDefaultAsync(u => u.Username == user.Username);

            var cpfJaExiste = await db.Usuarios
                .AnyAsync(u => u.CPF == user.CPF);

            if (cpfJaExiste)
            {
                return Results.Conflict(new
                {
                    message = "Já existe um usuário cadastrado com este CPF.",
                    field = "cpf",
                    error = "duplicate_cpf"
                });
            }

            var hasher = new PasswordHasher<Usuario>();

            if (existente != null)
            {
                existente.PasswordHash = hasher.HashPassword(existente, user.Password);
            }
            else
            {
                var novoUsuario = new Usuario
                {
                    Id = Guid.NewGuid(),
                    Username = user.Username,
                    Nome = user.Username,
                    Cidade = "Rio de Janeiro",
                    Estado = "RJ",
                    EstadoCivil = "Solteiro",
                    Idade = 56,
                    CPF = user.CPF
                };

                novoUsuario.PasswordHash = hasher.HashPassword(novoUsuario, user.Password);

                db.Usuarios.Add(novoUsuario);
            }

            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                message = "Usuário registrado com sucesso!"
            });
        });

        // Login
        group.MapPost("/login", async (UserLogin user, UsuarioDbContext db) =>
        {
            var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Username == user.Username);
            if (usuario == null) return Results.Unauthorized();

            var hasher = new PasswordHasher<Usuario>();

            if (hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, user.Password)
                == PasswordVerificationResult.Failed)
                return Results.Unauthorized();

            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Username),
                    new Claim("nome", usuario.Nome),
                    new Claim("idade", usuario.Idade.ToString()),
                    new Claim("cpf", usuario.CPF),
                    new Claim("cidade", usuario.Cidade),
                    new Claim("estado", usuario.Estado)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(jwtKey),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Results.Ok(new
            {
                message = "Login realizado com sucesso!",
                token = tokenHandler.WriteToken(token)
            });
        });

        group.MapPost("/create", async (UsuarioDbContext db, UsuarioCreateDto input, RabbitMQPublisher publisher) =>
        {
            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = input.Nome,
                Idade = input.Idade,
                EstadoCivil = input.EstadoCivil,
                CPF = input.CPF,
                Cidade = input.Cidade,
                Estado = input.Estado
            };

            db.Usuarios.Add(usuario);

            db.OutboxMessages.Add(new OutboxMessage
            {
                EventType = "UsuarioCreated",
                Payload = JsonSerializer.Serialize(usuario)
            });

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("IX_Usuarios_CPF") == true)
                {
                    return Results.Conflict(new
                    {
                        message = "Já existe um usuário cadastrado com este CPF.",
                        field = "cpf",
                        error = "duplicate_cpf"
                    });
                }

                return Results.Problem(
                    title: "Erro ao salvar usuário",
                    detail: "Ocorreu um erro inesperado ao salvar os dados.",
                    statusCode: 500);
            }

            publisher.Publish(new
            {
                Event = "UsuarioCreated",
                Data = usuario
            });

            return Results.Created($"/usuarios/{usuario.Id}", usuario);
        });

        // Listar usuários
        group.MapGet("/getAll", async (UsuarioDbContext db, int page = 1, int pageSize = 10) =>
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var total = await db.Usuarios.CountAsync();

            var data = await db.Usuarios
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Results.Ok(new { total, page, pageSize, data });
        });

        // Buscar por ID
        group.MapGet("/getById/{id:Guid}", async (UsuarioDbContext db, Guid id) =>
        {
            var usuario = await db.Usuarios.FindAsync(id);

            return usuario is null
                ? Results.NotFound()
                : Results.Ok(usuario);
        });

    group.MapPost("/forgot-password", async (UsuarioDbContext db, string email) =>
    {
      var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Username == email || u.CPF == email);
      if (usuario == null)
      {
        return Results.NotFound(new { message = "Usuário não encontrado" });
      }

      var tokenHandler = new JwtSecurityTokenHandler();
      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(new[]
          {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString())
        }),
        Expires = DateTime.UtcNow.AddMinutes(15), // token expira em 15 minutos
        SigningCredentials = new SigningCredentials(
              new SymmetricSecurityKey(jwtKey),
              SecurityAlgorithms.HmacSha256Signature)
      };

      var resetToken = tokenHandler.CreateToken(tokenDescriptor);
      var tokenString = tokenHandler.WriteToken(resetToken);

      // Aqui você enviaria o tokenString por email ao usuário
      return Results.Ok(new { message = "Email de recuperação enviado!", token = tokenString });
    });

    group.MapPost("/reset-password", async (UsuarioDbContext db, string token, string newPassword) =>
    {
      var tokenHandler = new JwtSecurityTokenHandler();
      try
      {
        var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
          ValidateIssuer = false,
          ValidateAudience = false,
          ClockSkew = TimeSpan.Zero
        }, out SecurityToken validatedToken);

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Results.BadRequest(new { message = "Token inválido" });

        var usuario = await db.Usuarios.FindAsync(Guid.Parse(userId));
        if (usuario == null) return Results.NotFound(new { message = "Usuário não encontrado" });

        var hasher = new PasswordHasher<Usuario>();
        usuario.PasswordHash = hasher.HashPassword(usuario, newPassword);

        await db.SaveChangesAsync();

        return Results.Ok(new { message = "Senha redefinida com sucesso!" });
      }

      catch (Exception)
      {
        return Results.Problem(
            title: "Token inválido ou expirado",
            statusCode: 401
        );
      }
    });

    group.MapPost("/refresh-token", (string refreshToken, IRefreshTokenRepository repo) =>
    {
      try
      {
        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
        {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
          ValidateIssuer = false,
          ValidateAudience = false,
          ClockSkew = TimeSpan.Zero
        }, out SecurityToken validatedToken);

        // 🔎 Validação de claim obrigatória
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
          return Results.Problem(
              title: "Token não contém o identificador do usuário",
              statusCode: 401
          );
        }

        // 🔒 Verifica se o refresh token ainda é válido no repositório
        var storedToken = repo.Get(refreshToken);
        if (storedToken == null || storedToken.IsRevoked || storedToken.Expires < DateTime.UtcNow)
        {
          return Results.Problem(
              title: "Refresh token inválido ou expirado",
              statusCode: 401
          );
        }

        // 🚫 Invalida o refresh token antigo
        repo.Revoke(refreshToken);

        // 🔄 Gera novo refresh token
        var newRefreshToken = Guid.NewGuid().ToString("N");
        repo.Save(new RefreshToken
        {
          Token = newRefreshToken,
          UserId = userId,
          Expires = DateTime.UtcNow.AddDays(7) // validade de 7 dias
        });

        // 🎟️ Cria novo access token
        var newTokenDescriptor = new SecurityTokenDescriptor
        {
          Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }),
          Expires = DateTime.UtcNow.AddHours(1),
          SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(jwtKey), SecurityAlgorithms.HmacSha256Signature)
        };

        var newToken = tokenHandler.CreateToken(newTokenDescriptor);

        return Results.Ok(new
        {
          token = tokenHandler.WriteToken(newToken),
          refreshToken = newRefreshToken
        });
      }
      catch
      {
        return Results.Problem(
            title: "Refresh token inválido",
            statusCode: 401
        );
      }
    });

    // Atualizar
    group.MapPut("/update/{id:Guid}", async (UsuarioDbContext db, Guid id, UsuarioCreateDto input, RabbitMQPublisher publisher) =>
        {
            var usuario = await db.Usuarios.FindAsync(id);

            if (usuario is null)
                return Results.NotFound();

            usuario.Nome = input.Nome;
            usuario.Idade = input.Idade;
            usuario.EstadoCivil = input.EstadoCivil;
            usuario.CPF = input.CPF;
            usuario.Cidade = input.Cidade;
            usuario.Estado = input.Estado;

            db.OutboxMessages.Add(new OutboxMessage
            {
                EventType = "UsuarioUpdated",
                Payload = JsonSerializer.Serialize(usuario)
            });

            await db.SaveChangesAsync();

            publisher.Publish(new
            {
                Event = "UsuarioUpdated",
                Data = usuario
            });

            return Results.Ok(usuario);
        });

        // Deletar
        group.MapDelete("/remote/{id:Guid}", async (UsuarioDbContext db, Guid id, RabbitMQPublisher publisher) =>
        {
            var usuario = await db.Usuarios.FindAsync(id);

            if (usuario is null)
                return Results.NotFound();

            db.Usuarios.Remove(usuario);

            db.OutboxMessages.Add(new OutboxMessage
            {
                EventType = "UsuarioDeleted",
                Payload = JsonSerializer.Serialize(usuario)
            });

            await db.SaveChangesAsync();

            publisher.Publish(new
            {
                Event = "UsuarioDeleted",
                Data = usuario
            });

            return Results.NoContent();
        });
    }
}
