using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccAROrgTaxConfigurationCollection : AccOrgTaxConfigurationCollectionByLedger
	{
		public AccAROrgTaxConfigurationCollection(OrgCompanyData parentOrgCompanyData) : base(parentOrgCompanyData, LedgerTypes.AccountsReceivable)
		{
		}
	}
}
