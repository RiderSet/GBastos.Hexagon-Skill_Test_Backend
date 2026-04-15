using GBastos.Hexagon_Skill_Test.Api.Endpoints;

namespace GBastos.Hexagon_Skill_Test.Api.Interfaces;

public interface IRefreshTokenRepository
{
  RefreshToken Get(string token);
  void Save(RefreshToken refreshToken);
  void Revoke(string token);
}
