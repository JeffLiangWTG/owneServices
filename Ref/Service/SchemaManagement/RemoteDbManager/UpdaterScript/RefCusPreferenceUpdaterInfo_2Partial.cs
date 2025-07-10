using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefCusPreferenceUpdaterInfo_2 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefCusPreference",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusPreferenceLanguages", "ZX9_ZZS_Preference" },
				{ "RefCusConditions", "ZX1_ZZS_Preference" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCusPreferenceLanguages", new DataTableMapping { TableName = "#TempRefCusPreferenceLanguage" }
				},
				{
					"RefCusConditions", RefCusConditionMapping.Mapping2
				}
			}
		};
	}
}
