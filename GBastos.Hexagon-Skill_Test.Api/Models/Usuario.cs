namespace GBastos.Hexagon_Skill_Test.Api.Models;

public class Usuario
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Nome { get; set; } = string.Empty;
    public int Idade { get; set; }
    public string EstadoCivil { get; set; } = null!;
    public string CPF { get; set; } = null!;
    public string Cidade { get; set; } = null!;
    public string Estado { get; set; } = null!;
}