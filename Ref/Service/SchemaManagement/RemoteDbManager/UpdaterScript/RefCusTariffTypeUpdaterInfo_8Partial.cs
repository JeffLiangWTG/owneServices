using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefCusTariffTypeUpdaterInfo_8 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefCusTariffType",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusTariffTypeLanguages", "ZXK_ZZI_TariffType" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCusTariffTypeLanguages", new DataTableMapping { TableName = "#TempRefCusTariffTypeLanguage" }
				}
			}
		};
	}
}
