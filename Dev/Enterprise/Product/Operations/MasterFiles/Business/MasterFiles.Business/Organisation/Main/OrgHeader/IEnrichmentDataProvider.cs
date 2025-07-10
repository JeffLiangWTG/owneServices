using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IEnrichmentDataProvider
	{
		OrgAddressDependentCollection Addresses { get; }
		OrgBrandOrRelatedNameCollection BrandsOrRelatedNames { get; }
		OrgContactDependentCollection Contacts { get; }
		OrgCusCodeCollection CustomsCodes { get; }
		OrgWebURLDependentCollection OrgWebURLs { get; }
		ZString OH_RL_NKClosestPort { get; }
		int OrganisationTypes { get; }
	}
}
