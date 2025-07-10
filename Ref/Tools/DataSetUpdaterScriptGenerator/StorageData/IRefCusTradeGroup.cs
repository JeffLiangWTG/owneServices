using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusTradeGroup : IDataSetStorage
	{
		Guid ZZA_PK { get; set; }
		string ZZA_TradeGroup { get; set; }
		string ZZA_Description { get; set; }
		DateTime ZZA_StartDate { get; set; }
		DateTime ZZA_EndDate { get; set; }
		string ZZA_ZZZ_NKDataGrouping { get; set; }

		IEnumerable<IRefCusApplicability> RefCusApplicability { get; }
		IEnumerable<IRefCusExcludedTradeGroup> RefCusExcludedTradeGroup { get; }
		IEnumerable<IRefCusTariffUOM> RefCusTariffUOM { get; }
		IEnumerable<IRefCusTariffUOM> RefCusTariffUOM1 { get; }
		IEnumerable<IRefCusTradeGroupCountry> RefCusTradeGroupCountry { get; }
		IEnumerable<IRefCusVATApplicability> RefCusVATApplicability { get; }
		IEnumerable<IRefCusApplicability> RefCusApplicability1 { get; }
	}
}
