using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public static class RefCusApplicabilityMapping
	{
		public static DataTableMapping Mapping1 => new DataTableMapping
		{
			TableName = "#TempRefCusApplicability",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusTradeGroup", "ZZT_ZZA_TradeGroup" },
				{ "RefCusExcludedTradeGroups", "ZZC_ZZT_Applicability" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCusTradeGroup", new DataTableMapping { TableName = "#TempRefCusTradeGroup" }
				},
				{
					"RefCusExcludedTradeGroups", new DataTableMapping {
						TableName = "#TempRefCusExcludedTradeGroup",
						RelatedFKColumnNames = new Dictionary<string, string>()
						{
							{ "RefCusTradeGroup", "ZZC_ZZA_TradeGroup" }
						},
						RelatedTableNames = new Dictionary<string, DataTableMapping>()
						{
							{
								"RefCusTradeGroup", new DataTableMapping { TableName = "#TempRefCusTradeGroup" }
							}
						}
					}
				}
			}
		};

		public static DataTableMapping Mapping2
		{
			get
			{
				var mapping = new DataTableMapping();
				mapping.TableName = Mapping1.TableName;
				mapping.RelatedFKColumnNames = Mapping1.RelatedFKColumnNames;
				mapping.RelatedFKColumnNames.Add("RefCusTradeGroup1", "ZZT_ZZA_SecondTradeGroup");

				mapping.RelatedTableNames = Mapping1.RelatedTableNames;
				mapping.RelatedTableNames.Add("RefCusTradeGroup1", new DataTableMapping { TableName = "#TempRefCusTradeGroup" });
				return mapping;
			}
		}
	}
}
