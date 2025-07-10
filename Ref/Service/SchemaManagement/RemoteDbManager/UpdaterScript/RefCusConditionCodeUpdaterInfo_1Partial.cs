using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefCusConditionCodeUpdaterInfo_1 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefCusConditionCode",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusConditionCodeLanguages", "ZY8_ZY7_ConditionCode" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCusConditionCodeLanguages", new DataTableMapping { TableName = "#TempRefCusConditionCodeLanguage" }
				}
			}
		};
	}
}
