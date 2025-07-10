using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public static class RefCusRateMapping
	{
		public static DataTableMapping Mapping1 => new DataTableMapping
		{
			TableName = "#TempRefCusRate",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusRateUOMs", "ZXG_ZZ2_Rate" },
				{ "RefCusRateCode", "ZZ2_ZY1_RateCode" },
				{ "RefCusPreference", "ZZ2_ZZS_Preference" },
				{ "RefCusApplicabilities", "ZZT_ZZ2_Rate" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCusRateUOMs", new DataTableMapping { TableName = "#TempRefCusRateUOM" }
				},
				{
					"RefCusRateCode", new DataTableMapping {
						TableName = "#TempRefCusRateCode",
						RelatedFKColumnNames = new Dictionary<string, string>()
						{
							{ "RefCusRateType", "ZY1_ZZR_RateType" }
						},
						RelatedTableNames = new Dictionary<string, DataTableMapping>()
						{
							{
								"RefCusRateType", new DataTableMapping { TableName = "#TempRefCusRateType" }
							}
						}
					}
				},
				{
					"RefCusPreference", new DataTableMapping { TableName = "#TempRefCusPreference" }
				},
				{
					"RefCusApplicabilities", RefCusApplicabilityMapping.Mapping1
				}
			}
		};

		public static DataTableMapping Mapping2
		{
			get
			{
				var mapping = new DataTableMapping();
				mapping.TableName = Mapping1.TableName;
				mapping.RelatedFKColumnNames = Mapping1.RelatedFKColumnNames;

				mapping.RelatedTableNames = Mapping1.RelatedTableNames;
				mapping.RelatedTableNames.Remove("RefCusApplicabilities");
				mapping.RelatedTableNames.Add("RefCusApplicabilities", RefCusApplicabilityMapping.Mapping2);
				return mapping;
			}
		}
	}
}
