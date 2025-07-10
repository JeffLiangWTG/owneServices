using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefCusCodeTypeUpdaterInfo_1 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefCusCodeType",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusCodeTypeLanguages", "ZXI_ZZK_CodeType" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCusCodeTypeLanguages", new DataTableMapping { TableName = "#TempRefCusCodeTypeLanguage" }
				}
			}
		};
	}
}
