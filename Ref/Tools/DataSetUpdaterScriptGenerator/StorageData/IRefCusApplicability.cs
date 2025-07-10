using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusApplicability : IDataSetStorage
	{
		Guid ZZT_PK { get; set; }
		Nullable<Guid> ZZT_ZZ2_Rate { get; set; }
		Nullable<Guid> ZZT_ZX1_Conditions { get; set; }
		DateTime ZZT_StartDate { get; set; }
		DateTime ZZT_EndDate { get; set; }
		Nullable<Guid> ZZT_ZZA_TradeGroup { get; set; }
		Nullable<Guid> ZZT_ZZH_TariffRelationship { get; set; }
		string ZZT_AdditionalCode { get; set; }
		string ZZT_OrderNumber { get; set; }
		Nullable<Guid> ZZT_ZY2_AdditionalCode { get; set; }
		Nullable<Guid> ZZT_ZZA_SecondTradeGroup { get; set; }

		IEnumerable<IRefCusExcludedTradeGroup> RefCusExcludedTradeGroup { get; }
	}
}
