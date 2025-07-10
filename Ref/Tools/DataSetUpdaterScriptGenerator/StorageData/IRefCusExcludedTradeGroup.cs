using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusExcludedTradeGroup : IDataSetStorage
	{
		Guid ZZC_PK { get; set; }
		Guid ZZC_ZZT_Applicability { get; set; }
		Guid ZZC_ZZA_TradeGroup { get; set; }
	}
}
