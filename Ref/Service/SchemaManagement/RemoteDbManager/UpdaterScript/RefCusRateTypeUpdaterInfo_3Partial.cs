using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefCusRateTypeUpdaterInfo_3: IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefCusRateType",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusRateTypeLanguages", "ZXT_ZZR_RateType" },
				{ "RefCusRateCodes", "ZY1_ZZR_RateType" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCusRateTypeLanguages", new DataTableMapping { TableName = "#TempRefCusRateTypeLanguage" }
				},
				{
					"RefCusRateCodes", new DataTableMapping {
						TableName = "#TempRefCusRateCode",
						RelatedFKColumnNames = new Dictionary<string, string>()
						{
							{ "RefCusRateCodeLanguages", "ZXC_ZY1_RateCode" }
						},
						RelatedTableNames = new Dictionary<string, DataTableMapping>()
						{
							{
								"RefCusRateCodeLanguages", new DataTableMapping { TableName = "#TempRefCusRateCodeLanguage" }
							}
						}
					}
				}
			}
		};
	}
}
