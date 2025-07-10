using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefCusCodeListUpdaterInfo_2 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefCusCodeList",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusCodeListLanguages", "ZXA_ZZD_CodeList" },
				{ "RefCusCodeOrAttributeTransportModes", "ZZU_ZZD_CodeList" },
				{ "RefCusCodeListAttributes", "ZZE_ZZD_CodeList" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCusCodeListLanguages", new DataTableMapping { TableName = "#TempRefCusCodeListLanguage" }
				},
				{
					"RefCusCodeOrAttributeTransportModes", new DataTableMapping { TableName = "#TempRefCusCodeOrAttributeTransportMode" }
				},
				{
					"RefCusCodeListAttributes", new DataTableMapping {
						TableName = "#TempRefCusCodeListAttribute",
						RelatedFKColumnNames = new Dictionary<string, string>()
						{
							{ "RefCusCodeOrAttributeTransportModes", "ZZU_ZZE_Attribute" }
						},
						RelatedTableNames = new Dictionary<string, DataTableMapping>()
						{
							{
								"RefCusCodeOrAttributeTransportModes", new DataTableMapping { TableName = "#TempRefCusCodeOrAttributeTransportMode" }
							}
						}
					}
				}
			}
		};
	}
}
