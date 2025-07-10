namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface IOrgCusCodePredicateProvider
	{
		bool IncludeForPlaceOfSupplyTaxRegsistration(OrgCusCode orgCusCode);
		bool IncludeForOrgHeaderTaxRegsistration(OrgCusCode orgCusCode);
	}
}
