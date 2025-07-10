using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using CargoWise.Application;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.Services.ServiceHost
{
	public sealed class GateManagementOAuth2AuthorizationAttribute : OAuth2AuthorizationAttribute
	{
		readonly string ApplicationCode = eAdaptorNextApplicationDescriptor.ApplicationCode; // ApplicationCode is now required to query EDICommunicationPartyConfig. The ability to create custom ApplicationCodes will be available in the future.

		protected override bool ValidateClientIdAndAuthUrl(string clientID, string authorityUrl, JwtSecurityToken token)
		{
			var cache = ObjectFactory.Get<ICommunicationPartyConfigCache>();

			if (cache.TryGetInboundCommunicationPartyConfigByClientId(clientID, authorityUrl, ApplicationCode, out var storedCommunicationPartyConfig))
			{
				ObjectFactory.Get<IMessagingContext>().CurrentInboundConfig = storedCommunicationPartyConfig;
				return true;
			}

			return WarehouseDataRegistry.Instance.GateManagementInboundOAuthAuthorityUrls.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).Any(x => token.Issuer.Equals(x, StringComparison.OrdinalIgnoreCase))
					&& WarehouseDataRegistry.Instance.GateManagementInboundOAuthClientIDs.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).Contains(clientID);
		}
	}
}
