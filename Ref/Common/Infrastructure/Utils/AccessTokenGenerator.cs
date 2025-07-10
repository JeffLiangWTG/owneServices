using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public static class AccessTokenGenerator
	{
		public static string GenerateS2SAccessToken(string clientId, DateTime expDateTime)
		{
			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
				new Claim(JwtRegisteredClaimNames.Exp, ((DateTimeOffset)expDateTime).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)),
				new Claim(AuthClaimType.Azp, clientId)
			};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(new string('a', 150)));
			var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				"S2S Issuer",
				"S2S Audience",
				claims,
				signingCredentials: credentials);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		public static string GenerateOIDCAccessToken(string uniqueName, DateTime expDateTime, string clientId = "")
		{
			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
				new Claim(JwtRegisteredClaimNames.Exp, ((DateTimeOffset)expDateTime).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)),
				new Claim(AuthClaimType.UniqueName, uniqueName),
				new Claim(AuthClaimType.Azp, clientId)
			};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(new string('b', 160)));
			var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				"OIDC Issuer",
				"OIDC Audience",
				claims,
				signingCredentials: credentials);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}
