using GBastos.Hexagon_Skill_Test.Api.Data;
using GBastos.Hexagon_Skill_Test.Api.Messaging;
using GBastos.Hexagon_Skill_Test.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GBastos.Hexagon_Skill_Test.Api.Endpoints;

public static class UsuarioEndpoints
{
    public static void MapUsuarioEndpoints(this WebApplication app)
    {
        var publisher = new RabbitMQPublisher(app.Configuration);

        // Criar usuário
        app.MapPost("/usuarios", [Authorize] async (Usuario usuario, UsuarioDbContext db) =>
        {
            db.Usuarios.Add(usuario);
            await db.SaveChangesAsync();
            publisher.Publish(usuario);
            return Results.Created($"/usuarios/{usuario.Id}", usuario);
        });

        // Listar todos
        app.MapGet("/usuarios", [Authorize] async (UsuarioDbContext db) =>
            await db.Usuarios.ToListAsync());

        // Obter por Id
        app.MapGet("/usuarios/{id}", [Authorize] async (int id, UsuarioDbContext db) =>
        {
            var usuario = await db.Usuarios.FindAsync(id);
            return usuario != null ? Results.Ok(usuario) : Results.NotFound();
        });

        // Atualizar
        app.MapPut("/usuarios/{id}", [Authorize] async (int id, Usuario updated, UsuarioDbContext db) =>
        {
            var usuario = await db.Usuarios.FindAsync(id);
            if (usuario == null) return Results.NotFound();

            usuario.Nome = updated.Nome;
            usuario.Idade = updated.Idade;
            usuario.EstadoCivil = updated.EstadoCivil;
            usuario.CPF = updated.CPF;
            usuario.Cidade = updated.Cidade;
            usuario.Estado = updated.Estado;

            await db.SaveChangesAsync();
            publisher.Publish(usuario);
            return Results.Ok(usuario);
        });

        // Remover
        app.MapDelete("/usuarios/{id}", [Authorize] async (int id, UsuarioDbContext db) =>
        {
            var usuario = await db.Usuarios.FindAsync(id);
            if (usuario == null) return Results.NotFound();

            db.Usuarios.Remove(usuario);
            await db.SaveChangesAsync();
            publisher.Publish(usuario);
            return Results.NoContent();
        });
    }
}