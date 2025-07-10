using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusRate : IDataSetStorage
	{
		Guid ZZ2_PK { get; set; }
		Nullable<Guid> ZZ2_ZZ1_Tariff { get; set; }
		DateTime ZZ2_StartDate { get; set; }
		DateTime ZZ2_EndDate { get; set; }
		string ZZ2_RateFormula { get; set; }
		Nullable<Guid> ZZ2_ZY1_RateCode { get; set; }
		Nullable<Guid> ZZ2_ZZS_Preference { get; set; }
		Nullable<Guid> ZZ2_ZZW_TariffNationalCode { get; set; }
		string ZZ2_ZZZ_NKDataGrouping { get; set; }
		string ZZ2_RateFormulaDerivedFrom { get; set; }
		string ZZ2_RX_NKCurrencyOverride { get; set; }

		IEnumerable<IRefCusApplicability> RefCusApplicability { get; }
		IEnumerable<IRefCusRateUOM> RefCusRateUOM { get; }
	}
}
