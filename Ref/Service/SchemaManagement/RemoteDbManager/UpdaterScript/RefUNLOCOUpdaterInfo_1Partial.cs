using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefUNLOCOUpdaterInfo_1 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefUNLOCO",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefUNLOCOUtcOffsets", "RLO_RL_NKCode" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefUNLOCOUtcOffsets", new DataTableMapping { TableName = "#TempRefUNLOCOUtcOffset" }
				}
			}
		};
	}
}
