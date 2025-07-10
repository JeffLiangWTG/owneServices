using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefCusTariffUpdaterInfo_7 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefCusTariff",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusTariffType", "ZZ1_ZZI_TariffType" },
				{ "RefCusTariffAttributes", "ZZ3_ZZ1_Tariff" },
				{ "RefCusTariffUOMs", "ZZ8_ZZ1_Tariff" },
				{ "RefCusTariffRelationships", "ZZH_ZZ1_Tariff" },
				{ "RefCusRates", "ZZ2_ZZ1_Tariff" },
				{ "RefCusVATApplicabilities", "ZX5_ZZ1_Tariff" },
				{ "RefCusTariffNationalCodes", "ZZW_ZZ1_Tariff" },
				{ "RefCusTariffLanguages", "ZX7_ZZ1_Tariff" },
				{ "RefCusTariffAdditionalCodes", "ZY2_ZZ1_Tariff" },
				{ "RefCusConditions", "ZX1_ZZ1_Tariff" },
				{ "RefCusTariffBRCharacteristics" , "ZB1_ZZ1_Tariff" },
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCusTariffType", new DataTableMapping { TableName = "#TempRefCusTariffType" }
				},
				{
					"RefCusTariffAttributes",  new DataTableMapping { TableName = "#TempRefCusTariffAttribute" }
				},
				{
					"RefCusTariffUOMs", RefCusTariffUOMMapping.Mapping1
				},
				{
					"RefCusTariffRelationships", RefCusTariffRelationshipMapping.Mapping
				},
				{
					"RefCusRates", RefCusRateMapping.Mapping2
				},
				{
					"RefCusVATApplicabilities", RefCusVATApplicabilityMapping.Mapping
				},
				{
					"RefCusTariffNationalCodes", RefCusTariffNationalCodeMapping.Mapping2
				},
				{
					"RefCusTariffLanguages", new DataTableMapping { TableName = "#TempRefCusTariffLanguage" }
				},
				{
					"RefCusTariffAdditionalCodes", RefCusTariffAdditionalCodeMapping.Mapping2
				},
				{
					"RefCusConditions", RefCusConditionMapping.Mapping3
				},
				{
					"RefCusTariffBRCharacteristics", RefCusTariffBRCharacteristicMapping.Mapping
				},

			}
		};
	}
}
