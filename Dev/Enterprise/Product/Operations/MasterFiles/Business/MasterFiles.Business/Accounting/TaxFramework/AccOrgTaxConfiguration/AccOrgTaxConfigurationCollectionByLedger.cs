using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public abstract class AccOrgTaxConfigurationCollectionByLedger : AccOrgTaxConfigurationCollection
	{
		protected AccOrgTaxConfigurationCollectionByLedger(OrgCompanyData parentOrgCompanyData, ZString ledger)
			: base(parentOrgCompanyData.Factory, parentOrgCompanyData)
		{
			parentOrgCompanyDataPK = parentOrgCompanyData.PK;
			parentLedger = ledger;
		}

		protected override void SetDefaultsForNewElementCore(AccOrgTaxConfiguration newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.OTC_OB = parentOrgCompanyDataPK;
			newElement.Ledger = parentLedger;
		}

		protected override bool MatchesFilterCore(AccOrgTaxConfiguration element, bool fetchOnlyFromLocalCache)
		{
			return base.MatchesFilterCore(element, fetchOnlyFromLocalCache) && element.Ledger == parentLedger;
		}

		readonly ZGuid parentOrgCompanyDataPK;
		readonly ZString parentLedger;
	}
}
