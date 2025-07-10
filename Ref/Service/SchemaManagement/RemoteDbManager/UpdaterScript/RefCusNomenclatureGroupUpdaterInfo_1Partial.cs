using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefCusNomenclatureGroupUpdaterInfo_1 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefCusNomenclatureGroup",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusNomenclatureGroupNotes", "ZZL_ZZ5_NomenclatureGroup" },
				{ "RefCusNomenclatureLanguages", "ZX8_ZZ5_NomenclatureGroup" },
				{ "RefCusConditions", "ZX1_ZZ5_Nomenclature" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCusNomenclatureGroupNotes", new DataTableMapping { TableName = "#TempRefCusNomenclatureGroupNote" }
				},
				{
					"RefCusNomenclatureLanguages", new DataTableMapping { TableName = "#TempRefCusNomenclatureLanguage" }
				},
				{
					"RefCusConditions", RefCusConditionMapping.Mapping1
				}
			}
		};
	}
}
