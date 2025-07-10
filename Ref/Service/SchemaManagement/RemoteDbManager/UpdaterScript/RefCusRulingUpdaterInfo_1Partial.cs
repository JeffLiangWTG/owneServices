using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefCusRulingUpdaterInfo_1 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefCusRuling",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusRulingConfigs", "ZZY_ZZX_CusRuling" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCusRulingConfigs", new DataTableMapping { TableName = "#TempRefCusRulingConfig" }
				}
			}
		};
	}
}
