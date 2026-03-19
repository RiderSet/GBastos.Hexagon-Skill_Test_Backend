namespace GBastos.Hexagon_Skill_Test.Api.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public int Idade { get; set; }
    public string EstadoCivil { get; set; } = null!;
    public string CPF { get; set; } = null!;
    public string Cidade { get; set; } = null!;
    public string Estado { get; set; } = null!;
}