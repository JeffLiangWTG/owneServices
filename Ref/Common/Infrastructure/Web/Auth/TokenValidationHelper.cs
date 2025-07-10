using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WTG.OpenIDConnect.Token;

namespace CargoWise.RefDbRepo.Common.Web.Auth
{
	public abstract class TokenValidationHelper : ITokenValidationHelper
	{
		protected TokenValidationHelper(string authority, string audience, bool disableAuthentication = false)
		{
			this.authority = authority;
			this.audience = audience;
			this.disableAuthentication = disableAuthentication;
		}

		readonly string authority;
		readonly string audience;
		readonly bool disableAuthentication;

		public abstract bool ShouldHandle(string accessToken);

		public async Task<JwtSecurityToken> VerifyAccessTokenAsync(string accessToken, ConcurrentDictionary<string, IConfigurationManagerWithLock> configurationManagerCache, ILogger logger, CancellationToken cancellationToken)
		{
			if (disableAuthentication)
			{
				return new JwtSecurityToken(accessToken);
			}
			var jwtSecurityToken = await TokenValidator.ValidateAccessToken(authority, audience, accessToken, configurationManagerCache, logger, cancellationToken);
			return jwtSecurityToken;
		}
	}
}
