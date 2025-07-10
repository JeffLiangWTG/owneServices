using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefAirlineProductCodeUpdaterInfo_1 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefAirlineProductCode",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefAirlineProductCodeCommodityCodePivots", "RPC_RAR" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefAirlineProductCodeCommodityCodePivots", new DataTableMapping {
						TableName = "#TempRefAirlineProductCodeCommodityCodePivot",
						RelatedFKColumnNames = new Dictionary<string, string>()
						{
							{ "RefAirlineCommodityCode", "RPC_RAC" }
						},
						RelatedTableNames = new Dictionary<string, DataTableMapping>()
						{
							{
								"RefAirlineCommodityCode", new DataTableMapping { TableName = "#TempRefAirlineCommodityCode" }
							}
						}
					}
				}
			}
		};
	}
}
