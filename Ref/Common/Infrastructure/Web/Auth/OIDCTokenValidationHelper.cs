using CargoWise.RefDbRepo.Common.Utils;

namespace CargoWise.RefDbRepo.Common.Web.Auth
{
	public class OIDCTokenValidationHelper : TokenValidationHelper
	{
		public OIDCTokenValidationHelper(string authority, string audience, bool disableAuthentication = false) : base(authority, audience, disableAuthentication)
		{
		}

		public override bool ShouldHandle(string accessToken)
		{
			var tokenReader = new JwtTokenReader(accessToken);
			var uniqueName = tokenReader.GetClaimValue(AuthClaimType.UniqueName);
			return !string.IsNullOrEmpty(uniqueName);
		}
	}
}
