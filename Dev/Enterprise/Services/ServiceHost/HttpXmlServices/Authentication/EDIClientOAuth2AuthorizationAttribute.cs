using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using CargoWise.Application;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.eServices;

namespace Enterprise.Services.ServiceHost
{
	public sealed class EDIClientOAuth2AuthorizationAttribute : OAuth2AuthorizationAttribute
	{
		readonly string ApplicationCode;

		public EDIClientOAuth2AuthorizationAttribute(string applicationCode)
		{
			ApplicationCode = applicationCode;
		}

		protected override bool ValidateClientIdAndAuthUrl(string clientID, string authorityUrl, JwtSecurityToken token)
		{
			var cache = ObjectFactory.Get<ICommunicationPartyConfigCache>();

			if (cache.TryGetInboundCommunicationPartyConfigByClientId(clientID, authorityUrl, ApplicationCode, out var storedCommunicationPartyConfig))
			{
				ObjectFactory.Get<IMessagingContext>().CurrentInboundConfig = storedCommunicationPartyConfig;
				return true;
			}

			return eAdaptorRegistry.Instance.eAdaptorInboundOAuthAuthorityUrls.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).Any(x => token.Issuer.Equals(x, StringComparison.OrdinalIgnoreCase))
					&& eAdaptorRegistry.Instance.eAdaptorInboundOAuthClientIDs.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).Contains(clientID);
		}
	}
}
