using System.ComponentModel.DataAnnotations;

namespace GBastos.Hexagon_Skill_Test.Api.Messaging.Outbox;

public class OutboxMessage
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string EventType { get; set; } = string.Empty;

    [Required]
    public string Payload { get; set; } = string.Empty;

    public bool Processed { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}