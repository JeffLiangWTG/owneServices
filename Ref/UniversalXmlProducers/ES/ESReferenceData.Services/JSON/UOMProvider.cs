using System;

namespace CargoWise.RefDbRepo.ESReferenceData.Services
{
	public static class UOMProvider
	{
		public static Uri GetQueryUrlForSpanishUOM(string refDbServiceURI, DateTime currentDate)
			=> new Uri($"{refDbServiceURI}{RefDbRepoMapURL}{PropertiesSelected}{MapTypeFilter}{DataGroupingFilter} and ZZM_EndDate ge {currentDate:s}Z");

		const string RefDbRepoMapURL = "RefCusMapUpdate?";
		const string PropertiesSelected = "$select=ZZM_CW1orCommercialValue, ZZM_CustomsValue&";
		const string MapTypeFilter = "$filter=ZZM_ZZP_NKMapType eq 'CUSUQ'";
		const string DataGroupingFilter = " and ZZM_ZZZ_NKDataGrouping eq 'ES'";
	}
}
