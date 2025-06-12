using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Security.Tokens;

namespace CargoWise.eHub.Products.NZCustoms.Client.Wcf
{
	internal class TokenManagerWithTransportCredentials : ClientCredentialsSecurityTokenManager
	{
		readonly ClientCredentialsWithTransportCertificate credentialsWithTransportCertificate;

		public TokenManagerWithTransportCredentials(ClientCredentialsWithTransportCertificate credentialsWithTransportCertificate)
			: base(credentialsWithTransportCertificate)
		{
			this.credentialsWithTransportCertificate = credentialsWithTransportCertificate;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public override SecurityTokenProvider CreateSecurityTokenProvider(SecurityTokenRequirement requirement)
		{
			SecurityTokenProvider result;

			if (requirement.Properties.ContainsKey(ServiceModelSecurityTokenRequirement.TransportSchemeProperty) &&
				requirement.TokenType == SecurityTokenTypes.X509Certificate)
			{
				result = new X509SecurityTokenProvider(credentialsWithTransportCertificate.TransportCertificate);
			}
			else
			{
				if (requirement.KeyUsage == SecurityKeyUsage.Signature &&
					requirement.TokenType == SecurityTokenTypes.X509Certificate)
				{
					result =
						new X509SecurityTokenProvider(credentialsWithTransportCertificate.ClientCertificate.Certificate);
				}
				else
				{
					result = base.CreateSecurityTokenProvider(requirement);
				}
			}

			return result;
		}
	}
}