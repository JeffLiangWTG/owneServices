using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefCarrierCodeUpdaterInfo_1 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefCarrierCode",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCarrierCodeAttributes", "ZZG_ZZ4_CarrierCode" },
				{ "RefCarrierVesselPivots", "ZZQ_ZZ4" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCarrierCodeAttributes", new DataTableMapping { TableName = "#TempRefCarrierCodeAttribute" }
				},
				{
					"RefCarrierVesselPivots", new DataTableMapping {
						TableName = "#TempRefCarrierVesselPivot",
						RelatedFKColumnNames = new Dictionary<string, string>()
						{
							{ "RefVesselZZ", "ZZQ_ZZO" }
						},
						RelatedTableNames = new Dictionary<string, DataTableMapping>()
						{
							{
								"RefVesselZZ", new DataTableMapping { TableName = "#TempRefVesselZZ" }
							}
						}
					}
				}
			}
		};
	}
}
