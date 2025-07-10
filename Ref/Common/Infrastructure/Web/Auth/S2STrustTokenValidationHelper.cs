using CargoWise.RefDbRepo.Common.Utils;

namespace CargoWise.RefDbRepo.Common.Web.Auth
{
	public class S2STrustTokenValidationHelper : TokenValidationHelper
	{
		public S2STrustTokenValidationHelper(string authority, string audience, bool disableAuthentication = false) : base(authority, audience, disableAuthentication)
		{
		}

		public override bool ShouldHandle(string accessToken)
		{
			var tokenReader = new JwtTokenReader(accessToken);
			var azp = tokenReader.GetClaimValue(AuthClaimType.Azp);
			var uniqueName = tokenReader.GetClaimValue(AuthClaimType.UniqueName);
			return !string.IsNullOrEmpty(azp) && string.IsNullOrEmpty(uniqueName);
		}
	}
}
