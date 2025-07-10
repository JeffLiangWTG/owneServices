using System;
using System.IdentityModel.Tokens.Jwt;

namespace Enterprise.Services.ServiceHost
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public sealed class AuditApiAuthenticationAttribute : S2STAndBasicAuthenticationAttribute
	{
		public AuditApiAuthenticationAttribute()
			: base("https://login.microsoftonline.com/1b20b87e-cebd-43cc-97bd-bdd41a2f5cf1/v2.0", audienceExtension: null)
		{
		}

		static readonly Guid DdcApplicationId = new Guid("d04c4c51-4ac4-4e27-b4b2-f1529abb443b");

		internal override bool ValidateExtraClaims(JwtSecurityToken token)
		{
			var azpClaim = token.Payload.Azp;
			return Guid.TryParse(azpClaim, out var parsedAzp) && parsedAzp == DdcApplicationId;
		}
	}
}
