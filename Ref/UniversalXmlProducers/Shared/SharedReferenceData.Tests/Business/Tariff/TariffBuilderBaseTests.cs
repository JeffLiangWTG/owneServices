using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using CargoWise.RefDbRepo.SharedReferenceData.Tests;
using Castle.Components.DictionaryAdapter;
using Moq;
using NUnit.Framework;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Common.CommonHelper;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Tests
{
	[TestFixture]
	class TariffBuilderBaseTests
	{
		[Test]
		public void ConvertModelsToRefModel()
		{
			var measure1 = new Measure
			{
				ItemId = "1234560000",
				CleanId = "123456",
				StartDate = new DateTime(2019, 10, 01, 1, 1, 1),
				EndDate = new DateTime(2020, 1, 1, 1, 1, 1),
				NomenclatureStartDate = new DateTime(2019, 10, 01, 15, 14, 13),
				NomenclatureEndDate = new DateTime(2019, 12, 31, 23, 59, 59),
				Description = "Test Model Convertion 1.1",
				CompositeKey = "03.12.34",
				OrderNumber = "ORD12345",
				AdditionalCode = "123",
				AdditionalCodeType = "4",
				GeographicalArea = "1001",
				Formula = "RateFormula",
				MeasureType = "103",
				MeasureTypeSeries = "C",
				MeasureTypeDescription = "Third country duty",
				RateType = "DTY",
				RateCode = "A00",
				Preferences = new List<string> { "100" },
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				TariffTypes = new List<string> { "IMP" },
			}.SetComponents(new List<MeasureComponent>
			{
				new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "1", DutyExpression = "01", HJID = "1" },
				new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "2", DutyExpression = "99", HJID = "2" }
			})
			.SetConditions(new[]
			{
				new MeasureCondition
				{
					MeasurementUnit = "UOM",
					MeasurementUnitQualifier = "2",
					ConditionCode = "A",
					ConditionCodeDescription = "Condition Code Type A",
					MeasureAction = "01",
					IsRateFormulaCondition = true,
					HJID = "5"
				}
				.SetComponents(new[]
				{
					new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "2", DutyExpression = "01", HJID = "3" },
					new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "3", DutyExpression = "02", HJID = "4" }
				}),
				new MeasureCondition
				{
					ConditionCode = "B",
					ConditionCodeDescription = "Condition Code Type B",
					MeasureAction = "99",
					CertificateCode = "999",
					CertificateTypeCode = "X",
					HJID = "6"
				},
				new MeasureCondition
				{
					ConditionCode = "B",
					ConditionCodeDescription = "Condition Code Type B",
					MeasureAction = "99",
					CertificateCode = "888",
					CertificateTypeCode = "Y",
					HJID = "7"
				}
			})
			.SetExcludedGeographicalAreas(new[]
			{
				new ExcludedGeographicalArea{Value = "EX1", HJID = "1" },
				new ExcludedGeographicalArea{Value = "EX2", HJID = "2" },
			});

			var measure2 = new Measure // previous version of #1
			{
				ItemId = "1234560000",
				CleanId = "123456",
				StartDate = new DateTime(2019, 09, 01),
				EndDate = new DateTime(2019, 09, 01, 15, 14, 13),
				NomenclatureStartDate = new DateTime(2019, 09, 01),
				NomenclatureEndDate = new DateTime(2019, 09, 01, 15, 14, 13),
				Description = "Test Model Convertion 1.2",
				CompositeKey = "03.12.34",
				MeasureType = "103",
				MeasureTypeSeries = "A",
				MeasureTypeDescription = "Third country duty",
				RateType = "DTY",
				Preferences = new List<string> { "100" },
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				TariffTypes = new List<string> { "IMP" },
			};

			var measure3 = new Measure // combine as variant of #1 - Extend Dates
			{
				ItemId = "1234560000",
				CleanId = "123456",
				StartDate = new DateTime(2019, 09, 30, 01, 02, 03),
				EndDate = new DateTime(2020, 01, 02, 03, 04, 05),
				NomenclatureStartDate = new DateTime(2019, 09, 30, 01, 02, 03),
				NomenclatureEndDate = new DateTime(2020, 01, 02, 03, 04, 05),
				Description = "Test Model Convertion 1.3",
				CompositeKey = "03.12.34",
				GeographicalArea = "2222",
				MeasureType = "103",
				MeasureTypeSeries = "A",
				RateType = "DTY",
				Preferences = new List<string> { "100" },
				MeasureTypeDescription = "Third country duty",
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				TariffTypes = new List<string> { "IMP" },
				Formula = "0",
			}
			.SetComponents(new List<MeasureComponent>
			{
				new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "3", DutyExpression = "99", HJID = "8" }
			});

			var measure4 = new Measure // different tariff
			{
				ItemId = "2020200000",
				CleanId = "202020",
				StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
				EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
				NomenclatureStartDate = new DateTime(2019, 10, 01, 15, 14, 13),
				NomenclatureEndDate = new DateTime(2019, 12, 31, 23, 59, 59),
				Description = "Test Model Convertion 2.1",
				CompositeKey = "04.20.20",
				MeasureType = "103",
				MeasureTypeSeries = "A",
				MeasureTypeDescription = "Third country duty",
				RateType = "DTY",
				Preferences = new List<string> { "100" },
				ConditionClass = MeasureHelper.ConditionClass.Class,
				Formula = "ConditionFormula",
				TariffTypes = new List<string> { "EXP" },
			}
			.SetConditions(new[]
			{
				new MeasureCondition
				{
					MeasurementUnit = "UOM",
					MeasurementUnitQualifier = "2",
					ConditionCode = "A",
					ConditionCodeDescription = "Condition Code Type A",
					MeasureAction = "55",
					HJID = "9"
				}
			});

			var measure6 = new Measure //Expiring Test base
			{
				ItemId = "3333000000",
				CleanId = "3333",
				Description = "No Dates",
				CompositeKey = "05.33.33",
				GeographicalArea = "333",
				MeasureType = "103",
				MeasureTypeSeries = "A",
				MeasureTypeDescription = "Third country duty",
				RateType = "DTY",
				Preferences = new List<string> { "100" },
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				Formula = "0",
				TariffTypes = new List<string> { "IMP" },
			}
			.SetConditions(new[]
			{
				new MeasureCondition
				{
					ConditionCode = "B",
					ConditionCodeDescription = "Condition Code Type B",
					MeasureAction = "99",
					CertificateCode = "999",
					CertificateTypeCode = "X",
					HJID = "10"
				}
			})
			.SetComponents(new[]
			{
				new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "4", DutyExpression = "01", HJID = "11" }
			});

			var measure7 = new Measure
			{
				ItemId = "0404040404",
				CleanId = "0404040404",
				Description = "Control Formula Test",
				CompositeKey = "04.04.04",
				GeographicalArea = "123",
				MeasureType = "755",
				MeasureTypeSeries = "A",
				MeasureTypeDescription = "Condition formula test",
				RateType = "",
				Preferences = new List<string> { null },
				ConditionClass = MeasureHelper.ConditionClass.Control,
				Formula = "",
				TariffTypes = new List<string> { "IMP" },
			}
			.SetConditions(new[]
			{
				new MeasureCondition
				{
					ConditionCode = "B",
					ConditionCodeDescription = "Condition Code Type B",
					MeasureAction = "99",
					CertificateCode = "999",
					CertificateTypeCode = "X",
					HJID = "12"
				},
				new MeasureCondition
				{
					ConditionCode = "B",
					ConditionCodeDescription = "Condition Code Type B",
					MeasureAction = "29",
					DutyAmount = 123.45m,
					MeasurementUnit = "KGM",
					MeasurementUnitQualifier = "",
					IsRateFormulaCondition = false,
					Formula = "[KGM] <= 123.45",
					HJID = "13"
				}
			});

			var tariffModels = new List<ITariffModel>() { measure1, measure2, measure3, measure4, measure6, measure7 };

			var refModels = builder.ConvertToRefModels(tariffModels);
			Assert.That(refModels.Count, Is.EqualTo(8));

			var refModel = refModels.OrderByDescending(x => x.ZZ1_StartDate).First(x => x.ZZ1_TariffCode == "1234560000");

			Assert.That(refModel, Is.Not.Null);
			Assert.That(refModel.ZZ1_TariffCode, Is.EqualTo("1234560000"));
			Assert.That(refModel.ZZ1_Description, Is.EqualTo(measure1.Description));
			Assert.That(refModel.ZZ1_CompositeKeyOnZZ5, Is.EqualTo("03.12.34"));
			Assert.That(refModel.ZZ1_ZZZ_NKDataGrouping, Is.EqualTo("ABC"));
			Assert.That(refModel.ZZ1_StartDate, Is.EqualTo(measure2.NomenclatureStartDate.Value));
			Assert.That(refModel.ZZ1_EndDate, Is.EqualTo(new DateTime(2020, 1, 2, 3, 4, 0)));
			Assert.That(refModel.ZZ1_ZZI_NKTariffType, Is.EqualTo("IMP"));

			Assert.That(refModel.RefCusRates, Is.Not.Null);
			Assert.That(refModel.RefCusRates.Count, Is.EqualTo(2));

			var refCusRate = refModel.RefCusRates.SingleOrDefault(x => x.ZZ2_RateFormula == "RateFormula");

			Assert.That(refCusRate, Is.Not.Null);
			Assert.That(refCusRate.ZZ2_StartDate, Is.EqualTo(DefaultValues.MinimumDateTime));
			Assert.That(refCusRate.ZZ2_EndDate, Is.EqualTo(DefaultValues.MaximumDateTime));
			Assert.That(refCusRate.ZZ2_ZZS_NKPreference, Is.EqualTo("100"));
			Assert.That(refCusRate.ZZ2_ZY1_NKRateCode, Is.EqualTo("A00"));
			Assert.That(refCusRate.ZZ2_ZY1_ZZR_NKRateType, Is.EqualTo("DTY"));

			Assert.That(refCusRate.RefCusApplicabilities, Is.Not.Null);
			Assert.That(refCusRate.RefCusApplicabilities.Count, Is.EqualTo(1));
			Assert.That(refCusRate.RefCusApplicabilities[0].ZZT_AdditionalCode, Is.EqualTo("4123"));
			Assert.That(refCusRate.RefCusApplicabilities[0].ZZT_OrderNumber, Is.EqualTo("ORD12345"));
			Assert.That(refCusRate.RefCusApplicabilities[0].ZZT_StartDate, Is.EqualTo(new DateTime(2019, 10, 1, 1, 1, 0)));
			Assert.That(refCusRate.RefCusApplicabilities[0].ZZT_EndDate, Is.EqualTo(new DateTime(2020, 1, 1, 1, 1, 0)));

			Assert.That(refCusRate.RefCusApplicabilities[0].RefCusExcludedTradeGroups, Is.Not.Null);
			Assert.That(refCusRate.RefCusApplicabilities[0].RefCusExcludedTradeGroups.Count, Is.EqualTo(2));
			Assert.That(refCusRate.RefCusApplicabilities[0].RefCusExcludedTradeGroups[0].ZZC_ZZA_NKTradeGroup, Is.EqualTo("EX1"));
			Assert.That(refCusRate.RefCusApplicabilities[0].RefCusExcludedTradeGroups[1].ZZC_ZZA_NKTradeGroup, Is.EqualTo("EX2"));

			Assert.That(refCusRate.RefCusRateUOMs, Is.Not.Null);
			Assert.That(refCusRate.RefCusRateUOMs.Count, Is.EqualTo(3));
			Assert.That(refCusRate.RefCusRateUOMs.Any(x => x.ZXG_UOM == "UOM1"));
			Assert.That(refCusRate.RefCusRateUOMs.Any(x => x.ZXG_UOM == "UOM2"));
			Assert.That(refCusRate.RefCusRateUOMs.Any(x => x.ZXG_UOM == "UOM3"));

			Assert.That(refModel.RefCusTariffUOMs, Is.Not.Null);
			Assert.That(refModel.RefCusTariffUOMs.Count, Is.EqualTo(3));
			Assert.That(refModel.RefCusTariffUOMs.Any(x => x.ZZ8_UOM == "UOM2" && x.ZZ8_Type == "CU2" && x.ZZ8_ZZA_NKTradeGroup == "1001" && x.ZZ8_ZZA_ZZZ_NKDataGrouping == "ABC"));
			Assert.That(refModel.RefCusTariffUOMs.Any(x => x.ZZ8_UOM == "UOM3" && x.ZZ8_Type == "CU2" && x.ZZ8_ZZA_NKTradeGroup == "2222" && x.ZZ8_ZZA_ZZZ_NKDataGrouping == "ABC"));
			Assert.That(refModel.RefCusTariffUOMs.Any(x => x.ZZ8_UOM == "KGM" && x.ZZ8_Type == "CU1" && x.ZZ8_ZZA_NKTradeGroup == null && x.ZZ8_ZZA_ZZZ_NKDataGrouping == null));

			Assert.That(refModel.RefCusConditions, Is.Not.Null);
			Assert.That(refModel.RefCusConditions.Count, Is.EqualTo(1));

			var formulaCondition = refModel.RefCusConditions.FirstOrDefault(x => x.ZX1_Comment == "Condition B: Condition Code Type B");
			Assert.That(formulaCondition, Is.Not.Null);
			Assert.That(formulaCondition.ZX1_ZX2_NKConditionType, Is.EqualTo("103"));
			Assert.That(formulaCondition.ZX1_StartDate, Is.EqualTo(DefaultValues.MinimumDateTime));
			Assert.That(formulaCondition.ZX1_EndDate, Is.EqualTo(DefaultValues.MaximumDateTime));
			Assert.That(formulaCondition.ZX1_ZZS_NKPreference, Is.EqualTo("100"));

			Assert.That(formulaCondition.RefCusConditionValues, Is.Not.Null);
			Assert.That(formulaCondition.RefCusConditionValues.Count, Is.EqualTo(2));
			Assert.That(formulaCondition.RefCusConditionValues[0].ZX3_ZX4_NKValueType, Is.EqualTo("SUP"));
			Assert.That(formulaCondition.RefCusConditionValues[0].ZX3_Value, Is.EqualTo("X999"));
			Assert.That(formulaCondition.RefCusConditionValues[1].ZX3_ZX4_NKValueType, Is.EqualTo("SUP"));
			Assert.That(formulaCondition.RefCusConditionValues[1].ZX3_Value, Is.EqualTo("Y888"));

			var refModel2 = refModels.FirstOrDefault(x => x.ZZ1_TariffCode == "20202000");
			Assert.That(refModel2, Is.Not.Null);
			Assert.That(refModel2.ZZ1_ZZI_NKTariffType, Is.EqualTo("EXP"));

			Assert.That(refModel2.RefCusConditions, Is.Not.Null);
			Assert.That(refModel2.RefCusConditions.Count, Is.EqualTo(1));

			formulaCondition = refModel2.RefCusConditions.FirstOrDefault(x => x.ZX1_Comment == "Condition A: Condition Code Type A");
			Assert.That(formulaCondition, Is.Not.Null);
			Assert.That(formulaCondition.RefCusConditionValues, Is.Not.Null);
			Assert.That(formulaCondition.RefCusConditionValues.Count, Is.EqualTo(1));
			Assert.That(formulaCondition.RefCusConditionValues[0].ZX3_ZX4_NKValueType, Is.EqualTo("SNR"));
			Assert.That(formulaCondition.RefCusConditionValues[0].ZX3_Value, Is.EqualTo("ConditionFormula"));

			var models3333 = refModels.Where(x => x.ZZ1_TariffCode == "3333000000").ToList();
			Assert.That(models3333, Is.Not.Null);
			Assert.That(models3333.Count, Is.EqualTo(1));
			Assert.That(models3333.Any(x => x.ZZ1_ZZI_NKTariffType == "IMP"));

			var refModel3 = models3333.First();
			Assert.That(refModel3, Is.Not.Null);
			Assert.That(refModel3.RefCusRates, Is.Not.Null);
			Assert.That(refModel3.RefCusRates.Count, Is.EqualTo(1));
			Assert.That(refModel3.RefCusConditions, Is.Not.Null);
			Assert.That(refModel3.RefCusConditions.Count, Is.EqualTo(1));

			var conFormulaModel = refModels.First(x => x.ZZ1_TariffCode == "0404040404");
			Assert.That(conFormulaModel, Is.Not.Null);
			Assert.That(conFormulaModel.RefCusConditions, Is.Not.Null);
			Assert.That(conFormulaModel.RefCusConditions.Count, Is.EqualTo(1));
			Assert.That(conFormulaModel.RefCusConditions[0].RefCusConditionValues.Any(y => y.ZX3_Value == "X999" && y.ZX3_ZX4_NKValueType == "SUP"));
			Assert.That(conFormulaModel.RefCusConditions[0].RefCusConditionValues.Any(y => y.ZX3_Value == "[KGM] <= 123.45" && y.ZX3_ZX4_NKValueType == "FRM"));

			measure1.StartDate = null;
			measure1.EndDate = null;
			measure1.NomenclatureStartDate = null;
			measure1.NomenclatureEndDate = null;

			refModel = builder.ConvertToRefModels(tariffModels).First();

			Assert.That(refModel.ZZ1_StartDate, Is.EqualTo(DefaultValues.MinimumDateTime));
			Assert.That(refModel.ZZ1_EndDate, Is.EqualTo(DefaultValues.MaximumDateTime));
		}

		[Test]
		public void NoFormulaNoRate()
		{
			var measure1 = new Measure
			{
				ItemId = "1234560000",
				CleanId = "123456",
				StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
				EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
				Description = "Test Model Convertion",
				CompositeKey = "03.12.34",
				GeographicalArea = "1001",
				Formula = "",
				MeasureType = "103",
				MeasureTypeSeries = "C",
				MeasureTypeDescription = "Third country duty",
				RateType = "DTY",
				Preferences = new List<string> { "100" },
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				TariffTypes = new List<string> { "IMP" }
			};

			var tariffModels = new List<ITariffModel>() { measure1 };

			var refModels = builder.ConvertToRefModels(tariffModels);
			Assert.That(refModels.Count, Is.EqualTo(2));

			var refModel = refModels.First();
			Assert.That(refModel.RefCusRates, Is.Null.Or.Empty);
		}

		[Test]
		public void XmlWriterConfig()
		{
			var config = builder.XmlWriterConfiguration();

			Assert.That(config, Is.Not.Null);
			var refType = typeof(RefCusTariff);
			var tariffConfig = config.GetConfiguration(refType);

			Assert.That(tariffConfig, Is.Not.Null);
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_TariffCode))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_ZZI_NKTariffType))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_ZZI_ZZZ_NKDataGrouping))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_ZZZ_NKDataGrouping))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_Description))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_StartDate))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_EndDate))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_CompositeKeyOnZZ5))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.RefCusConditions))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.RefCusRates))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.RefCusTariffUOMs))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.RefCusVATApplicabilities))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_IAMUnique))));

			AssertKeySets(nameof(RefCusTariff), tariffConfig.GetKeySets(),
				nameof(RefCusTariff.ZZ1_IAMUnique),
				nameof(RefCusTariff.ZZ1_TariffCode),
				nameof(RefCusTariff.ZZ1_ZZI_NKTariffType),
				nameof(RefCusTariff.ZZ1_ZZI_ZZZ_NKDataGrouping),
				nameof(RefCusTariff.ZZ1_ZZZ_NKDataGrouping));

			refType = typeof(RefCusTariffUOM);
			var uomConfig = config.GetConfiguration(refType);

			Assert.That(uomConfig, Is.Not.Null);
			Assert.That(uomConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariffUOM.ZZ8_Type))));
			Assert.That(uomConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariffUOM.ZZ8_ZZA_NKTradeGroup))));
			Assert.That(uomConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariffUOM.ZZ8_ZZZ_NKDataGrouping))));
			Assert.That(uomConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariffUOM.ZZ8_UOM))));
			Assert.That(uomConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariffUOM.ZZ8_ZZA_ZZZ_NKDataGrouping))));

			AssertKeySets(nameof(RefCusTariffUOM), uomConfig.GetKeySets(),
				nameof(RefCusTariffUOM.ZZ8_Type),
				nameof(RefCusTariffUOM.ZZ8_ZZA_NKTradeGroup),
				nameof(RefCusTariffUOM.ZZ8_ZZZ_NKDataGrouping),
				nameof(RefCusTariffUOM.ZZ8_ZZA_ZZZ_NKDataGrouping));

			refType = typeof(RefCusRate);
			var rateConfig = config.GetConfiguration(refType);

			Assert.That(rateConfig, Is.Not.Null);
			Assert.That(rateConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_ZY1_NKRateCode))));
			Assert.That(rateConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_ZY1_ZZR_NKRateType))));
			Assert.That(rateConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping))));
			Assert.That(rateConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_ZZS_NKPreference))));
			Assert.That(rateConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_ZZS_ZZZ_NKDataGrouping))));
			Assert.That(rateConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_ZZZ_NKDataGrouping))));
			Assert.That(rateConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.RefCusApplicabilities))));
			Assert.That(rateConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_StartDate))));
			Assert.That(rateConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_EndDate))));
			Assert.That(rateConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_RateFormula))));
			Assert.That(rateConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.RefCusRateUOMs))));

			AssertKeySets(nameof(RefCusRate), rateConfig.GetKeySets(),
				nameof(RefCusRate.ZZ2_ZY1_NKRateCode),
				nameof(RefCusRate.ZZ2_ZY1_ZZR_NKRateType),
				nameof(RefCusRate.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping),
				nameof(RefCusRate.ZZ2_ZZS_NKPreference),
				nameof(RefCusRate.ZZ2_ZZS_ZZZ_NKDataGrouping),
				nameof(RefCusRate.ZZ2_ZZZ_NKDataGrouping),
				nameof(RefCusRate.RefCusApplicabilities));

			refType = typeof(RefCusApplicability);
			var appConfig = config.GetConfiguration(refType);

			Assert.That(appConfig, Is.Not.Null);
			Assert.That(appConfig.IsIncluded(refType.GetProperty(nameof(RefCusApplicability.ZZT_AdditionalCode))));
			Assert.That(appConfig.IsIncluded(refType.GetProperty(nameof(RefCusApplicability.ZZT_OrderNumber))));
			Assert.That(appConfig.IsIncluded(refType.GetProperty(nameof(RefCusApplicability.ZZT_ZZA_NKTradeGroup))));
			Assert.That(appConfig.IsIncluded(refType.GetProperty(nameof(RefCusApplicability.ZZT_ZZA_ZZZ_NKDataGrouping))));
			Assert.That(appConfig.IsIncluded(refType.GetProperty(nameof(RefCusApplicability.ZZT_StartDate))));
			Assert.That(appConfig.IsIncluded(refType.GetProperty(nameof(RefCusApplicability.ZZT_EndDate))));
			Assert.That(appConfig.IsIncluded(refType.GetProperty(nameof(RefCusApplicability.RefCusExcludedTradeGroups))));

			AssertKeySets(nameof(RefCusApplicability), appConfig.GetKeySets(),
				nameof(RefCusApplicability.ZZT_AdditionalCode),
				nameof(RefCusApplicability.ZZT_OrderNumber),
				nameof(RefCusApplicability.ZZT_ZZA_ZZZ_NKDataGrouping),
				nameof(RefCusApplicability.ZZT_ZZA_NKTradeGroup));

			refType = typeof(RefCusRateUOM);
			var rateUOMConfig = config.GetConfiguration(refType);

			Assert.That(rateUOMConfig, Is.Not.Null);
			Assert.That(rateUOMConfig.IsIncluded(refType.GetProperty(nameof(RefCusRateUOM.ZXG_UOM))));

			AssertKeySets(nameof(RefCusRateUOM), rateUOMConfig.GetKeySets(),
				nameof(RefCusRateUOM.ZXG_UOM));

			refType = typeof(RefCusExcludedTradeGroup);
			var exclConfig = config.GetConfiguration(refType);

			Assert.That(exclConfig, Is.Not.Null);
			Assert.That(exclConfig.IsIncluded(refType.GetProperty(nameof(RefCusExcludedTradeGroup.ZZC_ZZA_NKTradeGroup))));
			Assert.That(exclConfig.IsIncluded(refType.GetProperty(nameof(RefCusExcludedTradeGroup.ZZC_ZZA_ZZZ_NKDataGrouping))));

			AssertKeySets(nameof(RefCusExcludedTradeGroup), exclConfig.GetKeySets(),
				nameof(RefCusExcludedTradeGroup.ZZC_ZZA_NKTradeGroup),
				nameof(RefCusExcludedTradeGroup.ZZC_ZZA_ZZZ_NKDataGrouping));

			refType = typeof(RefCusCondition);
			var conditionConfig = config.GetConfiguration(refType);

			Assert.That(conditionConfig, Is.Not.Null);
			Assert.That(conditionConfig.IsIncluded(refType.GetProperty(nameof(RefCusCondition.RefCusApplicabilities))));
			Assert.That(conditionConfig.IsIncluded(refType.GetProperty(nameof(RefCusCondition.ZX1_Comment))));
			Assert.That(conditionConfig.IsIncluded(refType.GetProperty(nameof(RefCusCondition.ZX1_ZX2_NKConditionType))));
			Assert.That(conditionConfig.IsIncluded(refType.GetProperty(nameof(RefCusCondition.ZX1_ZX2_ZZZ_NKDataGrouping))));
			Assert.That(conditionConfig.IsIncluded(refType.GetProperty(nameof(RefCusCondition.ZX1_ZZS_NKPreference))));
			Assert.That(conditionConfig.IsIncluded(refType.GetProperty(nameof(RefCusCondition.ZX1_ZZS_ZZZ_NKDataGrouping))));
			Assert.That(conditionConfig.IsIncluded(refType.GetProperty(nameof(RefCusCondition.ZX1_ZZZ_NKDataGrouping))));
			Assert.That(conditionConfig.IsIncluded(refType.GetProperty(nameof(RefCusCondition.ZX1_ConditionValueTrueMeansStop))));
			Assert.That(conditionConfig.IsIncluded(refType.GetProperty(nameof(RefCusCondition.ZX1_StartDate))));
			Assert.That(conditionConfig.IsIncluded(refType.GetProperty(nameof(RefCusCondition.ZX1_EndDate))));
			Assert.That(conditionConfig.IsIncluded(refType.GetProperty(nameof(RefCusCondition.ZX1_IsImport))));
			Assert.That(conditionConfig.IsIncluded(refType.GetProperty(nameof(RefCusCondition.ZX1_IsExport))));
			Assert.That(conditionConfig.IsIncluded(refType.GetProperty(nameof(RefCusCondition.ZX1_Source))));
			Assert.That(conditionConfig.IsIncluded(refType.GetProperty(nameof(RefCusCondition.RefCusConditionValues))));

			AssertKeySets(nameof(RefCusCondition), conditionConfig.GetKeySets(),
				nameof(RefCusCondition.ZX1_ZX2_NKConditionType),
				nameof(RefCusCondition.ZX1_ZX2_ZZZ_NKDataGrouping),
				nameof(RefCusCondition.ZX1_ZZS_NKPreference),
				nameof(RefCusCondition.ZX1_ZZS_ZZZ_NKDataGrouping),
				nameof(RefCusCondition.ZX1_ZZZ_NKDataGrouping),
				nameof(RefCusCondition.ZX1_Comment),
				nameof(RefCusCondition.RefCusApplicabilities),
				nameof(RefCusCondition.RefCusConditionValues));

			refType = typeof(RefCusConditionValue);
			var conValConfig = config.GetConfiguration(refType);

			Assert.That(conValConfig, Is.Not.Null);
			Assert.That(conValConfig.IsIncluded(refType.GetProperty(nameof(RefCusConditionValue.ZX3_Value))));
			Assert.That(conValConfig.IsIncluded(refType.GetProperty(nameof(RefCusConditionValue.ZX3_ZX4_NKValueType))));
			Assert.That(conValConfig.IsIncluded(refType.GetProperty(nameof(RefCusConditionValue.ZX3_ZX4_ZZZ_NKDataGrouping))));

			AssertKeySets(nameof(RefCusConditionValue), conValConfig.GetKeySets(),
				nameof(RefCusConditionValue.ZX3_Value),
				nameof(RefCusConditionValue.ZX3_ZX4_NKValueType),
				nameof(RefCusConditionValue.ZX3_ZX4_ZZZ_NKDataGrouping));

			refType = typeof(RefCusVATApplicability);
			var vatApplConfig = config.GetConfiguration(refType);

			Assert.That(vatApplConfig, Is.Not.Null);
			Assert.That(vatApplConfig.IsIncluded(refType.GetProperty(nameof(RefCusVATApplicability.ZX5_AdditionalCode))));
			Assert.That(vatApplConfig.IsIncluded(refType.GetProperty(nameof(RefCusVATApplicability.ZX5_StartDate))));
			Assert.That(vatApplConfig.IsIncluded(refType.GetProperty(nameof(RefCusVATApplicability.ZX5_EndDate))));
			Assert.That(vatApplConfig.IsIncluded(refType.GetProperty(nameof(RefCusVATApplicability.ZX5_ZZF_NKTaxOrFeeCode))));
			Assert.That(vatApplConfig.IsIncluded(refType.GetProperty(nameof(RefCusVATApplicability.ZX5_ZZZ_NKDataGrouping))));

			AssertKeySets(nameof(RefCusVATApplicability), vatApplConfig.GetKeySets(),
				nameof(RefCusVATApplicability.ZX5_ZZZ_NKDataGrouping),
				nameof(RefCusVATApplicability.ZX5_ZZF_NKTaxOrFeeCode),
				nameof(RefCusVATApplicability.ZX5_AdditionalCode));
		}

		void AssertKeySets(string entity, IEnumerable<KeySet> keySets, params string[] properties)
		{
			var keys = keySets.Select(x => x.PropertySchema.Name).OrderBy(x => x).ToList();
			var fields = properties.OrderBy(x => x).ToList();

			Assert.That(string.Join(", ", fields), Is.EqualTo(string.Join(", ", keys)), $"Keys for {entity}");
		}

		[Test]
		public void UniqueId()
		{
			var model = new RefCusTariff { ZZ1_TariffCode = "0102030405", ZZ1_StartDate = new DateTime(2019, 01, 26, 12, 13, 14), ZZ1_EndDate = new DateTime(2019, 02, 17, 09, 33, 04), ZZ1_ZZI_NKTariffType = "IMP", ZZ1_CompositeKeyOnZZ5 = "01.02.03", ZZ1_IAMUnique = 2 };

			Assert.That(builder.UniqueId(model), Is.EqualTo("0102030405_IMP_2_20190126121314_20190217093304"));
		}

		[Test]
		public void DuplicateErrors()
		{
			var model = new RefCusTariff { ZZ1_TariffCode = "1234567890", ZZ1_Description = "Unit Testing", ZZ1_ZZI_NKTariffType = "IMP" };

			builder.DuplicateError(model, "0987654321", "UnitTest");
			Assert.That(errorCollector.ToString(), Contains.Substring("RefCusTariff duplicate exists. Key: '0987654321' Filter: 'UnitTest'"));
		}

		[Test]
		public void ExportDuplicatesUseIAMUnique()
		{
			var measure1 = new Measure { StartDate = new DateTime(2019, 01, 01), EndDate = new DateTime(2019, 12, 31, 23, 59, 59), ItemId = "1000000001", Description = "Item1", CompositeKey = "01.01..10.10.10", TariffTypes = new List<string> { "EXP" } };
			var measure2 = new Measure { StartDate = new DateTime(2019, 01, 01), EndDate = new DateTime(2019, 12, 31, 23, 59, 59), ItemId = "1000000002", Description = "Item2", CompositeKey = "01.01..10.10.20", TariffTypes = new List<string> { "EXP" } };

			var tariffModels = new List<ITariffModel>() { measure1, measure2 };

			var refModels = builder.ConvertToRefModels(tariffModels);
			Assert.That(refModels.Count, Is.EqualTo(4));

			var model1 = refModels.First(x => x.ZZ1_CompositeKeyOnZZ5 == "01.01..10.10.10" && x.ZZ1_ZZI_NKTariffType == "EXP");
			var model2 = refModels.First(x => x.ZZ1_CompositeKeyOnZZ5 == "01.01..10.10.20" && x.ZZ1_ZZI_NKTariffType == "EXP");

			Assert.That(model1.ZZ1_ZZI_NKTariffType, Is.EqualTo(model2.ZZ1_ZZI_NKTariffType));
			Assert.That(model1.ZZ1_TariffCode, Is.EqualTo(model2.ZZ1_TariffCode));
			Assert.That(model1.ZZ1_StartDate, Is.EqualTo(model2.ZZ1_StartDate));

			Assert.That(model1.ZZ1_IAMUnique, Is.EqualTo(0));
			Assert.That(model2.ZZ1_IAMUnique, Is.EqualTo(1));
		}

		[Test]
		public void IsValid()
		{
			var model = new RefCusTariff();

			Assert.That(builder.IsValid(model, "UnitTest"), Is.False);
			Assert.That(errorCollector.ToString(), Contains.Substring("RefCusTariff validation error")
														.And.Contains("ZZ1_TariffCode is required")
														.And.Contains("ZZ1_Description is required")
														.And.Contains("Filter: 'UnitTest'"));

			errorCollector.Clear();
			model.ZZ1_TariffCode = "1234560000";
			Assert.That(builder.IsValid(model, "UnitTest"), Is.False);
			Assert.That(errorCollector.ToString(), Does.Not.Contain("ZZ1_TariffCode is required"));

			errorCollector.Clear();
			model.ZZ1_Description = "Unit Test";
			Assert.That(builder.IsValid(model, "UnitTest"), Is.True);
			Assert.That(errorCollector.ToString(), Does.Not.Contain("Tariff validation error"));

			errorCollector.Clear();
			model.RefCusRates = new RefCusRate[]
			{
				new RefCusRate
				{
					ZZ2_RateFormula = "".PadRight(500, 'X')
				}
			};
			Assert.That(builder.IsValid(model, "UnitTest"), Is.True);
			Assert.That(errorCollector.ToString(), Does.Not.Contain("Tariff validation error"));

			model.RefCusRates[0].ZZ2_RateFormula += "Y";
			Assert.That(builder.IsValid(model, "UnitTest"), Is.False);
			Assert.That(errorCollector.ToString(), Contains.Substring("RefCusTariff validation error")
														.And.Contains("ZZ2_RateFormula maxiumum length of 500 exceeded"));
		}

		[Test]
		public void IsExpired()
		{
			var model = new RefCusTariff();

			model.ZZ1_EndDate = DateTime.MinValue;
			Assert.That(builder.IsExpired(model), Is.True);

			model.ZZ1_EndDate = DateTime.Today.AddYears(-1);
			Assert.That(builder.IsExpired(model), Is.False);

			model.ZZ1_EndDate = DateTime.MaxValue.Date;
			Assert.That(builder.IsExpired(model), Is.False);
		}

		[Test]
		public void BuildXmlFile()
		{
			var models = new List<ITariffModel>()
			{
				new Measure { StartDate = new DateTime(2019,01,01), EndDate = new DateTime(2019,12,31, 23,59,59), NomenclatureStartDate = new DateTime(2019,01,01), NomenclatureEndDate = new DateTime(2019,12,31, 23,59,59), ItemId = "1010000000", CleanId = "1010",  Description = "Item1", CompositeKey = "01.10.10", TariffTypes = new List<string> { "IMP" }, GeographicalArea = "1001" },
				new Measure { StartDate = new DateTime(2019,01,01), EndDate = new DateTime(2019,12,31, 23,59,59), NomenclatureStartDate = new DateTime(2019,01,01), NomenclatureEndDate = new DateTime(2019,12,31, 23,59,59), ItemId = "2020000000", CleanId = "2020",  Description = "Item2", CompositeKey = "01.20.20", TariffTypes = new List<string> { "IMP" }, GeographicalArea = "1001" },
				new Measure { StartDate = new DateTime(2019,01,01), EndDate = new DateTime(2019,12,31, 23,59,59), NomenclatureStartDate = new DateTime(2019,01,01), NomenclatureEndDate = new DateTime(2019,12,31, 23,59,59), ItemId = "2020000000", CleanId = "2020",  Description = "Duplicate Item2", CompositeKey = "01.20.20", TariffTypes = new List<string> { "IMP" }, GeographicalArea = "1001" },
				new Measure { StartDate = new DateTime(2019,01,01), NomenclatureStartDate = new DateTime(2019,01,01), ItemId = "3456000000", CleanId = "3456", Description = "NoEndDate", CompositeKey = "02.34.56", TariffTypes = new List<string> { "IMP" }, GeographicalArea = "1001" },
				new Measure { EndDate = new DateTime(2019,12,31, 23,59,59), NomenclatureEndDate = new DateTime(2019,12,31, 23,59,59), ItemId = "4025000000", CleanId = "4025", Description = "NoStartDate", CompositeKey = "03.40.25", TariffTypes = new List<string> { "IMP" }, GeographicalArea = "1001" },
				new Measure { StartDate = new DateTime(2019,01,01), EndDate = new DateTime(2019,12,31, 23,59,59), NomenclatureStartDate = new DateTime(2019,01,01), NomenclatureEndDate = new DateTime(2019,12,31, 23,59,59), ItemId = "9876543210", CleanId = "Invalid", Description = "", TariffTypes = new List<string> { "IMP" }, GeographicalArea = "1001" },
				new Measure
				{
					ItemId = "1234560000",
					CleanId = "123456",
					StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
					EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
					NomenclatureStartDate = new DateTime(2019, 10, 01, 15, 14, 13),
					NomenclatureEndDate = new DateTime(2019, 12, 31, 23, 59, 59),
					Description = "XML structure test",
					CompositeKey = "03.12.34",
					OrderNumber = "123456",
					AdditionalCode = "123",
					AdditionalCodeType = "4",
					GeographicalArea = "1001",
					Formula = "RateFormula",
					MeasureType = "103",
					MeasureTypeSeries = "A",
					MeasureTypeDescription = "Third country duty",
					RateType = "DTY",
					RateCode = "A00",
					Preferences = new List<string> { "100" },
					ConditionClass = MeasureHelper.ConditionClass.Rate,
					TariffTypes = new List<string> { "IMP" },
				}.SetComponents(new[]
				{
					new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "1", DutyExpression = "01", HJID = "1" },
					new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "2", DutyExpression = "99", HJID = "2" }
				})
				.SetConditions(new[]
				{
					new MeasureCondition
					{
						MeasurementUnit = "UOM",
						MeasurementUnitQualifier = "2",
						ConditionCode = "A",
						ConditionCodeDescription = "Condition Code Type A",
						MeasureAction = "01",
						IsRateFormulaCondition = true,
						HJID = "3"
					}.SetComponents(new[]
					{
							new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "2", DutyExpression = "01", HJID = "4" },
							new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "3", DutyExpression = "02", HJID = "5" }
					}),
					new MeasureCondition
					{
						ConditionCode = "B",
						ConditionCodeDescription = "Condition Code Type B",
						MeasureAction = "99",
						CertificateCode = "999",
						CertificateTypeCode = "X",
						HJID = "6"
					},
					new MeasureCondition
					{
						ConditionCode = "B",
						ConditionCodeDescription = "Condition Code Type B",
						MeasureAction = "99",
						CertificateCode = "888",
						CertificateTypeCode = "Y",
						HJID = "7"
					}
				})
				.SetExcludedGeographicalAreas(new[]
				{
					new ExcludedGeographicalArea { Value = "EX1", HJID = "1" },
					new ExcludedGeographicalArea { Value = "EX2", HJID = "2" }
				}),
				new Measure
				{
					ItemId = "0100000001",
					StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
					EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
					NomenclatureStartDate = new DateTime(2019, 10, 01, 15, 14, 13),
					NomenclatureEndDate = new DateTime(2019, 12, 31, 23, 59, 59),
					Description = "Standard VAT",
					CompositeKey = "03.12.34",
					GeographicalArea = "1001",
					Formula = "RateFormula",
					MeasureType = "305",
					MeasureTypeSeries = "A",
					MeasureTypeDescription = "Vat",
					RateType = "DTY",
					RateCode = "A00",
					Preferences = new List<string> { null },
					ConditionClass = MeasureHelper.ConditionClass.Vat,
					VatCode = "666",
					AdditionalCode = "ABC",
					AdditionalCodeType = "V",

					TariffTypes = new List<string> { "IMP" },
				}.SetComponents(new[] { new MeasureComponent { DutyExpression = "01", DutyAmount = 20.0m, HJID = "8" } })
			};

			var dateTimeProvider = new Mock<IDateTimeProvider>();
			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2019, 11, 04, 22, 36, 45, 135));
			var errorCollector = new StringBuilder();
			var builder = new TariffBuilderTester(dateTimeProvider.Object, errorCollector);

			var publicationDate = new DateTime(2019, 11, 03, 13, 14, 15, 678, DateTimeKind.Utc);

			builder.BuildXml(publicationDate, models, TempFolder, "UnitTest");

			var fileName = Path.Combine(TempFolder, builder.OutputFileName);
			Assert.That(File.Exists(fileName));

			var xml = File.ReadAllText(fileName);
			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.SharedReferenceData.Tests.Business.Tariff.TestFiles.Output.RefCusTariff_UT001.xml");
			Assert.That(xml, Is.EqualTo(expectedXml));
		}

		[Test]
		public void Preferences()
		{
			var measure1 = new Measure
			{
				ItemId = "1234560000",
				StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
				EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
				Description = "XML structure test",
				CompositeKey = "03.12.34",
				GeographicalArea = "1001",
				Formula = "RateFormula",
				MeasureType = "103",
				MeasureTypeSeries = "A",
				MeasureTypeDescription = "Third country duty",
				RateType = "DTY",
				RateCode = "A00",
				Preferences = new List<string> { null },
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				TariffTypes = new List<string> { "IMP" },
			}
			.SetComponents(new[]
			{
				new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "1", DutyExpression = "01", HJID = "1" }
			})
			.SetConditions(new[]
			{
				new MeasureCondition
				{
					ConditionCode = "B",
					ConditionCodeDescription = "Condition Code Type B",
					MeasureAction = "99",
					CertificateCode = "999",
					CertificateTypeCode = "X",
					HJID = "2"
				}
			});

			var tariffModels = new List<ITariffModel>() { measure1 };

			var refModels = builder.ConvertToRefModels(tariffModels);
			Assert.That(refModels.Count, Is.EqualTo(2));

			var refModel = refModels.First();
			Assert.That(refModel.RefCusRates, Is.Not.Null);
			Assert.That(refModel.RefCusRates.Count, Is.EqualTo(1));
			Assert.That(refModel.RefCusRates[0].ZZ2_ZZS_NKPreference, Is.Null);

			Assert.That(refModel.RefCusConditions, Is.Not.Null);
			Assert.That(refModel.RefCusConditions.Count, Is.EqualTo(1));
			Assert.That(refModel.RefCusConditions[0].ZX1_ZZS_NKPreference, Is.Null);

			measure1.Preferences = new List<string> { string.Empty };
			refModels = builder.ConvertToRefModels(tariffModels);
			Assert.That(refModels.Count, Is.EqualTo(2));

			refModel = refModels.First();
			Assert.That(refModel.RefCusRates, Is.Not.Null);
			Assert.That(refModel.RefCusRates.Count, Is.EqualTo(1));
			Assert.That(refModel.RefCusRates[0].ZZ2_ZZS_NKPreference, Is.Null);

			measure1.Preferences = new List<string> { "123", "456" };
			refModels = builder.ConvertToRefModels(tariffModels);
			Assert.That(refModels.Count, Is.EqualTo(2));

			refModel = refModels.First();
			Assert.That(refModel.RefCusRates, Is.Not.Null);
			Assert.That(refModel.RefCusRates.Count, Is.EqualTo(2));
			Assert.That(refModel.RefCusRates.Any(x => x.ZZ2_ZZS_NKPreference == "123"));
			Assert.That(refModel.RefCusRates.Any(x => x.ZZ2_ZZS_NKPreference == "456"));

			Assert.That(refModel.RefCusConditions, Is.Not.Null);
			Assert.That(refModel.RefCusConditions.Count, Is.EqualTo(2));
			Assert.That(refModel.RefCusConditions.Any(x => x.ZX1_ZZS_NKPreference == "123"));
			Assert.That(refModel.RefCusConditions.Any(x => x.ZX1_ZZS_NKPreference == "456"));
		}

		[Test]
		public void ExcludeNegativeConditions()
		{
			var measure1 = new Measure
			{
				ItemId = "1234560000",
				StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
				EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
				Description = "XML structure test",
				CompositeKey = "03.12.34",
				GeographicalArea = "1001",
				Formula = "RateFormula",
				MeasureType = "103",
				MeasureTypeSeries = "A",
				MeasureTypeDescription = "Third country duty",
				RateType = "DTY",
				RateCode = "A00",
				Preferences = new List<string> { null },
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				TariffTypes = new List<string> { "IMP" },
			}
			.SetComponents(new[]
			{
				new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "1", DutyExpression = "01", HJID = "1" }
			})
			.SetConditions(new[]
			{
				new MeasureCondition
				{
					SequenceNumber = 1,
					ConditionCode = "S",
					ConditionCodeDescription = "Condition Code Type S",
					MeasureAction = "27",
					DutyAmount = 5.0m,
					MeasurementUnit = "TNE",
					Formula = "VALID",
					HJID = "2"
				},
				new MeasureCondition
				{
					SequenceNumber = 2,
					ConditionCode = "S",
					ConditionCodeDescription = "Condition Code Type S",
					MeasureAction = "07",
					DutyAmount = 0.0m,
					MeasurementUnit = "TNE",
					Formula = "INVALID",
					HJID = "3"
				}
			});

			var tariffModels = new List<ITariffModel>() { measure1 };

			var refModels = builder.ConvertToRefModels(tariffModels);
			Assert.That(refModels.Count, Is.EqualTo(2));

			var refModel = refModels.First();
			Assert.That(refModel.RefCusConditions, Is.Not.Null);
			Assert.That(refModel.RefCusConditions.Count, Is.EqualTo(1));
			Assert.That(refModel.RefCusConditions[0].RefCusConditionValues, Is.Not.Null);
			Assert.That(refModel.RefCusConditions[0].RefCusConditionValues.Count, Is.EqualTo(1));
			Assert.That(refModel.RefCusConditions[0].RefCusConditionValues[0].ZX3_Value, Is.EqualTo("VALID"));
		}

		[Test]
		public void VatApplicability()
		{
			var measure1 = new Measure
			{
				ItemId = "0100000001",
				StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
				EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
				Description = "Standard VAT",
				CompositeKey = "03.12.34",
				GeographicalArea = "1001",
				Formula = "RateFormula",
				MeasureType = "305",
				MeasureTypeSeries = "A",
				MeasureTypeDescription = "Vat",
				RateType = "DTY",
				RateCode = "A00",
				Preferences = new List<string> { null },
				ConditionClass = MeasureHelper.ConditionClass.Vat,
				VatCode = "666",

				TariffTypes = new List<string> { "IMP" },
			}
			.SetComponents(new[] { new MeasureComponent { DutyExpression = "01", DutyAmount = 20.0m, HJID = "1" } });

			var measure2 = new Measure
			{
				ItemId = "0200000002",
				StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
				EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
				Description = "Reduced VAT",
				CompositeKey = "03.12.34",
				GeographicalArea = "1001",
				Formula = "RateFormula",
				MeasureType = "305",
				MeasureTypeSeries = "A",
				MeasureTypeDescription = "Vat",
				RateType = "DTY",
				RateCode = "A00",
				Preferences = new List<string> { null },
				ConditionClass = MeasureHelper.ConditionClass.Vat,
				VatCode = "650",

				TariffTypes = new List<string> { "IMP" },
			}
			.SetComponents(new[] { new MeasureComponent { DutyExpression = "01", DutyAmount = 20.0m, HJID = "2" } });

			var measure3 = new Measure
			{
				ItemId = "0300000003",
				StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
				EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
				Description = "Zero VAT",
				CompositeKey = "03.12.34",
				GeographicalArea = "1001",
				Formula = "RateFormula",
				MeasureType = "305",
				MeasureTypeSeries = "A",
				MeasureTypeDescription = "Vat",
				RateType = "DTY",
				RateCode = "A00",
				Preferences = new List<string> { null },
				ConditionClass = MeasureHelper.ConditionClass.Vat,
				VatCode = "673",

				TariffTypes = new List<string> { "IMP" },
			}
			.SetComponents(new[] { new MeasureComponent { DutyExpression = "01", DutyAmount = 20.0m, HJID = "3" } });

			var measure4 = new Measure
			{
				ItemId = "0400000004",
				StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
				EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
				Description = "Not VAT",
				CompositeKey = "03.12.34",
				GeographicalArea = "1001",
				Formula = "RateFormula",
				MeasureType = "103",
				MeasureTypeSeries = "A",
				MeasureTypeDescription = "Something else",
				RateType = "DTY",
				RateCode = "A00",
				Preferences = new List<string> { null },
				ConditionClass = MeasureHelper.ConditionClass.Vat,
				VatCode = null,

				TariffTypes = new List<string> { "IMP" },
			}
			.SetComponents(new[] { new MeasureComponent { DutyExpression = "01", DutyAmount = 20.0m, HJID = "4" } });

			var measure5 = new Measure
			{
				ItemId = "0100000001",
				StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
				EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
				Description = "Standard VAT with additional code",
				CompositeKey = "03.12.34",
				GeographicalArea = "1001",
				Formula = "RateFormula",
				MeasureType = "305",
				MeasureTypeSeries = "A",
				MeasureTypeDescription = "Vat",
				RateType = "DTY",
				RateCode = "A00",
				Preferences = new List<string> { null },
				ConditionClass = MeasureHelper.ConditionClass.Vat,
				VatCode = "666",
				AdditionalCode = "ABC",
				AdditionalCodeType = "X",

				TariffTypes = new List<string> { "IMP" },
			}.SetComponents(new[] { new MeasureComponent { DutyExpression = "01", DutyAmount = 20.0m, HJID = "5" } });

			var tariffModels = new List<ITariffModel>() { measure1, measure2, measure3, measure4, measure5 };

			var refModels = builder.ConvertToRefModels(tariffModels);

			Assert.That(refModels, Is.Not.Null);
			Assert.That(refModels.Count, Is.EqualTo(8));

			var vatModel = refModels.FirstOrDefault(x => x.ZZ1_TariffCode == "0100000001");
			Assert.That(vatModel, Is.Not.Null);
			Assert.That(vatModel.RefCusVATApplicabilities, Is.Not.Null);
			Assert.That(vatModel.RefCusVATApplicabilities.Count, Is.EqualTo(2));
			Assert.That(vatModel.RefCusVATApplicabilities.Any(x => x.ZX5_ZZF_NKTaxOrFeeCode == "666" && string.IsNullOrEmpty(x.ZX5_AdditionalCode)));
			Assert.That(vatModel.RefCusVATApplicabilities.Any(x => x.ZX5_ZZF_NKTaxOrFeeCode == "666" && x.ZX5_AdditionalCode == "XABC"));

			var redModel = refModels.FirstOrDefault(x => x.ZZ1_TariffCode == "0200000002");
			Assert.That(redModel, Is.Not.Null);
			Assert.That(redModel.RefCusVATApplicabilities, Is.Not.Null);
			Assert.That(redModel.RefCusVATApplicabilities.Count, Is.EqualTo(1));
			Assert.That(redModel.RefCusVATApplicabilities[0].ZX5_ZZF_NKTaxOrFeeCode, Is.EqualTo("650"));

			var zerModel = refModels.FirstOrDefault(x => x.ZZ1_TariffCode == "0300000003");
			Assert.That(zerModel, Is.Not.Null);
			Assert.That(zerModel.RefCusVATApplicabilities, Is.Not.Null);
			Assert.That(zerModel.RefCusVATApplicabilities.Count, Is.EqualTo(1));
			Assert.That(zerModel.RefCusVATApplicabilities[0].ZX5_ZZF_NKTaxOrFeeCode, Is.EqualTo("673"));

			var djokovicModel = refModels.FirstOrDefault(x => x.ZZ1_TariffCode == "0400000004");
			Assert.That(djokovicModel, Is.Not.Null);
			Assert.That(djokovicModel.RefCusVATApplicabilities, Is.Null);
		}

		[Test]
		public void TariffUOMGKMForAllImports()
		{
			var measure1 = new Measure
			{
				ItemId = "1234560000",
				CleanId = "123456",
				StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
				EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
				Description = "Test Model Convertion 1.1",
				CompositeKey = "03.12.34",
				GeographicalArea = "1001",
				MeasureType = "103",
				MeasureTypeSeries = "C",
				MeasureTypeDescription = "Third country duty",
				RateType = "DTY",
				RateCode = "A00",
				Preferences = new List<string> { "100" },
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				TariffTypes = new List<string> { "IMP", "EXP" },
			}
			.SetComponents(new[]
			{
				new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "2", DutyExpression = "99", HJID = "1" }
			});

			var tariffModels = new List<ITariffModel>() { measure1 };

			var refModels = builder.ConvertToRefModels(tariffModels);
			Assert.That(refModels.Count, Is.EqualTo(2));

			var imp = refModels.FirstOrDefault(x => x.ZZ1_ZZI_NKTariffType == "IMP");
			Assert.That(imp.RefCusTariffUOMs, Is.Not.Null);
			Assert.That(imp.RefCusTariffUOMs.Count, Is.EqualTo(2));
			Assert.That(imp.RefCusTariffUOMs.Any(x => x.ZZ8_UOM == "UOM2" && x.ZZ8_Type == "CU2"));
			Assert.That(imp.RefCusTariffUOMs.Any(x => x.ZZ8_UOM == "KGM" && x.ZZ8_Type == "CU1" && x.ZZ8_ZZA_NKTradeGroup == null));

			var exp = refModels.FirstOrDefault(x => x.ZZ1_ZZI_NKTariffType == "EXP");
			Assert.That(exp.RefCusTariffUOMs, Is.Not.Null);
			Assert.That(exp.RefCusTariffUOMs.Count, Is.EqualTo(2));
			Assert.That(exp.RefCusTariffUOMs.Any(x => x.ZZ8_UOM == "UOM2" && x.ZZ8_Type == "CU2"));
			Assert.That(exp.RefCusTariffUOMs.Any(x => x.ZZ8_UOM == "KGM" && x.ZZ8_Type == "CU1" && x.ZZ8_ZZA_NKTradeGroup == null));
		}

		[Test]
		public void RefCusApplicabilityRefactor()
		{
			var measures = new List<ITariffModel>
			{
				new Measure { ItemId = "1234560000", StartDate = new DateTime(2019, 10, 01), EndDate = new DateTime(2050, 12, 31), Description = "Basic",       CompositeKey = "03.12.34", GeographicalArea = "1001", ConditionClass = MeasureHelper.ConditionClass.Rate, RateCode = "R", Formula = "0", Preferences = new List<string> { null }, TariffTypes = new List<string> { "IMP" } },
				new Measure { ItemId = "1234560000", StartDate = new DateTime(2019, 10, 01), EndDate = new DateTime(2050, 12, 31), Description = "Order",       CompositeKey = "03.12.34", GeographicalArea = "1001", ConditionClass = MeasureHelper.ConditionClass.Rate, RateCode = "R", Formula = "0", Preferences = new List<string> { null }, TariffTypes = new List<string> { "IMP" }, OrderNumber = "123456" },
				new Measure { ItemId = "1234560000", StartDate = new DateTime(2019, 10, 01), EndDate = new DateTime(2050, 12, 31), Description = "Additional",  CompositeKey = "03.12.34", GeographicalArea = "1001", ConditionClass = MeasureHelper.ConditionClass.Rate, RateCode = "R", Formula = "0", Preferences = new List<string> { null }, TariffTypes = new List<string> { "IMP" }, AdditionalCode = "123", AdditionalCodeType = "A" },
				new Measure { ItemId = "1234560000", StartDate = new DateTime(2019, 10, 01), EndDate = new DateTime(2050, 12, 31), Description = "Trade",       CompositeKey = "03.12.34", GeographicalArea = "2002", ConditionClass = MeasureHelper.ConditionClass.Rate, RateCode = "R", Formula = "0", Preferences = new List<string> { null }, TariffTypes = new List<string> { "IMP" } },
				new Measure { ItemId = "1234560000", StartDate = new DateTime(2019, 10, 01), EndDate = new DateTime(2050, 12, 31), Description = "Excl 1",      CompositeKey = "03.12.34", GeographicalArea = "1001", ConditionClass = MeasureHelper.ConditionClass.Rate, RateCode = "R", Formula = "0", Preferences = new List<string> { null }, TariffTypes = new List<string> { "IMP" } }.SetExcludedGeographicalAreas(new[] { new ExcludedGeographicalArea { Value = "AB", HJID = "1" }, new ExcludedGeographicalArea { Value = "CD", HJID = "2" } }),
				new Measure { ItemId = "1234560000", StartDate = new DateTime(2019, 10, 01), EndDate = new DateTime(2050, 12, 31), Description = "Excl 2",      CompositeKey = "03.12.34", GeographicalArea = "1001", ConditionClass = MeasureHelper.ConditionClass.Rate, RateCode = "R", Formula = "0", Preferences = new List<string> { null }, TariffTypes = new List<string> { "IMP" } }.SetExcludedGeographicalAreas(new[] { new ExcludedGeographicalArea { Value = "AB", HJID = "1" }, new ExcludedGeographicalArea { Value = "EF", HJID = "2" } }),
				new Measure { ItemId = "1234560000", StartDate = new DateTime(2019, 10, 01), EndDate = new DateTime(2050, 12, 31), Description = "Nothing new", CompositeKey = "03.12.34", GeographicalArea = "1001", ConditionClass = MeasureHelper.ConditionClass.Rate, RateCode = "R", Formula = "0", Preferences = new List<string> { null }, TariffTypes = new List<string> { "IMP" } }
			};

			var refModels = builder.ConvertToRefModels(measures);
			Assert.That(refModels.Count, Is.EqualTo(2));
			var t = refModels.First();

			Assert.That(t.RefCusRates, Is.Not.Null);
			Assert.That(t.RefCusRates.Count, Is.EqualTo(1));
			Assert.That(t.RefCusRates[0].RefCusApplicabilities, Is.Not.Null);
			Assert.That(t.RefCusRates[0].RefCusApplicabilities.Count, Is.EqualTo(6));
		}

		[Test]
		public void ConditionValueTypes()
		{
			var measure1 = new Measure
			{
				ItemId = "1234560000",
				CleanId = "123456",
				StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
				EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
				Description = "Test Model Convertion 1.1",
				CompositeKey = "03.12.34",
				OrderNumber = "ORD12345",
				AdditionalCode = "123",
				AdditionalCodeType = "4",
				GeographicalArea = "1001",
				Formula = "RateFormula",
				MeasureType = "103",
				MeasureTypeSeries = "C",
				MeasureTypeDescription = "Third country duty",
				RateType = "DTY",
				RateCode = "A00",
				Preferences = new List<string> { "100" },
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				TariffTypes = new List<string> { "IMP" },
			}
			.SetConditions(new[]
			{
				new MeasureCondition { ConditionCode = "A", ConditionCodeDescription = "FRM Group", Formula = "F001", HJID = "1" },
				new MeasureCondition { ConditionCode = "A", ConditionCodeDescription = "FRM Group", CertificateCode = "123", CertificateTypeCode = "C", HJID = "2" },

				new MeasureCondition { ConditionCode = "B", ConditionCodeDescription = "SNR Group", Formula = "F002", HJID = "3" },
				new MeasureCondition { ConditionCode = "B", ConditionCodeDescription = "SNR Group", Formula = "F003", HJID = "4" },
			});

			var measure2 = new Measure
			{
				ItemId = "2222222222",
				CleanId = "222222",
				StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
				EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
				Description = "Test Model 2",
				CompositeKey = "03.12.34",
				OrderNumber = "ORD12345",
				AdditionalCode = "123",
				AdditionalCodeType = "4",
				GeographicalArea = "1001",
				Formula = "RateFormula",
				MeasureType = "482",
				MeasureTypeSeries = "C",
				MeasureTypeDescription = "Third country duty",
				RateType = "DTY",
				RateCode = "A00",
				Preferences = new List<string> { "100" },
				ConditionClass = MeasureHelper.ConditionClass.Class,
				TariffTypes = new List<string> { "IMP" },
			}
			.SetConditions(new[]
			{
				new MeasureCondition { ConditionCode = "R", ConditionCodeDescription = "FRM Group", Formula = "F001", HJID = "5" },
				new MeasureCondition { ConditionCode = "R", ConditionCodeDescription = "FRM Group", Formula = "F002", HJID = "6" }
			});

			var refModels = builder.ConvertToRefModels(new List<ITariffModel> { measure1, measure2 });
			Assert.That(refModels.Count, Is.EqualTo(4));
			var t = refModels.First(x => x.ZZ1_TariffCode == "1234560000");

			Assert.That(t.RefCusConditions, Is.Not.Null);
			Assert.That(t.RefCusConditions.Count, Is.EqualTo(2));

			var frmGroup = t.RefCusConditions.FirstOrDefault(x => x.ZX1_Comment.Contains("FRM Group"));
			Assert.That(frmGroup, Is.Not.Null);
			Assert.That(frmGroup.RefCusConditionValues, Is.Not.Null);
			Assert.That(frmGroup.RefCusConditionValues.Count, Is.EqualTo(2));
			Assert.That(frmGroup.RefCusConditionValues.Any(x => x.ZX3_ZX4_NKValueType == "FRM"));
			Assert.That(frmGroup.RefCusConditionValues.Any(x => x.ZX3_ZX4_NKValueType == "SUP"));

			var snrGroup = t.RefCusConditions.FirstOrDefault(x => x.ZX1_Comment.Contains("SNR Group"));
			Assert.That(snrGroup, Is.Not.Null);
			Assert.That(snrGroup.RefCusConditionValues, Is.Not.Null);
			Assert.That(snrGroup.RefCusConditionValues.Count, Is.EqualTo(2));
			Assert.That(snrGroup.RefCusConditionValues.All(x => x.ZX3_ZX4_NKValueType == "SNR"));

			t = refModels.First(x => x.ZZ1_TariffCode == "2222222222");
			Assert.That(t.RefCusConditions, Is.Not.Null);
			Assert.That(t.RefCusConditions.Count, Is.EqualTo(1));
			var con = t.RefCusConditions.First();
			Assert.That(con.RefCusConditionValues, Is.Not.Null);
			Assert.That(con.RefCusConditionValues.Count, Is.EqualTo(1));
			Assert.That(con.RefCusConditionValues.First().ZX3_ZX4_NKValueType == "SNR");
		}

		[Test]
		public void TariffFilter()
		{
			var tariff = new RefCusTariff();

			Assert.That(builder.TariffFilter_Exposed(tariff), Is.EqualTo(true));
			Assert.That(builder.TariffFilter_Exposed(null), Is.EqualTo(true));
		}

		[Test]
		public void DataFiltering()
		{
			var dateTimeProvider = new Mock<IDateTimeProvider>();
			var errorCollector = new StringBuilder();
			var filterBuilder = new TariffBuilderTester(dateTimeProvider.Object, errorCollector);

			filterBuilder.TestFilter = (RefCusTariff tariff) => tariff.ZZ1_TariffCode.Contains("ABC");

			var tariffs = new List<RefCusTariff>
			{
				new RefCusTariff { ZZ1_TariffCode = "ABC123" },
				new RefCusTariff { ZZ1_TariffCode = "BYEBYE" },
				new RefCusTariff { ZZ1_TariffCode = "123ABC" }
			};

			var results = filterBuilder.FilterData(tariffs);

			Assert.That(results.Count, Is.EqualTo(2));
			Assert.That(results.Any(x => x.ZZ1_TariffCode == "BYEBYE"), Is.EqualTo(false));
		}

		[Test]
		public void CreateOppositeTariffForOneSidedMeasures()
		{
			var measure1 = new Measure
			{
				ItemId = "1234560000",
				CleanId = "123456",
				StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
				EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
				Description = "Test Model Convertion 1.1",
				CompositeKey = "03.12.34",
				OrderNumber = "ORD12345",
				AdditionalCode = "123",
				AdditionalCodeType = "4",
				GeographicalArea = "1001",
				Formula = "RateFormula",
				MeasureType = "103",
				MeasureTypeSeries = "C",
				MeasureTypeDescription = "Third country duty",
				RateType = "DTY",
				RateCode = "A00",
				Preferences = new List<string> { "100" },
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				TariffTypes = new List<string> { "IMP" },
			}
			.SetConditions(new[]
			{
				new MeasureCondition { ConditionCode = "A", ConditionCodeDescription = "FRM Group", Formula = "F001", HJID = "1" },
			});

			var measure2 = new Measure
			{
				ItemId = "2222222222",
				CleanId = "222222",
				StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
				EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
				Description = "Test Model 2",
				CompositeKey = "03.12.34",
				OrderNumber = "ORD12345",
				AdditionalCode = "123",
				AdditionalCodeType = "4",
				GeographicalArea = "1001",
				Formula = "RateFormula",
				MeasureType = "482",
				MeasureTypeSeries = "C",
				MeasureTypeDescription = "Third country duty",
				RateType = "DTY",
				RateCode = "A00",
				Preferences = new List<string> { "100" },
				ConditionClass = MeasureHelper.ConditionClass.Class,
				TariffTypes = new List<string> { "EXP" },
			}
			.SetConditions(new[]
			{
				new MeasureCondition { ConditionCode = "R", ConditionCodeDescription = "FRM Group", Formula = "F001", HJID = "2" }
			});


			var refModels = builder.ConvertToRefModels(new List<ITariffModel> { measure1, measure2 });
			Assert.That(refModels.Count, Is.EqualTo(4));

			Assert.That(refModels.Any(x => x.ZZ1_TariffCode == "1234560000" && x.ZZ1_ZZI_NKTariffType == "IMP"), Is.EqualTo(true));
			Assert.That(refModels.Any(x => x.ZZ1_TariffCode == "12345600" && x.ZZ1_ZZI_NKTariffType == "EXP"), Is.EqualTo(true));
			Assert.That(refModels.Any(x => x.ZZ1_TariffCode == "2222222222" && x.ZZ1_ZZI_NKTariffType == "IMP"), Is.EqualTo(true));
			Assert.That(refModels.Any(x => x.ZZ1_TariffCode == "22222222" && x.ZZ1_ZZI_NKTariffType == "EXP"), Is.EqualTo(true));
		}

		[Test]
		public void CreatePrimaryUOMForOneSidedMeasures()
		{
			var measure1 = new Measure
			{
				ItemId = "1234560000",
				CleanId = "123456",
				StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
				EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
				Description = "Test Model Convertion 1.1",
				CompositeKey = "03.12.34",
				GeographicalArea = "1001",
				MeasureType = "103",
				MeasureTypeSeries = "C",
				MeasureTypeDescription = "Third country duty",
				RateType = "DTY",
				RateCode = "A00",
				Preferences = new List<string> { "100" },
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				TariffTypes = new List<string> { "IMP" },
			}
			.SetComponents(new[]
			{
				new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "2", DutyExpression = "99", HJID = "1" }
			});

			var tariffModels = new List<ITariffModel>() { measure1 };

			var refModels = builder.ConvertToRefModels(tariffModels);
			Assert.That(refModels.Count, Is.EqualTo(2));

			var imp = refModels.FirstOrDefault(x => x.ZZ1_ZZI_NKTariffType == "IMP");
			Assert.That(imp, Is.Not.Null);
			Assert.That(imp.RefCusTariffUOMs, Is.Not.Null);
			Assert.That(imp.RefCusTariffUOMs.Count, Is.EqualTo(2));
			Assert.That(imp.RefCusTariffUOMs.Any(x => x.ZZ8_UOM == "KGM" && x.ZZ8_Type == "CU1" && x.ZZ8_ZZA_NKTradeGroup == null));
			Assert.That(imp.RefCusTariffUOMs.Any(x => x.ZZ8_UOM == "UOM2" && x.ZZ8_Type == "CU2"));

			var exp = refModels.FirstOrDefault(x => x.ZZ1_ZZI_NKTariffType == "EXP");
			Assert.That(exp, Is.Not.Null);
			Assert.That(exp.RefCusTariffUOMs, Is.Not.Null);
			Assert.That(exp.RefCusTariffUOMs.Count, Is.EqualTo(1));
			Assert.That(exp.RefCusTariffUOMs.Any(x => x.ZZ8_UOM == "KGM" && x.ZZ8_Type == "CU1" && x.ZZ8_ZZA_NKTradeGroup == null));
		}

		[Test]
		public void CertificateWithDutyCreatesRateAndCondition()
		{
			var measure1 = new Measure
			{
				ItemId = "1234560000",
				StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
				EndDate = new DateTime(2050, 12, 31, 23, 59, 59),
				Description = "Condition and Rate test",
				CompositeKey = "03.12.34",
				GeographicalArea = "1001",
				Formula = "RateFormula",
				MeasureType = "103",
				MeasureTypeSeries = "A",
				MeasureTypeDescription = "Third country duty",
				RateType = "DTY",
				RateCode = "A00",
				Preferences = new List<string> { null },
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				TariffTypes = new List<string> { "IMP" },
			}
			.SetConditions(new[]
			{
				new MeasureCondition
				{
					SequenceNumber = 1,
					ConditionCode = "C",
					ConditionCodeDescription = "Condition Code Type C",
					MeasureAction = "01",
					DutyAmount = 5.0m,
					CertificateCode = "002",
					CertificateTypeCode = "D",
					IsCertificate = true,
					IsRateFormulaCondition = true,
					HJID = "1"
				},
				new MeasureCondition
				{
					SequenceNumber = 2,
					ConditionCode = "C",
					ConditionCodeDescription = "Condition Code Type C",
					MeasureAction = "01",
					DutyAmount = 60.0m,
					IsCertificate = true,
					IsRateFormulaCondition = true,
					HJID = "2"
				}
			});

			var tariffModels = new List<ITariffModel>() { measure1 };

			var refModels = builder.ConvertToRefModels(tariffModels);
			Assert.That(refModels.Count, Is.EqualTo(2));

			var refModel = refModels.First();

			Assert.That(refModel.RefCusRates, Is.Not.Null.And.Not.Empty);

			Assert.That(refModel.RefCusConditions, Is.Not.Null);
			Assert.That(refModel.RefCusConditions.Count, Is.EqualTo(1));
			Assert.That(refModel.RefCusConditions[0].RefCusConditionValues, Is.Not.Null);
			Assert.That(refModel.RefCusConditions[0].RefCusConditionValues.Count, Is.EqualTo(1));
			Assert.That(refModel.RefCusConditions[0].RefCusConditionValues[0].ZX3_Value, Is.EqualTo("D002"));
		}

		[Test]
		public void IncludeValuesWhenCheckingForExistingConditions()
		{
			var measure1 = new Measure
			{
				ItemId = "0404040404",
				CleanId = "0404040404",
				Description = "Control Formula Test",
				CompositeKey = "04.04.04",
				GeographicalArea = "123",
				MeasureType = "755",
				MeasureTypeSeries = "A",
				MeasureTypeDescription = "Condition formula test",
				RateType = "",
				Preferences = new List<string> { null },
				ConditionClass = MeasureHelper.ConditionClass.Control,
				Formula = "",
				TariffTypes = new List<string> { "IMP" },
			}
			.SetConditions(new[]
			{
				new MeasureCondition
				{
					ConditionCode = "B",
					ConditionCodeDescription = "Condition Code Type B",
					MeasureAction = "99",
					CertificateCode = "999",
					CertificateTypeCode = "X",
					HJID = "1"
				},
				new MeasureCondition
				{
					ConditionCode = "B",
					ConditionCodeDescription = "Condition Code Type B",
					MeasureAction = "99",
					CertificateCode = "000",
					CertificateTypeCode = "Z",
					HJID = "2"
				}
			});

			var measure2 = new Measure
			{
				ItemId = "0404040404",
				CleanId = "0404040404",
				Description = "Control Formula Test",
				CompositeKey = "04.04.04",
				GeographicalArea = "XX",
				MeasureType = "755",
				MeasureTypeSeries = "A",
				MeasureTypeDescription = "Condition formula test",
				RateType = "",
				Preferences = new List<string> { null },
				ConditionClass = MeasureHelper.ConditionClass.Control,
				Formula = "",
				TariffTypes = new List<string> { "IMP" },
			}
			.SetConditions(new[]
			{
				new MeasureCondition
				{
					ConditionCode = "B",
					ConditionCodeDescription = "Condition Code Type B",
					MeasureAction = "99",
					CertificateCode = "999",
					CertificateTypeCode = "X",
					HJID = "3"
				}
			});

			var tariffModels = new List<ITariffModel>() { measure1, measure2 };

			var refModels = builder.ConvertToRefModels(tariffModels);
			Assert.That(refModels.Count, Is.EqualTo(2));

			var refModel = refModels.First();

			Assert.That(refModel.RefCusConditions, Is.Not.Null);
			Assert.That(refModel.RefCusConditions.Count, Is.EqualTo(2));
			Assert.That(refModel.RefCusConditions[0].RefCusConditionValues, Is.Not.Null);
			Assert.That(refModel.RefCusConditions[0].RefCusConditionValues.Count, Is.EqualTo(2));

			Assert.That(refModel.RefCusConditions[1].RefCusConditionValues, Is.Not.Null);
			Assert.That(refModel.RefCusConditions[1].RefCusConditionValues.Count, Is.EqualTo(1));
		}

		[Test]
		public void ConsolidateRatesWithSameDates()
		{
			var measure1 = new Measure
			{
				ItemId = "1234560000",
				CleanId = "123456",
				StartDate = new DateTime(2020, 1, 1),
				EndDate = new DateTime(2050, 12, 31, 23, 59, 0),
				Description = "Test Model Convertion 1.3",
				CompositeKey = "03.12.34",
				GeographicalArea = "1111",
				MeasureType = "103",
				MeasureTypeSeries = "A",
				RateType = "DTY",
				Preferences = new List<string> { "100" },
				MeasureTypeDescription = "Third country duty",
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				TariffTypes = new List<string> { "IMP" },
				Formula = "0",
			}
			.SetComponents(new[]
			{
				new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "3", DutyExpression = "99", HJID = "1" }
			});

			var measure2 = new Measure
			{
				ItemId = "1234560000",
				CleanId = "123456",
				StartDate = new DateTime(2019, 06, 01),
				EndDate = new DateTime(2020, 12, 31, 23, 59, 0),
				Description = "Test Model Convertion 1.3",
				CompositeKey = "03.12.34",
				GeographicalArea = "2222",
				MeasureType = "103",
				MeasureTypeSeries = "A",
				RateType = "DTY",
				Preferences = new List<string> { "100" },
				MeasureTypeDescription = "Third country duty",
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				TariffTypes = new List<string> { "IMP" },
				Formula = "0",
			}
			.SetComponents(new[]
			{
				new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "3", DutyExpression = "99", HJID = "2" }
			});

			var measure3 = new Measure
			{
				ItemId = "1234560000",
				CleanId = "123456",
				StartDate = new DateTime(2019, 06, 01),
				EndDate = new DateTime(2020, 12, 31, 23, 59, 0),
				Description = "Test Model Convertion 1.3",
				CompositeKey = "03.12.34",
				GeographicalArea = "3333",
				MeasureType = "103",
				MeasureTypeSeries = "A",
				RateType = "DTY",
				Preferences = new List<string> { "100" },
				MeasureTypeDescription = "Third country duty",
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				TariffTypes = new List<string> { "IMP" },
				Formula = "0",
			}
			.SetComponents(new[]
			{
				new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "3", DutyExpression = "99", HJID = "3" }
			});

			var refModels = builder.ConvertToRefModels(new List<ITariffModel> { measure1, measure2, measure3 });
			Assert.That(refModels.Count, Is.EqualTo(2));
			var t = refModels.First(x => x.ZZ1_ZZI_NKTariffType == "IMP");

			Assert.That(t.RefCusRates, Is.Not.Null);
			Assert.That(t.RefCusRates.Count, Is.EqualTo(1));

			var rate1 = t.RefCusRates.SingleOrDefault(x => x.ZZ2_StartDate == DefaultValues.MinimumDateTime);

			Assert.That(rate1, Is.Not.Null);
			Assert.That(rate1.ZZ2_EndDate, Is.EqualTo(DefaultValues.MaximumDateTime));

			Assert.That(rate1.RefCusApplicabilities, Is.Not.Null);
			Assert.That(rate1.RefCusApplicabilities.Count, Is.EqualTo(3));

			var apl1111 = rate1.RefCusApplicabilities.First(x => x.ZZT_ZZA_NKTradeGroup == "1111");
			Assert.That(apl1111.ZZT_StartDate, Is.EqualTo(measure1.StartDate));
			Assert.That(apl1111.ZZT_EndDate, Is.EqualTo(measure1.EndDate));

			var apl2222 = rate1.RefCusApplicabilities.First(x => x.ZZT_ZZA_NKTradeGroup == "2222");
			Assert.That(apl2222.ZZT_StartDate, Is.EqualTo(measure2.StartDate));
			Assert.That(apl2222.ZZT_EndDate, Is.EqualTo(measure2.EndDate));

			var apl3333 = rate1.RefCusApplicabilities.First(x => x.ZZT_ZZA_NKTradeGroup == "3333");
			Assert.That(apl3333.ZZT_StartDate, Is.EqualTo(measure3.StartDate));
			Assert.That(apl3333.ZZT_EndDate, Is.EqualTo(measure3.EndDate));
		}

		[Test]
		public void ConsolidateConditionsWithSameDates()
		{
			var measure1 = new Measure
			{
				ItemId = "1234560000",
				CleanId = "123456",
				StartDate = new DateTime(2020, 1, 1),
				EndDate = new DateTime(2050, 12, 31, 23, 59, 0),
				Description = "Test Model Convertion 1.3",
				CompositeKey = "03.12.34",
				GeographicalArea = "1111",
				MeasureType = "103",
				MeasureTypeSeries = "A",
				RateType = "DTY",
				Preferences = new List<string> { "100" },
				MeasureTypeDescription = "Third country duty",
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				TariffTypes = new List<string> { "IMP" },
				Formula = "0",
			}
			.SetConditions(new[]
			{
				new MeasureCondition
				{
					ConditionCode = "B",
					ConditionCodeDescription = "Condition Code Type B",
					MeasureAction = "99",
					CertificateCode = "999",
					CertificateTypeCode = "X",
					HJID = "1"
				}
			});

			var measure2 = new Measure
			{
				ItemId = "1234560000",
				CleanId = "123456",
				StartDate = new DateTime(2019, 06, 01),
				EndDate = new DateTime(2020, 12, 31, 23, 59, 0),
				Description = "Test Model Convertion 1.3",
				CompositeKey = "03.12.34",
				GeographicalArea = "2222",
				MeasureType = "103",
				MeasureTypeSeries = "A",
				RateType = "DTY",
				Preferences = new List<string> { "100" },
				MeasureTypeDescription = "Third country duty",
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				TariffTypes = new List<string> { "IMP" },
				Formula = "0",
			}
			.SetConditions(new[]
			{
				new MeasureCondition
				{
					ConditionCode = "B",
					ConditionCodeDescription = "Condition Code Type B",
					MeasureAction = "99",
					CertificateCode = "999",
					CertificateTypeCode = "X",
					HJID = "2"
				}
			});

			var measure3 = new Measure
			{
				ItemId = "1234560000",
				CleanId = "123456",
				StartDate = new DateTime(2019, 06, 01),
				EndDate = new DateTime(2020, 12, 31, 23, 59, 0),
				Description = "Test Model Convertion 1.3",
				CompositeKey = "03.12.34",
				GeographicalArea = "3333",
				MeasureType = "103",
				MeasureTypeSeries = "A",
				RateType = "DTY",
				Preferences = new List<string> { "100" },
				MeasureTypeDescription = "Third country duty",
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				TariffTypes = new List<string> { "IMP" },
				Formula = "0",
			}
			.SetConditions(new[]
			{
				new MeasureCondition
				{
					ConditionCode = "B",
					ConditionCodeDescription = "Condition Code Type B",
					MeasureAction = "99",
					CertificateCode = "999",
					CertificateTypeCode = "X",
					HJID = "3"
				}
			});

			var refModels = builder.ConvertToRefModels(new List<ITariffModel> { measure1, measure2, measure3 });
			Assert.That(refModels.Count, Is.EqualTo(2));
			var t = refModels.First(x => x.ZZ1_ZZI_NKTariffType == "IMP");

			Assert.That(t.RefCusConditions, Is.Not.Null);
			Assert.That(t.RefCusConditions.Count, Is.EqualTo(1));

			var con1 = t.RefCusConditions.SingleOrDefault(x => x.ZX1_StartDate == DefaultValues.MinimumDateTime);

			Assert.That(con1, Is.Not.Null);
			Assert.That(con1.ZX1_EndDate, Is.EqualTo(DefaultValues.MaximumDateTime));

			Assert.That(con1.RefCusApplicabilities, Is.Not.Null);
			Assert.That(con1.RefCusApplicabilities.Count, Is.EqualTo(3));

			var apl1111 = con1.RefCusApplicabilities.First(x => x.ZZT_ZZA_NKTradeGroup == "1111");
			Assert.That(apl1111.ZZT_StartDate, Is.EqualTo(measure1.StartDate));
			Assert.That(apl1111.ZZT_EndDate, Is.EqualTo(measure1.EndDate));

			var apl2222 = con1.RefCusApplicabilities.First(x => x.ZZT_ZZA_NKTradeGroup == "2222");
			Assert.That(apl2222.ZZT_StartDate, Is.EqualTo(measure2.StartDate));
			Assert.That(apl2222.ZZT_EndDate, Is.EqualTo(measure2.EndDate));

			var apl3333 = con1.RefCusApplicabilities.First(x => x.ZZT_ZZA_NKTradeGroup == "3333");
			Assert.That(apl3333.ZZT_StartDate, Is.EqualTo(measure3.StartDate));
			Assert.That(apl3333.ZZT_EndDate, Is.EqualTo(measure3.EndDate));
		}

		[Test]
		public void ApplyAdditionalRefCusApplicabilities()
		{
			var measure1 = new Measure
			{
				ItemId = "1234560000",
				CleanId = "123456",
				StartDate = new DateTime(2020, 1, 1),
				EndDate = new DateTime(2050, 12, 31, 23, 59, 0),
				Description = "Additonal Info 1",
				CompositeKey = "03.12.34",
				GeographicalArea = "1111",
				MeasureType = "103",
				MeasureTypeSeries = "A",
				RateType = "DTY",
				Preferences = new List<string> { "100" },
				MeasureTypeDescription = "Third country duty",
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				TariffTypes = new List<string> { "IMP", "EXP" },
				AdditionalCode = "999",
				AdditionalCodeType = "A"
			};

			var measure2 = new Measure
			{
				ItemId = "1234560000",
				CleanId = "123456",
				StartDate = new DateTime(2020, 1, 1),
				EndDate = new DateTime(2050, 12, 31, 23, 59, 0),
				Description = "Additonal Info 2 With Order No",
				CompositeKey = "03.12.34",
				GeographicalArea = "1111",
				MeasureType = "103",
				MeasureTypeSeries = "A",
				RateType = "DTY",
				Preferences = new List<string> { "100" },
				MeasureTypeDescription = "Third country duty",
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				TariffTypes = new List<string> { "IMP" },
				AdditionalCode = "999",
				AdditionalCodeType = "A",
				OrderNumber = "ORDER NO 1"
			};

			var measure3 = new Measure
			{
				ItemId = "1234560000",
				CleanId = "123456",
				StartDate = new DateTime(2020, 1, 1),
				EndDate = new DateTime(2050, 12, 31, 23, 59, 0),
				Description = "Additonal Info Wrong Geo",
				CompositeKey = "03.12.34",
				GeographicalArea = "2222",
				MeasureType = "103",
				MeasureTypeSeries = "A",
				RateType = "DTY",
				Preferences = new List<string> { "100" },
				MeasureTypeDescription = "Third country duty",
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				TariffTypes = new List<string> { "IMP", "EXP" },
				AdditionalCode = "999",
				AdditionalCodeType = "A",
				OrderNumber = "ORDER NO"
			};

			var measure4 = new Measure
			{
				ItemId = "1234560000",
				CleanId = "123456",
				StartDate = new DateTime(2052, 1, 1),
				EndDate = new DateTime(2060, 12, 31, 23, 59, 0),
				Description = "Additonal Info wrong date range",
				CompositeKey = "03.12.34",
				GeographicalArea = "1111",
				MeasureType = "103",
				MeasureTypeSeries = "A",
				RateType = "DTY",
				Preferences = new List<string> { "100" },
				MeasureTypeDescription = "Third country duty",
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				TariffTypes = new List<string> { "IMP", "EXP" },
				AdditionalCode = "888",
				AdditionalCodeType = "A"
			};

			var measure5 = new Measure
			{
				ItemId = "1234560000",
				CleanId = "123456",
				StartDate = new DateTime(2020, 1, 1),
				EndDate = new DateTime(2050, 12, 31, 23, 59, 0),
				Description = "Actual Rate",
				CompositeKey = "03.12.34",
				GeographicalArea = "1111",
				MeasureType = "103",
				MeasureTypeSeries = "A",
				RateType = "DTY",
				Preferences = new List<string> { "100" },
				MeasureTypeDescription = "Third country duty",
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				TariffTypes = new List<string> { "IMP" },
				Formula = "HasAFormula",
			}
			.SetComponents(new[]
			{
				new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "3", DutyExpression = "99", HJID = "1" }
			});

			var measure6 = new Measure
			{
				ItemId = "1234560000",
				CleanId = "123456",
				StartDate = new DateTime(2020, 1, 1),
				EndDate = new DateTime(2050, 12, 31, 23, 59, 0),
				Description = "Actual Condition",
				CompositeKey = "03.12.34",
				GeographicalArea = "1111",
				MeasureType = "103",
				MeasureTypeSeries = "A",
				RateType = "DTY",
				Preferences = new List<string> { "100" },
				MeasureTypeDescription = "Third country duty",
				ConditionClass = MeasureHelper.ConditionClass.Control,
				TariffTypes = new List<string> { "EXP" },
			}
			.SetConditions(new[]
			{
				new MeasureCondition
				{
					ConditionCode = "B",
					ConditionCodeDescription = "Condition Code Type B",
					MeasureAction = "99",
					CertificateCode = "999",
					CertificateTypeCode = "X",
					HJID = "2"
				}
			});

			var measure7 = new Measure
			{
				ItemId = "1234560000",
				CleanId = "123456",
				StartDate = new DateTime(2020, 1, 1),
				EndDate = new DateTime(2050, 12, 31, 23, 59, 0),
				Description = "Additonal Info 1 - Duplicate",
				CompositeKey = "03.12.34",
				GeographicalArea = "1111",
				MeasureType = "103",
				MeasureTypeSeries = "A",
				RateType = "DTY",
				Preferences = new List<string> { "100" },
				MeasureTypeDescription = "Third country duty",
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				TariffTypes = new List<string> { "IMP" },
				AdditionalCode = "999",
				AdditionalCodeType = "A"
			};

			var measure8 = new Measure
			{
				ItemId = "1234560000",
				CleanId = "123456",
				StartDate = new DateTime(2020, 1, 1),
				EndDate = new DateTime(2050, 12, 31, 23, 59, 0),
				Description = "Additonal Info 2 With Order No 2",
				CompositeKey = "03.12.34",
				GeographicalArea = "1111",
				MeasureType = "103",
				MeasureTypeSeries = "A",
				RateType = "DTY",
				Preferences = new List<string> { "100" },
				MeasureTypeDescription = "Third country duty",
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				TariffTypes = new List<string> { "EXP" },
				AdditionalCode = "999",
				AdditionalCodeType = "A",
				OrderNumber = "ORDER NO 2"
			};

			var refModels = builder.ConvertToRefModels(new List<ITariffModel> { measure1, measure2, measure3, measure4, measure5, measure6, measure7, measure8 });
			Assert.That(refModels.Count, Is.EqualTo(2));
			var t = refModels.First(x => x.ZZ1_ZZI_NKTariffType == "IMP");

			Assert.That(t.RefCusRates, Is.Not.Null);
			Assert.That(t.RefCusRates.Count, Is.EqualTo(1));
			Assert.That(t.RefCusConditions, Is.Null.Or.Empty);

			Assert.That(t.RefCusRates[0].RefCusApplicabilities, Is.Not.Null);
			Assert.That(t.RefCusRates[0].RefCusApplicabilities.Count, Is.EqualTo(3));

			Assert.That(t.RefCusRates[0].RefCusApplicabilities.Any(x => x.ZZT_ZZA_NKTradeGroup == "1111" && string.IsNullOrEmpty(x.ZZT_AdditionalCode) && string.IsNullOrEmpty(x.ZZT_OrderNumber)), Is.EqualTo(true));
			Assert.That(t.RefCusRates[0].RefCusApplicabilities.Any(x => x.ZZT_ZZA_NKTradeGroup == "1111" && x.ZZT_AdditionalCode == "A999" && string.IsNullOrEmpty(x.ZZT_OrderNumber)), Is.EqualTo(true));
			Assert.That(t.RefCusRates[0].RefCusApplicabilities.Any(x => x.ZZT_ZZA_NKTradeGroup == "1111" && x.ZZT_AdditionalCode == "A999" && x.ZZT_OrderNumber == "ORDER NO 1"), Is.EqualTo(true));

			t = refModels.First(x => x.ZZ1_ZZI_NKTariffType == "EXP");

			Assert.That(t.RefCusConditions, Is.Not.Null);
			Assert.That(t.RefCusConditions.Count, Is.EqualTo(1));
			Assert.That(t.RefCusRates, Is.Null.Or.Empty);

			Assert.That(t.RefCusConditions[0].RefCusApplicabilities, Is.Not.Null);
			Assert.That(t.RefCusConditions[0].RefCusApplicabilities.Count, Is.EqualTo(4));

			Assert.That(t.RefCusConditions[0].RefCusApplicabilities.Any(x => x.ZZT_ZZA_NKTradeGroup == "1111" && string.IsNullOrEmpty(x.ZZT_AdditionalCode) && string.IsNullOrEmpty(x.ZZT_OrderNumber)), Is.EqualTo(true));
			Assert.That(t.RefCusConditions[0].RefCusApplicabilities.Any(x => x.ZZT_ZZA_NKTradeGroup == "1111" && x.ZZT_AdditionalCode == "A999" && string.IsNullOrEmpty(x.ZZT_OrderNumber)), Is.EqualTo(true));
			Assert.That(t.RefCusConditions[0].RefCusApplicabilities.Any(x => x.ZZT_ZZA_NKTradeGroup == "1111" && x.ZZT_AdditionalCode == "A999" && x.ZZT_OrderNumber == "ORDER NO 2"), Is.EqualTo(true));
			Assert.That(t.RefCusConditions[0].RefCusApplicabilities.Any(x => x.ZZT_ZZA_NKTradeGroup == "1111" && x.ZZT_AdditionalCode == "A888" && string.IsNullOrEmpty(x.ZZT_OrderNumber)), Is.EqualTo(true));
		}

		[Test]
		public void SupplementaryUnits()
		{
			var measure2 = new Measure
			{
				HJID = "12540754",
				ItemId = "3921905590",
				CleanId = "3921905590",
				StartDate = new DateTime(2021, 5, 20),
				EndDate = new DateTime(2028, 12, 31, 23, 59, 0),
				Description = "Rate with 2701",
				CompositeKey = "07.39.02.21.9.10.40.100",
				GeographicalArea = "1011",
				MeasureType = "103",
				MeasureTypeSeries = "C",
				RateType = "DTY",
				Preferences = new List<string> { "100" },
				MeasureTypeDescription = "Third country duty",
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				TariffTypes = new List<string> { "IMP" },
				Formula = "VFD * 0.060",
				AdditionalCode = "701",
				AdditionalCodeType = "2",
			}
			.SetComponents(new[]
			{
				new MeasureComponent { DutyAmount = 6, DutyExpression = "01", HJID = "1" }
			});

			var measure3 = new Measure
			{
				HJID = "12540757",
				ItemId = "3921905590",
				CleanId = "3921905590",
				StartDate = new DateTime(2021, 5, 20),
				EndDate = new DateTime(2028, 12, 31, 23, 59, 0),
				Description = "Rate with 2700 and N990",
				CompositeKey = "07.39.02.21.9.10.40.100",
				GeographicalArea = "1011",
				MeasureType = "103",
				MeasureTypeSeries = "C",
				RateType = "DTY",
				Preferences = new List<string> { "100" },
				MeasureTypeDescription = "Third country duty",
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				TariffTypes = new List<string> { "IMP" },
				Formula = "0",
				AdditionalCode = "700",
				AdditionalCodeType = "2",
			}
			.SetComponents(new[]
			{
				new MeasureComponent { DutyAmount = 0, DutyExpression = "01", HJID = "2" }
			})
			.SetConditions(new[]
			{
				new MeasureCondition
				{
					ConditionCode = "B",
					ConditionCodeDescription = "N990",
					MeasureAction = "27",
					CertificateCode = "990",
					CertificateTypeCode = "N",
					HJID = "3"
				},
				new MeasureCondition
				{
					ConditionCode = "B",
					ConditionCodeDescription = "B",
					MeasureAction = "08",
					HJID = "4"
				}
			});

			var measure4 = new Measure
			{
				HJID = "12572550",
				ItemId = "3921905590",
				CleanId = "3921905590",
				StartDate = new DateTime(2021, 5, 20),
				EndDate = new DateTime(4712, 12, 31, 23, 59, 0),
				Description = "Rate With No Conditions",
				CompositeKey = "07.39.02.21.9.10.40.100",
				GeographicalArea = "1011",
				MeasureType = "103",
				MeasureTypeSeries = "C",
				RateType = "DTY",
				Preferences = new List<string> { "100" },
				MeasureTypeDescription = "Third country duty",
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				TariffTypes = new List<string> { "IMP" },
				Formula = "VFD * 0.060",
			}
			.SetComponents(new[]
			{
				new MeasureComponent { DutyAmount = 6, DutyExpression = "01", HJID = "5" }
			});

			var measure5 = new Measure
			{
				HJID = "14309108",
				ItemId = "3921905590",
				CleanId = "3921905590",
				StartDate = new DateTime(2024, 7, 1),
				EndDate = new DateTime(2028, 12, 31, 23, 59, 0),
				Description = "Supplementary unit",
				CompositeKey = "07.39.02.21.9.10.40.100",
				GeographicalArea = "1011",
				MeasureType = "110",
				MeasureTypeSeries = "O",
				RateType = string.Empty,
				Preferences = new List<string> { null },
				MeasureTypeDescription = "Supplementary unit import",
				ConditionClass = string.Empty,
				TariffTypes = new List<string> { "IMP" },
				IsSupplementaryUnit = true,
			}
			.SetComponents(new[]
			{
				new MeasureComponent { MeasurementUnit = "MTK", MeasurementUnitQualifier = string.Empty, DutyExpression = "99", HJID = "6" }
			});

			var refModels = builder.ConvertToRefModels(new List<ITariffModel> { measure2, measure3, measure4, measure5 }).ToList();
			RefCusTariff t = null;
			Assert.DoesNotThrow(() => { t = refModels.Single(x => x.ZZ1_ZZI_NKTariffType == "IMP"); }, "ConvertToRefModels expected to return a single IMP record");

			Assert.That(t.RefCusRates, Is.Not.Null, "RefCusRates");
			Assert.That(t.RefCusRates.Count, Is.EqualTo(2), "RefCusRates.Count");
			var sixPercentRate = t.RefCusRates.FirstOrDefault(x => x.ZZ2_RateFormula == "VFD * 0.060");
			Assert.That(sixPercentRate, Is.Not.Null, "RefCusRates should include a rate with formula VFD * 0.060");
			Assert.That(sixPercentRate.RefCusApplicabilities, Is.Not.Null, "sixPercentRate.RefCusApplicabilities");
			Assert.That(sixPercentRate.RefCusApplicabilities.Count, Is.EqualTo(2), "sixPercentRate.RefCusApplicabilities.Count");
			Assert.That(sixPercentRate.RefCusApplicabilities, Does.Contain((object)string.Empty).Using((RefCusApplicability a, string b) => a.ZZT_AdditionalCode == b), "sixPercentRate.RefCusApplicabilities");
			Assert.That(sixPercentRate.RefCusApplicabilities, Does.Contain((object)"2701").Using((RefCusApplicability a, string b) => a.ZZT_AdditionalCode == b), "sixPercentRate.RefCusApplicabilities");
			var zeroRate = t.RefCusRates.FirstOrDefault(x => x.ZZ2_RateFormula == "0");
			Assert.That(zeroRate, Is.Not.Null, "RefCusRates should include a rate with formula 0");
			Assert.That(zeroRate.RefCusApplicabilities, Is.Not.Null, "zeroRate.RefCusApplicabilities");
			Assert.That(zeroRate.RefCusApplicabilities.Count, Is.EqualTo(1), "zeroRate.RefCusApplicabilities.Count");
			Assert.That(zeroRate.RefCusApplicabilities, Does.Contain((object)"2700").Using((RefCusApplicability a, string b) => a.ZZT_AdditionalCode == b), "zeroRate.RefCusApplicabilities");

			Assert.That(t.RefCusConditions, Is.Not.Null, "RefCusConditions");
			Assert.That(t.RefCusConditions.Count, Is.EqualTo(1), "RefCusConditions.Count");
			Assert.That(t.RefCusConditions[0].ZX1_Comment, Is.EqualTo("Condition B: N990"), "RefCusConditions[0].ZX1_Comment");
			Assert.That(t.RefCusConditions[0].RefCusApplicabilities, Is.Not.Null, "RefCusConditions[0].RefCusApplicabilities");
			Assert.That(t.RefCusConditions[0].RefCusApplicabilities.Count, Is.EqualTo(1), "RefCusConditions[0].RefCusApplicabilities.Count");
			Assert.That(t.RefCusConditions[0].RefCusApplicabilities[0].ZZT_AdditionalCode, Is.EqualTo("2700"), "RefCusConditions[0].RefCusApplicabilities[0].ZZT_AdditionalCode");

			Assert.That(t.RefCusTariffUOMs, Is.Not.Null, "RefCusTariffUOMs");
			Assert.That(t.RefCusTariffUOMs.Count, Is.EqualTo(2), "RefCusTariffUOMs.Count");
			var cu1 = t.RefCusTariffUOMs.FirstOrDefault(x => x.ZZ8_Type == "CU1");
			Assert.That(cu1, Is.Not.Null, "RefCusTariffUOMs should include CU1");
			Assert.That(cu1.ZZ8_UOM, Is.EqualTo("KGM"), "CU1.ZZ8_UOM");
			Assert.That(cu1.ZZ8_ZZA_NKTradeGroup, Is.Null, "CU1.ZZ8_ZZA_NKTradeGroup");

			var cu2 = t.RefCusTariffUOMs.FirstOrDefault(x => x.ZZ8_Type == "CU2");
			Assert.That(cu2, Is.Not.Null, "RefCusTariffUOMs should include CU2");
			Assert.That(cu2.ZZ8_UOM, Is.EqualTo("MTK"), "CU2.ZZ8_UOM");
			Assert.That(cu2.ZZ8_ZZA_NKTradeGroup, Is.EqualTo("1011"), "CU2.ZZ8_ZZA_NKTradeGroup");
		}

		[Test]
		public void IsEntityTypeConfigurationRefCusTariffEnableNullOrEmptyKeyMatching()
		{
			Assert.That(builder.IsEntityTypeConfigurationRefCusTariffEnableNullOrEmptyKeyMatching, Is.EqualTo(true));
		}

		[Test]
		public void ParentDataGrouping()
		{
			var dateTimeProvider = new Mock<IDateTimeProvider>();
			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2019, 11, 04, 22, 36, 45, 135));
			dateTimeProvider.Setup(x => x.UTCHistoricalDate).Returns(new DateTime(2018, 11, 04, 0, 0, 0, 0));
			errorCollector = new StringBuilder();
			var tariffBuilder = new TariffBuilderTesterForDataGrouping(dateTimeProvider.Object, errorCollector);

			Assert.That(tariffBuilder.ParentDataGrouping, Is.EqualTo("XYZ"));
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			var dateTimeProvider = new Mock<IDateTimeProvider>();
			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2019, 11, 04, 22, 36, 45, 135));
			dateTimeProvider.Setup(x => x.UTCHistoricalDate).Returns(new DateTime(2018, 11, 04, 0, 0, 0, 0));
			errorCollector = new StringBuilder();
			builder = new TariffBuilderTester(dateTimeProvider.Object, errorCollector);
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		[SetUp]
		public void Setup()
		{
			errorCollector.Clear();
		}

		TariffBuilderTester builder;
		StringBuilder errorCollector;
		string TempFolder;
	}

	internal class TariffBuilderTester : TariffBuilderBase
	{
		public TariffBuilderTester(IDateTimeProvider dateTimeProvider, StringBuilder errorCollector) : base(dateTimeProvider, errorCollector)
		{
		}

		public new IEnumerable<RefCusTariff> ConvertToRefModels(List<ITariffModel> data) => base.ConvertToRefModels(data);
		public new XmlWriterConfiguration XmlWriterConfiguration() => base.XmlWriterConfiguration();
		public new bool IsValid(RefCusTariff refModel, string source) => base.IsValid(refModel, source);
		public new bool IsExpired(RefCusTariff refModel) => base.IsExpired(refModel);
		public new void DuplicateError(RefCusTariff refModel, string uniqueId, string source) => base.DuplicateError(refModel, uniqueId, source);
		public new string UniqueId(RefCusTariff refModel) => base.UniqueId(refModel);
		public new string OutputFileName => base.OutputFileName;
		public Predicate<RefCusTariff> TariffFilter_Exposed => TariffFilter;
		protected override Predicate<RefCusTariff> TariffFilter => TestFilter == null ? base.TariffFilter : TestFilter;
		public Predicate<RefCusTariff> TestFilter { get; set; }
		public new List<RefCusTariff> FilterData(List<RefCusTariff> data) => base.FilterData(data);

		protected override string DataGrouping => "ABC";
		protected override string ParentDataGrouping => "EUN";
		protected override string FilePrefix => "UTTariff";
		protected override string XMLWriterDataSource => "ABC Tariff";
		public new bool IsEntityTypeConfigurationRefCusTariffEnableNullOrEmptyKeyMatching => base.IsEntityTypeConfigurationRefCusTariffEnableNullOrEmptyKeyMatching;
	}

	internal class TariffBuilderTesterForDataGrouping : TariffBuilderBase
	{
		public TariffBuilderTesterForDataGrouping(IDateTimeProvider dateTimeProvider, StringBuilder errorCollector) : base(dateTimeProvider, errorCollector)
		{
		}

		protected override string DataGrouping => "XYZ";
		public new string ParentDataGrouping => base.ParentDataGrouping;
		protected override string FilePrefix => "UTTariff";
		protected override string XMLWriterDataSource => "ABC Tariff";
	}
}
