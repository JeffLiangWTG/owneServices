using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WTG.OpenIDConnect.Token;

namespace CargoWise.Blazor.Common
{
	public interface ITokenValidatorWrapper
	{
		Task<JwtSecurityToken> ValidateIdentityTokenAsync(TokenValidatorParameters parameters);
	}

	public class TokenValidatorWrapper : ITokenValidatorWrapper
	{
		public async Task<JwtSecurityToken> ValidateIdentityTokenAsync(TokenValidatorParameters parameters)
		{
			return await TokenValidator.ValidateIdentityToken(
				parameters.authority,
				parameters.clientId,
				parameters.accessToken,
				parameters.configurationManagerCache,
				parameters.logger,
				parameters.cancellationToken);
		}
	}

	public record TokenValidatorParameters(
		string authority,
		string clientId,
		string accessToken,
		ConcurrentDictionary<string, IConfigurationManagerWithLock> configurationManagerCache,
		ILogger logger,
		CancellationToken cancellationToken);
}
