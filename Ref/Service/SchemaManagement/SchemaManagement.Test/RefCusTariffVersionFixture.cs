using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefCusTariffVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var tariff = new RefCusTariff
			{
				ZZ1_PK = Guid.NewGuid(),
				ZZ1_TariffCode = "811010",
				ZZ1_Description = "Unwrought antimony; powders",
				ZZ1_StartDate = DateTime.Now.AddDays(-1),
				ZZ1_EndDate = DateTime.Now.AddDays(1),
				ZZ1_ZZF_NKTaxOrFeeCode = "VATA",
				ZZ1_ZZZ_NKDataGrouping = "ZA",
				ZZ1_ZZI_TariffType = Guid.Parse("9F27344A-EB2C-4252-B1FD-FEF676B4BE8F"),
				ZZ1_CompositeKeyOnZZ5 = string.Empty
			};
			result.Add(tariff);
			result.Add(new RefCusTariffLanguage
			{
				ZX7_PK = Guid.NewGuid(),
				ZX7_ZZ1_Tariff = tariff.ZZ1_PK,
				ZX7_ZX6_NKLanguage = "EN",
				ZX7_Description = "English"
			});
			result.Add(new RefCusTariffAttribute
			{
				ZZ3_PK = Guid.NewGuid(),
				ZZ3_ZZ1_Tariff = tariff.ZZ1_PK,
				ZZ3_Name = "CheckDigit",
				ZZ3_Value = "3",
				ZZ3_DataSetPK = tariff.ZZ1_PK
			});

			var rate1 = new RefCusRate
			{
				ZZ2_PK = Guid.NewGuid(),
				ZZ2_ZZ1_Tariff = tariff.ZZ1_PK,
				ZZ2_StartDate = DateTime.Now.AddDays(-1),
				ZZ2_EndDate = DateTime.Now.AddDays(1),
				ZZ2_RateFormula = "0.15 * VFD",
				ZZ2_SelectorFormula = "pp='EFTA'",
				ZZ2_ZZZ_NKDataGrouping = "ZA",
				ZZ2_RX_NKCurrencyOverride = "",
				ZZ2_DataSetPK = tariff.ZZ1_PK
			};
			result.Add(rate1);
			result.Add(new RefCusTariffUOM
			{
				ZZ8_PK = Guid.NewGuid(),
				ZZ8_ZZ1_Tariff = tariff.ZZ1_PK,
				ZZ8_Type = "RU1",
				ZZ8_UOM = "NO",
				ZZ8_ZZZ_NKDataGrouping = "ZA",
				ZZ8_DataSetPK = tariff.ZZ1_PK,
				ZZ8_DataSetCode = "ZZ1",
				ZZ8_ZZA_TradeGroup = tradeGroupGuid,
				ZZ8_ZZA_SecondTradeGroup = tradeGroupGuid1,
				ZZ8_EndDate = new DateTime(2079, 06, 06),
				ZZ8_StartDate = new DateTime(1900, 01, 01),
			});
			var tariffRelationship = new RefCusTariffRelationship
			{
				ZZH_PK = Guid.NewGuid(),
				ZZH_TariffCode = "95043010",
				ZZH_ZZ1_Tariff = tariff.ZZ1_PK,
				ZZH_ZZI_TariffType = Guid.Parse("9F27344A-EB2C-4252-B1FD-FEF676B4BE8F"),
				ZZH_DataSetPK = Guid.NewGuid(),
				ZZH_DataSetCode	= "ZZH",
			};
			result.Add(tariffRelationship);
			result.Add(new RefCusRateUOM
			{
				ZXG_PK = Guid.NewGuid(),
				ZXG_ZZ2_Rate = rate1.ZZ2_PK,
				ZXG_UOM = "LI",
				ZXG_DataSetPK = tariff.ZZ1_PK
			});
			var condition = new RefCusCondition
			{
				ZX1_PK = Guid.NewGuid(),
				ZX1_ZZ1_Tariff = tariff.ZZ1_PK,
				ZX1_ZX2_ConditionType = condTypeGuid,
				ZX1_ZZZ_NKDataGrouping = "ZA",
				ZX1_Comment = "A",
				ZX1_EndDate = new DateTime(2079, 06, 06),
				ZX1_StartDate = new DateTime(1900, 01, 01),
				ZX1_Source = "B",
				ZX1_DataSetPK = tariff.ZZ1_PK,
				ZX1_ZY7_NKConditionCode = "AAA",
				ZX1_AdditionalComment = "comment",
				ZX1_Severity = "MSG"
			};
			result.Add(condition);
			result.Add(new RefCusConditionValue
			{
				ZX3_LogicalORWithinGroup = 0,
				ZX3_PK = Guid.NewGuid(),
				ZX3_ZX1_Condition = condition.ZX1_PK,
				ZX3_Value = "T",
				ZX3_ZX4_ValueType = valueTypeGuid,
				ZX3_DataSetPK = tariff.ZZ1_PK
			});
			var applicability1 = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_AdditionalCode = "A",
				ZZT_EndDate = new DateTime(2079, 06, 06),
				ZZT_StartDate = new DateTime(1900, 01, 01),
				ZZT_OrderNumber = "1",
				ZZT_ZZA_TradeGroup = tradeGroupGuid,
				ZZT_ZX1_Conditions = condition.ZX1_PK,
				ZZT_DataSetPK = tariff.ZZ1_PK
			};
			result.Add(applicability1);
			result.Add(new RefCusExcludedTradeGroup
			{
				ZZC_PK = Guid.NewGuid(),
				ZZC_ZZA_TradeGroup = tradeGroupGuid,
				ZZC_ZZT_Applicability = applicability1.ZZT_PK,
				ZZC_DataSetPK = tariff.ZZ1_PK
			});
			var applicability2 = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_AdditionalCode = "B",
				ZZT_EndDate = new DateTime(2079, 06, 06),
				ZZT_StartDate = new DateTime(1900, 01, 01),
				ZZT_OrderNumber = "2",
				ZZT_ZZA_TradeGroup = tradeGroupGuid,
				ZZT_ZZ2_Rate = rate1.ZZ2_PK,
				ZZT_DataSetPK = tariff.ZZ1_PK
			};
			result.Add(applicability2);
			result.Add(new RefCusExcludedTradeGroup
			{
				ZZC_PK = Guid.NewGuid(),
				ZZC_ZZA_TradeGroup = tradeGroupGuid,
				ZZC_ZZT_Applicability = applicability2.ZZT_PK,
				ZZC_DataSetPK = tariff.ZZ1_PK
			});
			var nationalCode = new RefCusTariffNationalCode
			{
				ZZW_PK = Guid.NewGuid(),
				ZZW_Description = "A",
				ZZW_EndDate = new DateTime(2079, 06, 06),
				ZZW_StartDate = new DateTime(1900, 01, 01),
				ZZW_ZZ1_Tariff = tariff.ZZ1_PK,
				ZZW_ZZF_NKTaxOrFeeCode = "T",
				ZZW_NationalCode = "A",
				ZZW_ZZZ_NKDataGrouping = "ZA"
			};
			result.Add(nationalCode);
			var rate2 = new RefCusRate
			{
				ZZ2_PK = Guid.NewGuid(),
				ZZ2_ZZW_TariffNationalCode = nationalCode.ZZW_PK,
				ZZ2_StartDate = DateTime.Now.AddDays(-1),
				ZZ2_EndDate = DateTime.Now.AddDays(1),
				ZZ2_RateFormula = "0.15 * VFD",
				ZZ2_SelectorFormula = "pp='FREE'",
				ZZ2_ZZZ_NKDataGrouping = "ZA",
				ZZ2_RX_NKCurrencyOverride = "",
				ZZ2_DataSetPK = nationalCode.ZZW_ZZ1_Tariff
			};
			result.Add(rate2);
			result.Add(new RefCusTariffAttribute
			{
				ZZ3_PK = Guid.NewGuid(),
				ZZ3_ZZW_TariffNationalCode = nationalCode.ZZW_PK,
				ZZ3_Name = "CheckDigit",
				ZZ3_Value = "4",
				ZZ3_DataSetPK = nationalCode.ZZW_ZZ1_Tariff
			});
			var applicability3 = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_AdditionalCode = "C",
				ZZT_EndDate = new DateTime(2079, 06, 06),
				ZZT_StartDate = new DateTime(1900, 01, 01),
				ZZT_OrderNumber = "2",
				ZZT_ZZA_TradeGroup = tradeGroupGuid,
				ZZT_ZZ2_Rate = rate2.ZZ2_PK,
				ZZT_DataSetPK = nationalCode.ZZW_ZZ1_Tariff
			};
			result.Add(applicability3);
			result.Add(new RefCusExcludedTradeGroup
			{
				ZZC_PK = Guid.NewGuid(),
				ZZC_ZZA_TradeGroup = tradeGroupGuid,
				ZZC_ZZT_Applicability = applicability3.ZZT_PK,
				ZZC_DataSetPK = nationalCode.ZZW_ZZ1_Tariff
			});
			result.Add(new RefCusRateUOM
			{
				ZXG_PK = Guid.NewGuid(),
				ZXG_ZZ2_Rate = rate2.ZZ2_PK,
				ZXG_UOM = "KG",
				ZXG_DataSetPK = nationalCode.ZZW_ZZ1_Tariff
			});
			var additionalCode1 = new RefCusTariffAdditionalCode
			{
				ZY2_AdditionalCode = "AD",
				ZY2_Description = "DSC",
				ZY2_IsMandatory = true,
				ZY2_PK = Guid.NewGuid(),
				ZY2_ZY3_NKCategory = "CAT",
				ZY2_ZZ1_Tariff = tariff.ZZ1_PK,
				ZY2_ZZW_NationalCode = null,
				ZY2_ZZZ_NKDataGrouping = "ZA",
				ZY2_ParentAdditionalCode = "",
				ZY2_ZY3_NKParentCategory = "",
				ZY2_DataSetPK = tariff.ZZ1_PK
			};
			result.Add(additionalCode1);
			result.Add(new RefCusTariffAdditionalCodeLanguage
			{
				ZY4_PK = Guid.NewGuid(),
				ZY4_Description = "Desc",
				ZY4_ZX6_NKLanguage = "EN",
				ZY4_ZY2_TariffAdditionalCode = additionalCode1.ZY2_PK,
				ZY4_DataSetPK = tariff.ZZ1_PK
			});
			result.Add(new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_AdditionalCode = string.Empty,
				ZZT_EndDate = new DateTime(2079, 06, 06),
				ZZT_StartDate = new DateTime(1900, 01, 01),
				ZZT_OrderNumber = string.Empty,
				ZZT_ZZA_TradeGroup = tradeGroupGuid,
				ZZT_ZY2_AdditionalCode = additionalCode1.ZY2_PK,
				ZZT_DataSetPK = tariff.ZZ1_PK
			});
			var additionalCode2 = new RefCusTariffAdditionalCode
			{
				ZY2_AdditionalCode = "DA",
				ZY2_Description = "DSC",
				ZY2_IsMandatory = true,
				ZY2_PK = Guid.NewGuid(),
				ZY2_ZY3_NKCategory = "CAT",
				ZY2_ZZ1_Tariff = tariff.ZZ1_PK,
				ZY2_ZZW_NationalCode = null,
				ZY2_ZZZ_NKDataGrouping = "ZA",
				ZY2_ParentAdditionalCode = "AD",
				ZY2_ZY3_NKParentCategory = "CAT",
				ZY2_DataSetPK = tariff.ZZ1_PK
			};
			result.Add(additionalCode2);
			result.Add(new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_AdditionalCode = string.Empty,
				ZZT_EndDate = new DateTime(2079, 06, 06),
				ZZT_StartDate = new DateTime(1900, 01, 01),
				ZZT_OrderNumber = string.Empty,
				ZZT_ZZA_TradeGroup = tradeGroupGuid,
				ZZT_ZY2_AdditionalCode = additionalCode2.ZY2_PK,
				ZZT_DataSetPK = tariff.ZZ1_PK
			});
			var brCharacteristic = new RefCusTariffBRCharacteristic
			{
				ZB1_PK = Guid.NewGuid(),
				ZB1_CharacteristicType = "NVE",
				ZB1_ZZ1_Tariff = tariff.ZZ1_PK,
				ZB1_Style = "NUMBER",
				ZB1_MaxLength = 5,
				ZB1_DecimalPlaces =2,
				ZB1_Code = "BR1",
				ZB1_Text = "TXT",
				ZB1_StartDate = DateTime.Now.AddDays(-1),
				ZB1_EndDate = DateTime.Now,
				ZB1_IsImport = true,	
				ZB1_IsExport = true,	
				ZB1_IsMandatory = true,	
				ZB1_IsConditioningAttribute = true,	
			};
			result.Add(brCharacteristic);
			result.Add(new RefCusTariffBRCharacteristicAttribute
			{
				ZB3_PK = Guid.NewGuid(),	
				ZB3_ZB1_Characteristic = brCharacteristic.ZB1_PK,
				ZB3_Name = "Name",
				ZB3_Code = "CD",
				ZB3_Value = "Val"
			});
			result.Add(new RefCusTariffBRCharacteristicValue
			{
				ZB2_PK = Guid.NewGuid(),	
				ZB2_ZB1_Characteristic = brCharacteristic.ZB1_PK,
				ZB2_Value = "Val",
				ZB2_Description = "Des"
			});
			var applicability4 = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_AdditionalCode = "C",
				ZZT_EndDate = new DateTime(2079, 06, 06),
				ZZT_StartDate = new DateTime(1900, 01, 01),
				ZZT_OrderNumber = "2",
				ZZT_ZZA_TradeGroup = tradeGroupGuid,
				ZZT_ZZH_TariffRelationship = tariffRelationship.ZZH_PK,
				ZZT_DataSetPK = nationalCode.ZZW_ZZ1_Tariff
			};
			result.Add(applicability4);
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefCusTariff tariff)
			{
				tariff.ZZ1_Description = "XX";
			}
			if (data is RefCusTariffLanguage language)
			{
				language.ZX7_Description = "XX";
			}
			if (data is RefCusRate rate)
			{
				rate.ZZ2_RateFormula = "XX";
			}
			if (data is RefCusRateUOM rateUOM)
			{
				rateUOM.ZXG_UOM = "XX";
			}
			if (data is RefCusTariffAttribute tariffAttr)
			{
				tariffAttr.ZZ3_Value = "XX";
			}
			if (data is RefCusTariffUOM uom)
			{
				uom.ZZ8_UOM = "XX";
			}
			if (data is RefCusTariffRelationship relationship)
			{
				relationship.ZZH_TariffCode = "XX";
			}
			if (data is RefCusCondition condition)
			{
				condition.ZX1_Source = "D";
				condition.ZX1_Severity = "WAR";
			}
			if (data is RefCusApplicability applicability)
			{
				applicability.ZZT_EndDate = new DateTime(2030, 06, 06);
			}
			if (data is RefCusConditionValue value)
			{
				value.ZX3_Value = "S";
			}
			if (data is RefCusExcludedTradeGroup excluded)
			{
				return false;
			}
			if (data is RefCusTariffNationalCode national)
			{
				national.ZZW_Description = "XX";
			}
			if (data is RefCusVATApplicability vat)
			{
				vat.ZX5_Description = "XX";
			}
			if (data is RefCusTariffAdditionalCode addCode)
			{
				addCode.ZY2_Description = "XX";
			}
			if (data is RefCusTariffAdditionalCodeLanguage addCodeLanguage)
			{
				addCodeLanguage.ZY4_Description = "XX";
			}
			if (data is RefCusTariffBRCharacteristic brCharacteristic)
			{
				brCharacteristic.ZB1_Code = "BR2";
			}
			if (data is RefCusTariffBRCharacteristicAttribute brCharacteristicAttribute)
			{
				brCharacteristicAttribute.ZB3_Value = "XX";
			}
			if (data is RefCusTariffBRCharacteristicValue brCharacteristicValue)
			{
				brCharacteristicValue.ZB2_Value = "XX";
			}
			return true;
		}

		protected override object[] PrepareFKReferencedData()
		{
			return new object[] { new RefCusTariff
			{
				ZZ1_PK = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6"),
				ZZ1_TariffCode = "111111",
				ZZ1_Description = "Unwrought antimony; powders1",
				ZZ1_StartDate = DateTime.Now.AddDays(-2),
				ZZ1_EndDate = DateTime.Now.AddDays(2),
				ZZ1_ZZF_NKTaxOrFeeCode = "VATA",
				ZZ1_ZZZ_NKDataGrouping = "ZA",
				ZZ1_ZZI_TariffType = Guid.Parse("9F27344A-EB2C-4252-B1FD-FEF676B4BE8F"),
				ZZ1_CompositeKeyOnZZ5 = string.Empty
			} };
		}

		protected override bool UpdateFKColumnData(object data)
		{
			if (data is RefCusTariffNationalCode code)
			{
				code.ZZW_ZZ1_Tariff = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			if (data is RefCusTariffRelationship relationship)
			{
				relationship.ZZH_ZZ1_Tariff = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			if (data is RefCusTariffLanguage language)
			{
				language.ZX7_ZZ1_Tariff = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			if (data is RefCusTariffBRCharacteristic brCharacteristic)
			{
				brCharacteristic.ZB1_ZZ1_Tariff = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			return false;

		}

		Guid rateTypeGuid;
		Guid tradeGroupGuid;
		Guid tradeGroupGuid1;
		Guid condTypeGuid;
		Guid valueTypeGuid;

		protected override void PrepareDb(string dbName)
		{
			base.PrepareDb(dbName);
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				rateTypeGuid = Guid.NewGuid();
				var tariffTypeGuid = Guid.Parse("9F27344A-EB2C-4252-B1FD-FEF676B4BE8F");
				var rateType = context.RefCusRateTypes.Add(new RefCusRateType
				{
					ZZR_PK = rateTypeGuid,
					ZZR_RateType = "EXC",
					ZZR_Description = "Excise",
					ZZR_IsPayable = true,
					ZZR_ZZZ_NKDataGrouping = "ZA",
					ZZR_CustomsValueFormula = "",
					ZZR_RX_NKFormulaCurrency = string.Empty
				});
				context.RefCusTariffTypes.Add(new RefCusTariffType
				{
					ZZI_PK = tariffTypeGuid,
					ZZI_TariffType = "1P1",
					ZZI_Description = "Schedule 1 Part 1",
					ZZI_ZZZ_NKDataGrouping = "ZA",
					ZZI_HasFormulaSpecificQuestions = false,
					ZZI_ZZR_RateType = rateTypeGuid,
					ZZI_ZZ9_NKNomenclatureGroupType = "",
				});
				condTypeGuid = Guid.NewGuid();
				context.RefCusConditionTypes.Add(new RefCusConditionType
				{
					ZX2_PK = condTypeGuid,
					ZX2_ConditionClass = "RATE",
					ZX2_ConditionType = "B",
					ZX2_Description = "C",
					ZX2_ZZZ_NKDataGrouping = "ZA"
				});
				context.RefCusConditionCodes.Add(new RefCusConditionCode
				{
					ZY7_PK = Guid.NewGuid(),
					ZY7_ConditionCode = "AAA",
					ZY7_Description = "D",
					ZY7_ZZZ_NKDataGrouping = "ZA"
				});
				tradeGroupGuid = Guid.NewGuid();
				context.RefCusTradeGroups.Add(new RefCusTradeGroup
				{
					ZZA_PK = tradeGroupGuid,
					ZZA_Description = "A",
					ZZA_StartDate = new DateTime(1900, 01, 01),
					ZZA_EndDate = new DateTime(2079, 06, 06),
					ZZA_TradeGroup = "ZT",
					ZZA_ZZZ_NKDataGrouping = "ZA"
				});
				tradeGroupGuid1 = Guid.NewGuid();
				context.RefCusTradeGroups.Add(new RefCusTradeGroup
				{
					ZZA_PK = tradeGroupGuid1,
					ZZA_Description = "B",
					ZZA_StartDate = new DateTime(1900, 01, 01),
					ZZA_EndDate = new DateTime(2079, 06, 06),
					ZZA_TradeGroup = "ZT1",
					ZZA_ZZZ_NKDataGrouping = "ZA"
				});
				valueTypeGuid = Guid.NewGuid();
				context.RefCusConditionValueTypes.Add(new RefCusConditionValueType
				{
					ZX4_PK = valueTypeGuid,
					ZX4_Description = "A",
					ZX4_ValueType = "B",
					ZX4_ZZZ_NKDataGrouping = "ZA"
				});
				context.RefLanguageTypes.Add(new RefLanguageType
				{
					ZX6_PK = Guid.NewGuid(),
					ZX6_Description = "English",
					ZX6_Language = "EN"
				});
				context.RefCusTariffAdditionalCodeCategories.Add(new RefCusTariffAdditionalCodeCategory
				{
					ZY3_Category = "CAT",
					ZY3_Description = "Category",
					ZY3_PK = Guid.NewGuid(),
					ZY3_ZZZ_NKDataGrouping = "ZA"
				});
				context.SaveChanges();
				context.Database.ExecuteSqlRaw($"DELETE {nameof(RefDbVersionControl)}");
			}
		}
	}
}

