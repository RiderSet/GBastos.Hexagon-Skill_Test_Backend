namespace GBastos.Hexagon_Skill_Test.Api.Models;

public class UsuarioCreateDto
{
    public string Nome { get; set; } = string.Empty;
    public int Idade { get; set; }
    public string EstadoCivil { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}