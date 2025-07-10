using System.IdentityModel.Selectors;
using System.ServiceModel;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Message.PuescService.Credentials;

sealed class PuescSecurityTokenManager(PuescClientCredentials cred) : ClientCredentialsSecurityTokenManager(cred)
{
	public override SecurityTokenSerializer CreateSecurityTokenSerializer(SecurityTokenVersion version) => new PuescSecurityTokenSerializer();
}
