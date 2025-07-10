using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefCusConditionValueTypeUpdaterInfo_1 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefCusConditionValueType",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusConditionValueTypeLanguages", "ZXX_ZX4_ValueType" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCusConditionValueTypeLanguages", new DataTableMapping { TableName = "#TempRefCusConditionValueTypeLanguage" }
				}
			}
		};
	}
}
