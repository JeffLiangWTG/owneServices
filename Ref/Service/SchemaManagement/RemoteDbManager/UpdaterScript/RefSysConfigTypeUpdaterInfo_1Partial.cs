using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefSysConfigTypeUpdaterInfo_1 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefSysConfigType",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefSysConfigs", "ZRC_ZRT_NKConfigCode" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefSysConfigs", new DataTableMapping { TableName = "#TempRefSysConfig" }
				}
			}
		};
	}
}
