using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefUNLOCOUpdaterInfo_2 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefUNLOCO",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefUNLOCOUtcOffsets", "RLO_RL_NKCode" },
				{ "RefUNLOCORelatedPorts", "RLR_RL_NKRelatedPort" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefUNLOCOUtcOffsets", new DataTableMapping { TableName = "#TempRefUNLOCOUtcOffset" }
				},
				{
					"RefUNLOCORelatedPorts", new DataTableMapping { TableName = "#TempRefUNLOCORelatedPort" }
				}
			}
		};
	}
}
