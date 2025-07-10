using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefAccTaxRate : IDataSetStorage
	{
		Guid ZAT_PK { get; set; }
		string ZAT_RN_NKCountry { get; set; }
		string ZAT_ReferenceRateType { get; set; }
		DateTime ZAT_StartDate { get; set; }
		DateTime ZAT_EndDate { get; set; }
		int ZAT_RateNumerator { get; set; }
		int ZAT_RateDenominator { get; set; }
	}
}
