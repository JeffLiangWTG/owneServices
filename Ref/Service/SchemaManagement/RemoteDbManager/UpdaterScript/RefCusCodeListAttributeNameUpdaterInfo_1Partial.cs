using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefCusCodeListAttributeNameUpdaterInfo_1 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefCusCodeListAttributeName",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusCodeListAttributeNameLanguages", "ZXH_ZXE_CodeListAttributeName" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCusCodeListAttributeNameLanguages", new DataTableMapping { TableName = "#TempRefCusCodeListAttributeNameLanguage" }
				}
			}
		};
	}
}
