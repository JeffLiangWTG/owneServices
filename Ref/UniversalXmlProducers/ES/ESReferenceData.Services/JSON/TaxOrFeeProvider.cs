using System;

namespace CargoWise.RefDbRepo.ESReferenceData.Services
{
	public static class TaxOrFeeProvider
	{
		public static Uri GetQueryUrlForSpanishTaxOrFee(string refDbServiceURI, string codeType, DateTime currentDate)
			=> new Uri($"{refDbServiceURI}{RefDbRepoTaxOrFeeURL}{PropertiesSelected}$filter=startswith(ZZF_Code,'{codeType}'){DataGroupingFilter} and ZZF_EndDate ge {currentDate:s}Z");

		const string RefDbRepoTaxOrFeeURL = "RefCusTaxOrFeeUpdate?";
		const string PropertiesSelected = "$select=ZZF_Code, ZZF_Value&";
		const string DataGroupingFilter = " and ZZF_ZZZ_NKDataGrouping eq 'ES'";
	}
}
