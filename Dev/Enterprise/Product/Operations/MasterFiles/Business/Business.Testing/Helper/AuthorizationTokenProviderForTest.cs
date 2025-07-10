using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using CargoWise.Types;
using Microsoft.IdentityModel.Tokens;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class AuthorizationTokenProviderForTest : IAuthorizationTokenProvider
	{
		public DateTime ExpiryTime { get; set; } = ZDateTime.UtcNow.ToDateTime().AddHours(1);

		public bool ShouldThrowException { get; set; }

		public Exception Exception { get; set; }

		public string GetToken(AuthenticationCertificateInfo certificateInfo)
		{
			if (ShouldThrowException)
			{
				throw Exception;
			}

			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
			var startTime = ZDateTime.UtcNow.ToDateTime();
			var token =
				new JwtSecurityToken(
					"Microsoft.Security.Bearer",
					"Microsoft.Security.Bearer",
					null,
					notBefore: startTime,
					expires: ExpiryTime,
					signingCredentials: credentials);
			var receivedToken = new JwtSecurityTokenHandler().WriteToken(token);
			return receivedToken;
		}
	}
}
