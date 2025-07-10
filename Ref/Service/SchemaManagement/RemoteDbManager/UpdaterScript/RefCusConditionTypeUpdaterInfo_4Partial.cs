using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefCusConditionTypeUpdaterInfo_4 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefCusConditionType",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusConditionTypeLanguages", "ZXW_ZX2_ConditionType" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCusConditionTypeLanguages", new DataTableMapping { TableName = "#TempRefCusConditionTypeLanguage" }
				}
			}
		};
	}
}
