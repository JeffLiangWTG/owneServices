using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccAPOrgTaxConfigurationCollection : AccOrgTaxConfigurationCollectionByLedger
	{
		public AccAPOrgTaxConfigurationCollection(OrgCompanyData parentOrgCompanyData) : base(parentOrgCompanyData, LedgerTypes.AccountsPayable)
		{
		}
	}
}
