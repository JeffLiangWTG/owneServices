using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public static class RefCusVATApplicabilityMapping
	{
		public static DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefCusVATApplicability",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusTradeGroup", "ZX5_ZZA_TradeGroup" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCusTradeGroup", new DataTableMapping { TableName = "#TempRefCusTradeGroup" }
				}
			}
		};
	}
}
