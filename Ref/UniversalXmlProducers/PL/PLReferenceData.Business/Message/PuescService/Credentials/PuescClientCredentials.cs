using System.IdentityModel.Selectors;
using System.ServiceModel.Description;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Message.PuescService.Credentials;

sealed class PuescClientCredentials : ClientCredentials
{
	public PuescClientCredentials()
	{
	}

	PuescClientCredentials(PuescClientCredentials cc)
		: base(cc)
	{
	}

	public override SecurityTokenManager CreateSecurityTokenManager() => new PuescSecurityTokenManager(this);

	protected override ClientCredentials CloneCore() => new PuescClientCredentials(this);
}
