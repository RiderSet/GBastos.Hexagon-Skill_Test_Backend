namespace GBastos.Hexagon_Skill_Test.Api.Endpoints;

public class RefreshToken
{
  public required string Token { get; set; }
  public required string UserId { get; set; }
  public DateTime Expires { get; set; }
  public bool IsRevoked { get; set; }
}
