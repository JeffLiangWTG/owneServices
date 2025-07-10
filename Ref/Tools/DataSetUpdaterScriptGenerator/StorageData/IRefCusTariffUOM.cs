using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusTariffUOM : IDataSetStorage
	{
		Guid ZZ8_PK { get; set; }
		Nullable<Guid> ZZ8_ZZ1_Tariff { get; set; }
		string ZZ8_Type { get; set; }
		string ZZ8_UOM { get; set; }
		Nullable<Guid> ZZ8_ZZA_TradeGroup { get; set; }
		Nullable<Guid> ZZ8_ZZW_TariffNationalCode { get; set; }
		string ZZ8_ZZZ_NKDataGrouping { get; set; }
		Nullable<Guid> ZZ8_ZZA_SecondTradeGroup { get; set; }
		Nullable<System.DateTime> ZZ8_StartDate { get; set; }
		Nullable<System.DateTime> ZZ8_EndDate { get; set; }
	}
}
