using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public class JwtTokenReader
	{
		readonly IEnumerable<Claim> claims;

		public JwtTokenReader(string accessToken)
		{
			var handler = new JwtSecurityTokenHandler();
			var jwtToken = handler.ReadJwtToken(accessToken);
			claims = jwtToken.Claims;
		}

		public string GetClaimValue(string type)
		{
			return claims.FirstOrDefault(x => x.Type == type)?.Value;
		}
	}
}
