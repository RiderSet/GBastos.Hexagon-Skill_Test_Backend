using GBastos.Hexagon_Skill_Test.Api.Data;
using GBastos.Hexagon_Skill_Test.Api.Messaging.Brokers;
using GBastos.Hexagon_Skill_Test.Api.Messaging.Outbox;
using GBastos.Hexagon_Skill_Test.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

public static class UsuarioEndpoints
{
    public static void MapUsuarioEndpoints(this WebApplication app, RabbitMQPublisher publisher)
    {
        var group = app.MapGroup("/usuarios").RequireAuthorization();

        group.MapPost("", async (UsuarioDbContext db, UsuarioCreateDto input) =>
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

            await db.SaveChangesAsync();

            publisher.Publish(new { Event = "UsuarioCreated", Data = usuario });

            return Results.Created($"/usuarios/{usuario.Id}", usuario);
        });

        group.MapGet("", async (UsuarioDbContext db, int page = 1, int pageSize = 10) =>
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var total = await db.Usuarios.CountAsync();
            var data = await db.Usuarios.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return Results.Ok(new { total, page, pageSize, data });
        });

        group.MapGet("/{id:Guid}", async (UsuarioDbContext db, Guid id) =>
        {
            var usuario = await db.Usuarios.FindAsync(id);
            return usuario is null ? Results.NotFound() : Results.Ok(usuario);
        });

        group.MapPut("/{id:Guid}", async (UsuarioDbContext db, Guid id, UsuarioCreateDto input) =>
        {
            var usuario = await db.Usuarios.FindAsync(id);
            if (usuario is null) return Results.NotFound();

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
            publisher.Publish(new { Event = "UsuarioUpdated", Data = usuario });

            return Results.Ok(usuario);
        });

        group.MapDelete("/{id:Guid}", async (UsuarioDbContext db, Guid id) =>
        {
            var usuario = await db.Usuarios.FindAsync(id);
            if (usuario is null) return Results.NotFound();

            db.Usuarios.Remove(usuario);
            db.OutboxMessages.Add(new OutboxMessage
            {
                EventType = "UsuarioDeleted",
                Payload = JsonSerializer.Serialize(usuario)
            });

            await db.SaveChangesAsync();
            publisher.Publish(new { Event = "UsuarioDeleted", Data = usuario });

            return Results.NoContent();
        });
    }
}