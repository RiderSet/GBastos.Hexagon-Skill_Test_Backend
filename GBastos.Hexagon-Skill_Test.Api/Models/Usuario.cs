using System.ComponentModel.DataAnnotations;

namespace GBastos.Hexagon_Skill_Test.Api.Models;

public class Usuario
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    public int Idade { get; set; }

    [Required, MaxLength(50)]
    public string EstadoCivil { get; set; } = string.Empty;

    [Required, MaxLength(11)]
    public string CPF { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Cidade { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Estado { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;
}