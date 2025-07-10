using CargoWise.Application;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class EDICommunicationAuthLookups : AutoEDICommunicationAuthLookups
	{
		public EDICommunicationAuthLookups(AutoEDICommunicationAuth parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList AuthModesList
		{
			get
			{
				if ((Parent as EDICommunicationAuth).Config.ECC_Direction == EDICommunicationPartyConfigDirectionsList.Codes.Inbound)
				{
					return Factory.GetCachedValue("EDICommunicationAuthLookups.InboundAuthModesList", () =>
					{
						var modesList = new EDICommunicationAuthModesList();
						modesList.RemoveCode(EDICommunicationAuthModesList.Codes.NoAuthentication);
						if (EnvProxy.IsHostedWithCargowise)
						{
							modesList.RemoveCode(EDICommunicationAuthModesList.Codes.BasicAuthentication);
						}
						if (!SupportsEntity(AccessRequirement.SupportsInboundOAuth))
						{
							modesList.RemoveCode(EDICommunicationAuthModesList.Codes.OAuthAuthentication);
						}
						if (!SupportsEntity(AccessRequirement.SupportsInboundBasicAuth))
						{
							modesList.RemoveCode(EDICommunicationAuthModesList.Codes.BasicAuthentication);
						}
						if (!SupportsEntity(AccessRequirement.SupportsInbound))
						{
							modesList.RemoveCode(EDICommunicationAuthModesList.Codes.OAuthAuthentication);
							modesList.RemoveCode(EDICommunicationAuthModesList.Codes.BasicAuthentication);
						}
						return modesList;
					});
				}
				return Factory.GetCachedValue("EDICommunicationAuthLookups.OutboundAuthModesList", () =>
				{
					var modesList = new EDICommunicationAuthModesList();
					if (!SupportsEntity(AccessRequirement.SupportsOutboundOAuth))
					{
						modesList.RemoveCode(EDICommunicationAuthModesList.Codes.OAuthAuthentication);
					}
					if (!SupportsEntity(AccessRequirement.SupportsOutboundBasicAuth))
					{
						modesList.RemoveCode(EDICommunicationAuthModesList.Codes.BasicAuthentication);
					}
					if (!SupportsEntity(AccessRequirement.SupportsOutboundNoAuth))
					{
						modesList.RemoveCode(EDICommunicationAuthModesList.Codes.NoAuthentication);
					}
					if (!SupportsEntity(AccessRequirement.SupportsOutbound))
					{
						modesList.RemoveCode(EDICommunicationAuthModesList.Codes.OAuthAuthentication);
						modesList.RemoveCode(EDICommunicationAuthModesList.Codes.BasicAuthentication);
						modesList.RemoveCode(EDICommunicationAuthModesList.Codes.NoAuthentication);
					}
					return modesList;
				});
			}
		}

		public CodeDescriptionPairList GrantTypesList
		{
			get
			{
				return Factory.GetCachedValue("EDICommunicationAuthLookups.GrantTypesList", () =>
				{
					return new EDICommunicationAuthOutboundGrantTypesList();
				});
			}
		}
		ZBool SupportsEntity(AccessRequirement key)
		{
			return ApplicationDescriptor != null && ApplicationDescriptor.AccessTypes.Contains(key);
		}

		IEDIClientApplicationDescriptor ApplicationDescriptor => ObjectFactory.Get<IEDIClientApplicationDescriptors>().GetValue((Parent as EDICommunicationAuth).Config.Party.ECP_ApplicationCode);
	}
}
