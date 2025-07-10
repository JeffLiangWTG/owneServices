using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefCusTradeGroupUpdaterInfo_3 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefCusTradeGroup",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusTradeGroupCountries", "ZZB_ZZA_TradeGroup" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCusTradeGroupCountries", new DataTableMapping { TableName = "#TempRefCusTradeGroupCountry" }
				}
			}
		};
	}
}
