using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.ServiceHost
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
	public sealed class EDIClientBasicAuthenticationAttribute : IdentityBasicAuthenticationAttribute
	{
		readonly string ApplicationCode;

		public EDIClientBasicAuthenticationAttribute(string applicationCode)
		{
			ApplicationCode = applicationCode;
		}

		protected override bool IsOK(string userName, string password, out string message)
		{
			message = string.Empty;
			var cache = ObjectFactory.Get<ICommunicationPartyConfigCache>();

			if (cache.TryGetInboundCommunicationPartyConfigByUsername(new ZString(userName), ApplicationCode, out var storedCommunicationPartyConfig))
			{
				if (storedCommunicationPartyConfig.Auth.Username.Equals(userName) && storedCommunicationPartyConfig.Auth.Password.Equals(password))
				{
					ObjectFactory.Get<IMessagingContext>().CurrentInboundConfig = storedCommunicationPartyConfig;
					return true;
				}
			}

			message = (NoResString)"Invalid Credentials.";
			return false;
		}
	}
}
