using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	[TestFixture]
	[TransactionedTestCase]
	public class InsertTriggersWithNomenclatureTariffAndPreferenceFixture
	{
		protected object[] PrepareData()
		{
			var result = new List<object>();

			var tariff = new RefCusTariff
			{
				ZZ1_PK = Guid.NewGuid(),
				ZZ1_TariffCode = "811010",
				ZZ1_Description = "Unwrought antimony; powders",
				ZZ1_StartDate = DateTime.Now.AddDays(-1),
				ZZ1_EndDate = DateTime.Now.AddDays(1),
				ZZ1_ZZF_NKTaxOrFeeCode = "VAT",
				ZZ1_ZZZ_NKDataGrouping = "ZA",
				ZZ1_ZZI_TariffType = tariffTypeGuid,
				ZZ1_CompositeKeyOnZZ5 = string.Empty
			};
			result.Add(tariff);
			var nomenclature = new RefCusNomenclatureGroup
			{
				ZZ5_PK = Guid.NewGuid(),
				ZZ5_ZZZ_NKDataGrouping = "ZA",
				ZZ5_Description = "Nomenclature Test",
				ZZ5_Value = "Value",
				ZZ5_StartDate = DateTime.Now.AddDays(-1),
				ZZ5_EndDate = DateTime.Now.AddDays(1),
				ZZ5_ZZ9_NKNomenclatureGroupType = "GT",
				ZZ5_CompositeKey = "01"
			};
			result.Add(nomenclature);
			var preference = new RefCusPreference
			{
				ZZS_PK = Guid.NewGuid(),
				ZZS_Description = "Pref",
				ZZS_Preference = "PRE",
				ZZS_ZZZ_NKDataGrouping = "ZA"
			};
			result.Add(preference);

			var conditionTariff = new RefCusCondition
			{
				ZX1_PK = Guid.NewGuid(),
				ZX1_Comment = "cmt",
				ZX1_ConditionValueTrueMeansStop = true,
				ZX1_IsExport = false,
				ZX1_IsImport = true,
				ZX1_StartDate = DateTime.Now.AddDays(-1),
				ZX1_EndDate = DateTime.Now.AddDays(1),
				ZX1_ZX2_ConditionType = condTypeGuid,
				ZX1_Source = "A",
				ZX1_ZZ1_Tariff = tariff.ZZ1_PK,
				ZX1_ZZS_Preference = preference.ZZS_PK,
				ZX1_ZZZ_NKDataGrouping = "ZA",
				ZX1_ZY7_NKConditionCode = "ABC",
				ZX1_AdditionalComment = "comment"
			};
			result.Add(conditionTariff);

			var conditionNomenclature = new RefCusCondition
			{
				ZX1_PK = Guid.NewGuid(),
				ZX1_Comment = "cmt",
				ZX1_ConditionValueTrueMeansStop = true,
				ZX1_IsExport = false,
				ZX1_IsImport = true,
				ZX1_StartDate = DateTime.Now.AddDays(-1),
				ZX1_EndDate = DateTime.Now.AddDays(1),
				ZX1_ZX2_ConditionType = condTypeGuid,
				ZX1_Source = "B",
				ZX1_ZZ5_Nomenclature = nomenclature.ZZ5_PK,
				ZX1_ZZS_Preference = preference.ZZS_PK,
				ZX1_ZZZ_NKDataGrouping = "ZA",
				ZX1_ZY7_NKConditionCode = "ABC",
				ZX1_AdditionalComment = "comment"
			};
			result.Add(conditionNomenclature);

			var conditionPreference = new RefCusCondition
			{
				ZX1_PK = Guid.NewGuid(),
				ZX1_Comment = "cmt",
				ZX1_ConditionValueTrueMeansStop = true,
				ZX1_IsExport = false,
				ZX1_IsImport = true,
				ZX1_StartDate = DateTime.Now.AddDays(-1),
				ZX1_EndDate = DateTime.Now.AddDays(1),
				ZX1_ZX2_ConditionType = condTypeGuid,
				ZX1_Source = "C",
				ZX1_ZZS_Preference = preference.ZZS_PK,
				ZX1_ZZZ_NKDataGrouping = "ZA",
				ZX1_ZY7_NKConditionCode = "ABC",
				ZX1_AdditionalComment = "comment"
			};
			result.Add(conditionPreference);

			var conditionValueTariff = new RefCusConditionValue
			{
				ZX3_PK = Guid.NewGuid(),
				ZX3_LogicalORWithinGroup = 0,
				ZX3_Value = "VAL",
				ZX3_ZX1_Condition = conditionTariff.ZX1_PK,
				ZX3_ZX4_ValueType = condValueTypeGuid
			};
			result.Add(conditionValueTariff);
			var conditionValueNomenclature = new RefCusConditionValue
			{
				ZX3_PK = Guid.NewGuid(),
				ZX3_LogicalORWithinGroup = 0,
				ZX3_Value = "VAL",
				ZX3_ZX1_Condition = conditionNomenclature.ZX1_PK,
				ZX3_ZX4_ValueType = condValueTypeGuid
			};
			result.Add(conditionValueNomenclature);
			var conditionValuePreference = new RefCusConditionValue
			{
				ZX3_PK = Guid.NewGuid(),
				ZX3_LogicalORWithinGroup = 0,
				ZX3_Value = "VAL",
				ZX3_ZX1_Condition = conditionPreference.ZX1_PK,
				ZX3_ZX4_ValueType = condValueTypeGuid
			};
			result.Add(conditionValuePreference);

			result.Add(new RefCusTariffAdditionalCodeCategory
			{
				ZY3_PK = Guid.NewGuid(),
				ZY3_Description = "D",
				ZY3_Category = "CAT",
				ZY3_ZZZ_NKDataGrouping = "ZA"
			});
			var tariffAdditionalCode = new RefCusTariffAdditionalCode
			{
				ZY2_PK = Guid.NewGuid(),
				ZY2_AdditionalCode = "A",
				ZY2_Description = "D",
				ZY2_IsMandatory = true,
				ZY2_ZY3_NKCategory = "CAT",
				ZY2_ZZ1_Tariff = tariff.ZZ1_PK,
				ZY2_ZZZ_NKDataGrouping = "ZA"
			};
			result.Add(tariffAdditionalCode);

			var applicabilityTariff = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_AdditionalCode = "A",
				ZZT_EndDate = new DateTime(2079, 06, 06),
				ZZT_StartDate = new DateTime(1900, 01, 01),
				ZZT_OrderNumber = "1",
				ZZT_ZZA_TradeGroup = tradeGroupGuid,
				ZZT_ZX1_Conditions = conditionTariff.ZX1_PK
			};
			result.Add(applicabilityTariff);
			var applicabilityNomenclature = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_AdditionalCode = "A",
				ZZT_EndDate = new DateTime(2079, 06, 06),
				ZZT_StartDate = new DateTime(1900, 01, 01),
				ZZT_OrderNumber = "1",
				ZZT_ZZA_TradeGroup = tradeGroupGuid,
				ZZT_ZX1_Conditions = conditionNomenclature.ZX1_PK
			};
			result.Add(applicabilityNomenclature);
			var applicabilityPreference = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_AdditionalCode = "A",
				ZZT_EndDate = new DateTime(2079, 06, 06),
				ZZT_StartDate = new DateTime(1900, 01, 01),
				ZZT_OrderNumber = "1",
				ZZT_ZZA_TradeGroup = tradeGroupGuid,
				ZZT_ZX1_Conditions = conditionPreference.ZX1_PK
			};
			result.Add(applicabilityPreference);
			var applicabilityTariffAdditionalCode = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_AdditionalCode = "",
				ZZT_EndDate = new DateTime(2079, 06, 06),
				ZZT_StartDate = new DateTime(1900, 01, 01),
				ZZT_OrderNumber = "",
				ZZT_ZZA_TradeGroup = tradeGroupGuid,
				ZZT_ZY2_AdditionalCode = tariffAdditionalCode.ZY2_PK
			};
			result.Add(applicabilityTariffAdditionalCode);

			result.Add(new RefCusExcludedTradeGroup
			{
				ZZC_PK = Guid.NewGuid(),
				ZZC_ZZA_TradeGroup = tradeGroupGuid,
				ZZC_ZZT_Applicability = applicabilityTariff.ZZT_PK,
			});
			result.Add(new RefCusExcludedTradeGroup
			{
				ZZC_PK = Guid.NewGuid(),
				ZZC_ZZA_TradeGroup = tradeGroupGuid,
				ZZC_ZZT_Applicability = applicabilityNomenclature.ZZT_PK,
			});
			result.Add(new RefCusExcludedTradeGroup
			{
				ZZC_PK = Guid.NewGuid(),
				ZZC_ZZA_TradeGroup = tradeGroupGuid,
				ZZC_ZZT_Applicability = applicabilityPreference.ZZT_PK,
			});

			return result.ToArray();
		}

		[Test]
		public void IsSettingValuesFollowingCorrectOrderOnTheTrigger()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			PrepareDb(dbName);
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				var dataSet = PrepareData();
				var lastUpdatedUTC = DateTime.MinValue;
				foreach (var data in dataSet)
				{
					context.Add(data);
					Thread.Sleep(100);
					context.SaveChanges();
				}
			}

			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				var conditionTariff = context.RefCusConditions
					.Include(x => x.RefCusConditionValues)
					.Include(x => x.RefCusApplicabilities).ThenInclude(x => x.RefCusExcludedTradeGroups)
					.FirstOrDefault(c => c.ZX1_ZZ1_Tariff != null && c.ZX1_ZZ5_Nomenclature == null);
				Assert.NotNull(conditionTariff);
				Assert.AreEqual("ZZ1", conditionTariff.ZX1_DataSetCode);
				Assert.NotNull(conditionTariff.ZX1_ZZ1_Tariff);
				Assert.Null(conditionTariff.ZX1_ZZ5_Nomenclature);
				Assert.NotNull(conditionTariff.ZX1_ZZS_Preference);
				Assert.NotNull(conditionTariff.RefCusConditionValues);
				Assert.AreEqual("ZZ1", conditionTariff.RefCusConditionValues.First().ZX3_DataSetCode);
				Assert.NotNull(conditionTariff.RefCusApplicabilities);
				Assert.AreEqual("ZZ1", conditionTariff.RefCusApplicabilities.First().ZZT_DataSetCode);
				Assert.NotNull(conditionTariff.RefCusApplicabilities.First().RefCusExcludedTradeGroups);
				Assert.AreEqual(conditionTariff.ZX1_ZZ1_Tariff, conditionTariff.RefCusApplicabilities.First().ZZT_DataSetPK);
				Assert.NotNull(conditionTariff.RefCusApplicabilities.First().RefCusExcludedTradeGroups);
				Assert.AreEqual("ZZ1", conditionTariff.RefCusApplicabilities.First().RefCusExcludedTradeGroups.First().ZZC_DataSetCode);
				Assert.AreEqual(conditionTariff.ZX1_ZZ1_Tariff, conditionTariff.RefCusApplicabilities.First().RefCusExcludedTradeGroups.First().ZZC_DataSetPK);

				var conditionNomenclature = context.RefCusConditions
					.Include(x => x.RefCusConditionValues)
					.Include(x=>x.RefCusApplicabilities).ThenInclude(x => x.RefCusExcludedTradeGroups)
					.FirstOrDefault(c => c.ZX1_ZZ1_Tariff == null && c.ZX1_ZZ5_Nomenclature != null);
				Assert.NotNull(conditionNomenclature);
				Assert.AreEqual("ZZ5", conditionNomenclature.ZX1_DataSetCode);
				Assert.Null(conditionNomenclature.ZX1_ZZ1_Tariff);
				Assert.NotNull(conditionNomenclature.ZX1_ZZ5_Nomenclature);
				Assert.NotNull(conditionNomenclature.ZX1_ZZS_Preference);
				Assert.NotNull(conditionNomenclature.RefCusConditionValues);
				Assert.AreEqual("ZZ5", conditionNomenclature.RefCusConditionValues.First().ZX3_DataSetCode);
				Assert.NotNull(conditionNomenclature.RefCusApplicabilities);
				Assert.AreEqual("ZZ5", conditionNomenclature.RefCusApplicabilities.First().ZZT_DataSetCode);
				Assert.AreEqual(conditionNomenclature.ZX1_ZZ5_Nomenclature, conditionNomenclature.RefCusApplicabilities.First().ZZT_DataSetPK);
				Assert.NotNull(conditionNomenclature.RefCusApplicabilities.First().RefCusExcludedTradeGroups);
				Assert.AreEqual("ZZ5", conditionNomenclature.RefCusApplicabilities.First().RefCusExcludedTradeGroups.First().ZZC_DataSetCode);
				Assert.AreEqual(conditionNomenclature.ZX1_ZZ5_Nomenclature, conditionNomenclature.RefCusApplicabilities.First().RefCusExcludedTradeGroups.First().ZZC_DataSetPK);

				var conditionPreference = context.RefCusConditions
					.Include(x => x.RefCusConditionValues)
					.Include(x => x.RefCusApplicabilities).ThenInclude(x => x.RefCusExcludedTradeGroups)
					.FirstOrDefault(c => c.ZX1_ZZ1_Tariff == null && c.ZX1_ZZ5_Nomenclature == null && c.ZX1_ZZS_Preference != null);
				Assert.NotNull(conditionPreference);
				Assert.AreEqual("ZZS", conditionPreference.ZX1_DataSetCode);
				Assert.Null(conditionPreference.ZX1_ZZ1_Tariff);
				Assert.Null(conditionPreference.ZX1_ZZ5_Nomenclature);
				Assert.NotNull(conditionPreference.ZX1_ZZS_Preference);
				Assert.NotNull(conditionPreference.RefCusConditionValues);
				Assert.AreEqual("ZZS", conditionPreference.RefCusConditionValues.First().ZX3_DataSetCode);
				Assert.AreEqual(conditionPreference.ZX1_ZZS_Preference, conditionPreference.RefCusConditionValues.First().ZX3_DataSetPK);
				Assert.NotNull(conditionPreference.RefCusApplicabilities);
				Assert.AreEqual("ZZS", conditionPreference.RefCusApplicabilities.First().ZZT_DataSetCode);
				Assert.AreEqual(conditionPreference.ZX1_ZZS_Preference, conditionPreference.RefCusApplicabilities.First().ZZT_DataSetPK);
				Assert.NotNull(conditionPreference.RefCusApplicabilities.First().RefCusExcludedTradeGroups);
				Assert.AreEqual("ZZS", conditionPreference.RefCusApplicabilities.First().RefCusExcludedTradeGroups.First().ZZC_DataSetCode);
				Assert.AreEqual(conditionPreference.ZX1_ZZS_Preference, conditionPreference.RefCusApplicabilities.First().RefCusExcludedTradeGroups.First().ZZC_DataSetPK);

				var additionalCode = context.RefCusTariffAdditionalCodes.FirstOrDefault(x => x.ZY2_ZZ1_Tariff != null);
				Assert.NotNull(additionalCode);
				Assert.AreEqual("ZZ1", additionalCode.ZY2_DataSetCode);
				var additionalCodeApplicability = context.RefCusApplicabilities.FirstOrDefault(x => x.ZZT_ZY2_AdditionalCode == additionalCode.ZY2_PK);
				Assert.NotNull(additionalCodeApplicability);
				Assert.AreEqual("ZZ1", additionalCodeApplicability.ZZT_DataSetCode);
				Assert.AreEqual(additionalCode.ZY2_ZZ1_Tariff, additionalCodeApplicability.ZZT_DataSetPK);
			}
		}
		Guid rateTypeGuid;
		Guid tariffTypeGuid;
		Guid nomenclatureGroupTypeGuid;
		Guid condTypeGuid;
		Guid condValueTypeGuid;
		Guid tradeGroupGuid;

		protected void PrepareDb(string dbName)
		{
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				rateTypeGuid = Guid.NewGuid();
				tariffTypeGuid = Guid.NewGuid();
				nomenclatureGroupTypeGuid = Guid.NewGuid();
				condTypeGuid = Guid.NewGuid();
				condValueTypeGuid = Guid.NewGuid();
				tradeGroupGuid = Guid.NewGuid();

				context.RefDataGroupings.Add(new RefDataGrouping
				{
					ZZZ_PK = Guid.NewGuid(),
					ZZZ_DataGrouping = "ZA",
					ZZZ_Description = "South Africar"
				});
				context.SaveChanges();
				context.RefCusRateTypes.Add(new RefCusRateType
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
				context.RefCusNomenclatureGroupTypes.Add(new RefCusNomenclatureGroupType
				{
					ZZ9_PK = nomenclatureGroupTypeGuid,
					ZZ9_Description = "type",
					ZZ9_GroupType = "GT"
				});
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
					ZY7_ConditionCode = "ABC",
					ZY7_Description = "D",
					ZY7_ZZZ_NKDataGrouping = "ZA"
				});
				context.RefCusConditionValueTypes.Add(new RefCusConditionValueType
				{
					ZX4_PK = condValueTypeGuid,
					ZX4_Description = "DESC",
					ZX4_IsFormula = false,
					ZX4_ValueType = "A",
					ZX4_ZZZ_NKDataGrouping = "ZA"
				});
				context.RefCusTradeGroups.Add(new RefCusTradeGroup
				{
					ZZA_PK = tradeGroupGuid,
					ZZA_Description = "A",
					ZZA_StartDate = new DateTime(1900, 01, 01),
					ZZA_EndDate = new DateTime(2079, 06, 06),
					ZZA_TradeGroup = "ZT",
					ZZA_ZZZ_NKDataGrouping = "ZA"
				});
				context.SaveChanges();
				context.Database.ExecuteSqlRaw($"DELETE {nameof(RefDbVersionControl)}");
			}
		}
	}
}
