using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WTG.OpenIDConnect.Token;

namespace CargoWise.RefDbRepo.Common.Web.Auth
{
	public interface ITokenValidationHelper
	{
		bool ShouldHandle(string accessToken);
		Task<JwtSecurityToken> VerifyAccessTokenAsync(string accessToken, ConcurrentDictionary<string, IConfigurationManagerWithLock> configurationManagerCache, ILogger logger, CancellationToken cancellationToken);
	}
}
