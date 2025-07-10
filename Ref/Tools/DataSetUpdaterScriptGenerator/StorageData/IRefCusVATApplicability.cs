using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusVATApplicability : IDataSetStorage
	{
		Guid ZX5_PK { get; set; }
		Nullable<Guid> ZX5_ZZ1_Tariff { get; set; }
		Nullable<Guid> ZX5_ZZW_TariffNationalCode { get; set; }
		string ZX5_ZZF_NKTaxOrFeeCode { get; set; }
		DateTime ZX5_StartDate { get; set; }
		DateTime ZX5_EndDate { get; set; }
		string ZX5_AdditionalCode { get; set; }
		string ZX5_Description { get; set; }
		short ZX5_DataSetId { get; set; }
		string ZX5_ZZZ_NKDataGrouping { get; set; }
		Nullable<Guid> ZX5_ZZA_TradeGroup { get; set; }
		string ZX5_VATCategory { get; set; }
	}
}
