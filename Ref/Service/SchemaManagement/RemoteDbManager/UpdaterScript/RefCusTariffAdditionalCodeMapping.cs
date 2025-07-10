using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public static class RefCusTariffAdditionalCodeMapping
	{
		public static DataTableMapping Mapping1 => new DataTableMapping
		{
			TableName = "#TempRefCusTariffAdditionalCode",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusTariffAdditionalCodeLanguages", "ZY4_ZY2_TariffAdditionalCode" },
				{ "RefCusApplicabilities", "ZZT_ZY2_AdditionalCode" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCusTariffAdditionalCodeLanguages", new DataTableMapping { TableName = "#TempRefCusTariffAdditionalCodeLanguage" }
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
