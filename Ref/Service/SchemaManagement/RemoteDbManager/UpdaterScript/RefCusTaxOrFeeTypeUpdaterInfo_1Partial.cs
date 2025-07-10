using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefCusTaxOrFeeTypeUpdaterInfo_1 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefCusTaxOrFeeType",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusTaxOrFees", "ZZF_ZX0_NKTaxOrFeeType" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCusTaxOrFees", new DataTableMapping {
						TableName = "#TempRefCusTaxOrFee",
						RelatedFKColumnNames = new Dictionary<string, string>()
						{
							{ "RefCusTaxOrFeeLanguages", "ZXU_ZZF_TaxOrFee" }
						},
						RelatedTableNames = new Dictionary<string, DataTableMapping>()
						{
							{
								"RefCusTaxOrFeeLanguages", new DataTableMapping { TableName = "#TempRefCusTaxOrFeeLanguage" }
							}
						}
					}
				}
			}
		};
	}
}
