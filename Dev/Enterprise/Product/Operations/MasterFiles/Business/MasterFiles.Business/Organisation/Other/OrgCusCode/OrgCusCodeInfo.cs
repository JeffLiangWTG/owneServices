using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CountryCompliance;

namespace Enterprise.MasterFiles.Business
{
	public abstract class OrgCusCodeInfo
	{
		//Implement logic specific to OrgCusCode for specific country in Countries subfolder here.
		//Inherit it from OrgCusCodeInfo, name it like {CountryName}OrgCusCodeInfo.cs}) at that location, add to OrgCusCodeCountryFactory.GetOrgCusCodeInfo method.
		//Write tests in MasterFiles/Business/Business.Testing/Organisation/Other/OrgCusCode/Countries/
		//Ideally, move all obsolete OrgCusCode related old logic for a country you change here and remove interface implementation from there completely.

		static string GetLocalBusinessRegNoCodeType(ZString countryCode) => CountryComplianceFactory.GetICountryComplianceInfo(countryCode)?.GetLocalBusinessRegNoCodeType();

		static string GetConsumptionTaxRegistrationCode(ZString countryCode) => CountryComplianceFactory.GetICountryComplianceInfo(countryCode)?.GetConsumptionTaxRegistrationCode();

		public static HashSet<string> GetPrimaryCusCodes(ZString countryCode)
		{
			var result = new HashSet<string>();
			result.Add(OrgCusCode.CodeTypes.CreditAgencyCode);
			result.Add(OrgCusCode.CodeTypes.ExternalCreditorAccountCode);
			result.Add(OrgCusCode.CodeTypes.ExternalDebtorAccountCode);
			result.Add(OrgCusCode.CodeTypes.CorporationCode);
			result.Add(OrgCusCode.CodeTypes.GovBusinessCode);
			result.Add(OrgCusCode.CodeTypes.TaxFileCode);
			result.Add(GetConsumptionTaxRegistrationCode(countryCode));
			result.Add(GetLocalBusinessRegNoCodeType(countryCode));

			return result.Where(x => !string.IsNullOrEmpty(x)).ToHashSet();
		}

		public static HashSet<string> GetMainOrganizationNumberTypes(ZString countryCode)
		{
			var result = new HashSet<string>();

			var localBusinessRegNoCode = GetLocalBusinessRegNoCodeType(countryCode);
			var taxCode = GetConsumptionTaxRegistrationCode(countryCode);

			result.Add(localBusinessRegNoCode);
			result.Add(taxCode);

			if (taxCode == localBusinessRegNoCode && localBusinessRegNoCode != OrgCusCode.CodeTypes.CorporationCode)
			{
				result.Add(OrgCusCode.CodeTypes.CorporationCode);
			}

			return result.Where(x => !string.IsNullOrEmpty(x)).ToHashSet();
		}
	}
}
