using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public static class RefCusTariffNationalCodeMapping
	{
		public static DataTableMapping Mapping1 => new DataTableMapping
		{
			TableName = "#TempRefCusTariffNationalCode",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusRates", "ZZ2_ZZW_TariffNationalCode" },
				{ "RefCusTariffAttributes", "ZZ3_ZZW_TariffNationalCode" },
				{ "RefCusVATApplicabilities", "ZX5_ZZW_TariffNationalCode" },
				{ "RefCusTariffUOMs", "ZZ8_ZZW_TariffNationalCode" },
				{ "RefCusTariffAdditionalCodes", "ZY2_ZZW_NationalCode" },
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCusRates", RefCusRateMapping.Mapping1
				},
				{
					"RefCusTariffAttributes", new DataTableMapping { TableName = "#TempRefCusTariffAttribute" }
				},
				{
					"RefCusVATApplicabilities", RefCusVATApplicabilityMapping.Mapping
				},
				{
					"RefCusTariffUOMs", RefCusTariffUOMMapping.Mapping1
				},
				{
					"RefCusTariffAdditionalCodes", RefCusTariffAdditionalCodeMapping.Mapping1
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
				mapping.RelatedTableNames.Remove("RefCusRates");
				mapping.RelatedTableNames.Add("RefCusRates", RefCusRateMapping.Mapping2);
				mapping.RelatedTableNames.Remove("RefCusTariffAdditionalCodes");
				mapping.RelatedTableNames.Add("RefCusTariffAdditionalCodes", RefCusTariffAdditionalCodeMapping.Mapping2);
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

				mapping.RelatedTableNames = Mapping2.RelatedTableNames;
				mapping.RelatedTableNames.Remove("RefCusTariffUOMs");
				mapping.RelatedTableNames.Add("RefCusTariffUOMs", RefCusTariffUOMMapping.Mapping2);
				return mapping;
			}
		}
	}
}
