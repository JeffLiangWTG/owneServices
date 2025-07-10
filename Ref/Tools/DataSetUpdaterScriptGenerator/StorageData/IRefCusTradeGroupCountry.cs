using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusTradeGroupCountry : IDataSetStorage
	{
		Guid ZZB_PK { get; set; }
		Guid ZZB_ZZA_TradeGroup { get; set; }
		string ZZB_RN_NKTradeGroupCountryCode { get; set; }
		DateTime ZZB_StartDate { get; set; }
		DateTime ZZB_EndDate { get; set; }
		string ZZB_Description { get; set; }
	}
}
