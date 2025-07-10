using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public static class RefCusConditionMapping
	{
		public static DataTableMapping Mapping1 => new DataTableMapping
		{
			TableName = "#TempRefCusCondition",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusConditionType", "ZX1_ZX2_ConditionType" },
				{ "RefCusApplicabilities", "ZZT_ZX1_Conditions" },
				{ "RefCusConditionValues", "ZX3_ZX1_Condition" },
				{ "RefCusPreference", "ZX1_ZZS_Preference" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCusConditionType", new DataTableMapping { TableName = "#TempRefCusConditionType" }
				},
				{
					"RefCusApplicabilities", RefCusApplicabilityMapping.Mapping1
				},
				{
					"RefCusConditionValues", new DataTableMapping {
						TableName = "#TempRefCusConditionValue",
						RelatedFKColumnNames = new Dictionary<string, string>()
						{
							{ "RefCusConditionValueType", "ZX3_ZX4_ValueType" }
						},
						RelatedTableNames = new Dictionary<string, DataTableMapping>()
						{
							{
								"RefCusConditionValueType", new DataTableMapping { TableName = "#TempRefCusConditionValueType" }
							}
						}
					}
				},
				{
					"RefCusPreference", new DataTableMapping { TableName = "#TempRefCusPreference" }
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

		public static DataTableMapping Mapping3
		{
			get
			{
				var mapping = new DataTableMapping();
				mapping.TableName = Mapping2.TableName;
				mapping.RelatedFKColumnNames = Mapping2.RelatedFKColumnNames;
				mapping.RelatedFKColumnNames.Add("RefCusConditionLanguages", "ZXJ_ZX1_Condition");

				mapping.RelatedTableNames = Mapping2.RelatedTableNames;
				mapping.RelatedTableNames.Add("RefCusConditionLanguages", new DataTableMapping { TableName = "#TempRefCusConditionLanguage" });
				return mapping;
			}
		}
	}
}
