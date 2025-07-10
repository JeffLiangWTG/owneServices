using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class EDICommunicationAuthValidation : AutoEDICommunicationAuthValidation
	{
		public EDICommunicationAuthValidation(AutoEDICommunicationAuth parent)
			: base(parent)
		{
		}

		public new EDICommunicationAuth Parent => (EDICommunicationAuth)base.Parent;

		void ValidateConfig()
		{
			Parent.ClearRowNotificationsContaining(Res.GetString("db6e9d00-4260-4f36-8aae-ad36637b207a", "Configuration must be verified before changes can be saved."));
			if (IsConfigActive && IsOutboundOAuth)
			{
				if (Parent.ECA_Certificate.IsEmpty && !Parent.ECA_RenewalEncodedPrivateKey.IsEmpty)
				{
					return;
				}

				if (!Parent.IsVerified)
				{
					Parent.AddRowError(Res.GetString("db6e9d00-4260-4f36-8aae-ad36637b207a", "Configuration must be verified before changes can be saved."));
				}
			}
		}

		protected override void CheckECA_AuthorizationMode()
		{
			if (IsConfigActive)
			{
				MandatoryValidation.CheckEntered(Parent.ECA_AuthorizationModeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.ECA_AuthorizationModeInfo, Parent.Lookups.AuthModesList);
			}
		}

		protected override void CheckECA_Password()
		{
			if(IsConfigActive && (IsBasic || IsOutboundOAuthPassword))
			{
				MandatoryValidation.CheckEntered(Parent.ECA_PasswordInfo);
			}
		}

		protected override void CheckECA_Username()
		{
			if (IsConfigActive && (IsBasic || IsOutboundOAuthPassword))
			{
				MandatoryValidation.CheckEntered(Parent.ECA_UsernameInfo);
			}
		}

		protected override void CheckECA_FlowCode()
		{
			if (IsConfigActive && IsOutboundOAuth)
			{
				MandatoryValidation.CheckEntered(Parent.ECA_FlowCodeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.ECA_FlowCodeInfo, Parent.Lookups.GrantTypesList);
			}
		}

		protected override void CheckECA_ClientSecret()
		{
			if (IsConfigActive && (IsOutboundOAuthClientCredentials || IsOutboundOAuthPassword))
			{
				MandatoryValidation.CheckEntered(Parent.ECA_ClientSecretInfo);
			}
		}

		protected override void CheckECA_AuthorizationEndpoint()
		{
			if (IsConfigActive && IsInboundOAuth && !EnvProxy.IsHostedWithCargowise && Parent.Config.ECC_IsSelfManaged)
			{
				MandatoryValidation.CheckEntered(Parent.ECA_AuthorizationEndpointInfo);
				if (!Regex.IsMatch(Parent.ECA_AuthorizationEndpoint, @"^https:\/\/([\w.-]+\.)?microsoftonline\.com(\/[^\/]+)?\/v2\.0\/?$", RegexOptions.IgnoreCase))
				{
					Parent.ECA_AuthorizationEndpointInfo.AddWarning(Res.GetString("b29f230d-ea9f-4d24-8760-f3d2b317caa8", "We currently offer official support only for Azure AD B2C. Using any other identity provider may result in unexpected behavior, and such use is at your own risk."));
				}
			}
		}

		protected override void CheckECA_ClientID()
		{
			if (IsConfigActive && IsInboundOAuth && !EnvProxy.IsHostedWithCargowise && Parent.Config.ECC_IsSelfManaged)
			{
				MandatoryValidation.CheckEntered(Parent.ECA_ClientIDInfo);
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateConfig();
		}

		bool IsConfigActive => Parent.Config?.ECC_IsActive == true && Parent.Config?.Party?.ECP_IsActive == true;
		bool IsBasic => Parent.Config != null && Parent.ECA_AuthorizationMode == EDICommunicationAuthModesList.Codes.BasicAuthentication;
		bool IsOAuth => Parent.Config != null && Parent.ECA_AuthorizationMode == EDICommunicationAuthModesList.Codes.OAuthAuthentication;
		bool IsOutboundOAuth => IsOAuth && Parent.Config.ECC_Direction == EDICommunicationPartyConfigDirectionsList.Codes.Outbound;
		bool IsOutboundOAuthClientCredentials => IsOutboundOAuth && Parent.ECA_FlowCode == EDICommunicationAuthOutboundGrantTypesList.Codes.ClientCredentials;
		bool IsOutboundOAuthPassword => IsOutboundOAuth && Parent.ECA_FlowCode == EDICommunicationAuthOutboundGrantTypesList.Codes.Password;
		bool IsInboundOAuth => IsOAuth && Parent.Config.ECC_Direction == EDICommunicationPartyConfigDirectionsList.Codes.Inbound;
	}
}
