using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Helpers.Tests;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using CargoWise.RefDbRepo.SharedReferenceData.Tests;
using NUnit.Framework;
using static CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Helpers.Tests.TestHelperClasses;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Common.CommonHelper;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors.Tests
{
	[TestFixture]
	class MeasureProcessorTests
	{
		[Test]
		public void ProcessChapters()
		{
			var filePath = Path.Combine(ContentFolder, "UT_Measure_001.xml");
			TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Business.Tariff.TestFiles.Input.UT_Measure_001.xml");

			var file = new FileDetails { Content = new ContentDetails { ExecutionDate = new DateTime(2019, 10, 03, 14, 15, 16) }, Filename = filePath };
			var files = new List<FileDetails>() { file };

			var errorCollector = new StringBuilder();
			var processor = new MeasureProcessorTester();
			processor.SimulateProcessing("", files, errorCollector, "");

			Assert.That(processor.TestBuilders.First(), Is.Not.Null);
			Assert.That(processor.TestBuilders.First().PublicationDate, Is.EqualTo(file.Content.ExecutionDate));
			Assert.That(processor.TestBuilders.First().BuildCount, Is.EqualTo(5));
			Assert.That(errorCollector.ToString(), Does.Contain("RegulationId is required"));

			errorCollector.Clear();
			processor = new MeasureProcessorTester();
			processor.SimulateProcessing("21", files, errorCollector, "");

			Assert.That(processor.TestBuilders.First().BuildCount, Is.EqualTo(1));
			Assert.That(errorCollector.ToString(), Is.Empty);

			errorCollector.Clear();
			processor = new MeasureProcessorTester();
			processor.SimulateProcessing("17", files, errorCollector, "");
			Assert.That(processor.TestBuilders.First().BuildCount, Is.EqualTo(0));
			Assert.That(errorCollector.ToString(), Does.Contain("RegulationId is required"));
		}

		[Test]
		public void ModelUpdates()
		{
			var filePath = Path.Combine(ContentFolder, "UT_Measure_001.xml");
			TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Business.Tariff.TestFiles.Input.UT_Measure_001.xml");

			var file = new FileDetails { Content = new ContentDetails { ExecutionDate = new DateTime(2019, 10, 03, 14, 15, 16) }, Filename = filePath };
			var files = new List<FileDetails>() { file };

			var errorCollector = new StringBuilder();
			var mappingProvider = new MeasureMappingTestDataProvider();

			mappingProvider.MeasureTypeMappingsForTest = new Dictionary<string, MeasureTypeMapping>
			{
				{ "110", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty, SupplementaryUnit = true } },
				{ "690", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Countervailing, RateCode = "A45" } }
			};
			var processor = new MeasureProcessorTester(mappingProvider);

			processor.LoadData("", files, errorCollector);
			Assert.That(processor.Models.Count, Is.EqualTo(7));

			processor.UpdateModels("", TestHelperClasses.TestData.CreateReferenceData(), errorCollector);

			Assert.That(processor.Models.Count, Is.EqualTo(5));

			var models = processor.Models.Cast<Measure>().ToList();
			Assert.That(models[0].Description, Is.EqualTo("Measure 001"));
			Assert.That(models[0].Formula, Is.EqualTo("0"));
			Assert.That(models[0].CompositeKey, Is.EqualTo("03.01.02.03"));
			Assert.That(models[0].CleanId, Is.EqualTo("0811109010"));
			Assert.That(models[0].ConditionClass, Is.EqualTo("RATE"));
			Assert.That(models[0].MeasureTypeDescription, Is.EqualTo("UT Measure Type 690"));
			Assert.That(models[0].MeasureTypeSeries, Is.EqualTo("J"));
			Assert.That(models[0].RateType, Is.EqualTo("CVD"));
			Assert.That(models[0].RateCode, Is.EqualTo("A45"));
			Assert.That(models[0].Preferences, Is.Not.Null);

			Assert.That(models[0].TariffTypes, Is.Not.Null);
			Assert.That(models[0].TariffTypes.Count, Is.EqualTo(2));
			Assert.That(models[0].TariffTypes.Any(x => x == "IMP"));
			Assert.That(models[0].TariffTypes.Any(x => x == "EXP"));

			Assert.That(models[1].Description, Is.EqualTo("Measure 002"));
			Assert.That(models[1].Formula, Is.EqualTo("MIN(VFD * 1.2345 + 25.718 * [DTNN], VFD * 0.257 + 12.000 * [DTNN])"));
			Assert.That(models[1].CleanId, Is.EqualTo("21050010"));

			Assert.That(models[2].Description, Is.EqualTo("Measure 004"));
			Assert.That(models[2].Formula, Is.EqualTo("0 + 191.000 * [DTNE]"));

			Assert.That(models[2].Conditions, Is.Not.Null);
			Assert.That(models[2].Conditions.Count, Is.EqualTo(3));
			var conditions = models[2].Conditions.OrderBy(c => c.ConditionCode).ToList();
			Assert.That(conditions[0].ConditionCodeDescription, Is.EqualTo("UT Measure Condition Code B"));
			Assert.That(conditions[0].IsRateFormulaCondition, Is.EqualTo(false));
			Assert.That(conditions[1].IsRateFormulaCondition, Is.EqualTo(false));
			Assert.That(conditions[2].IsRateFormulaCondition, Is.EqualTo(true));
			Assert.That(conditions[0].IsCertificate, Is.EqualTo(true));
			Assert.That(conditions[1].IsCertificate, Is.EqualTo(true));
			Assert.That(conditions[2].IsCertificate, Is.EqualTo(false));

			Assert.That(models[3].Conditions.First().IsCertificate, Is.EqualTo(false));

			Assert.That(models.Take(4).Select(x => x.IsSupplementaryUnit), Does.Not.Contain(true), "IsSupplementaryUnit for first four models");

			Assert.That(models[4].Description, Is.EqualTo("Test"), "models[4].Description");
			Assert.That(models[4].Formula, Is.EqualTo(string.Empty), "models[4].Formula");
			Assert.That(models[4].IsSupplementaryUnit, Is.EqualTo(true), "models[4].IsSupplementaryUnit");
		}

		[Test]
		public void ModelUpdates_ProcessConfiguredMeasureTypesOnly()
		{
			var filePath = Path.Combine(ContentFolder, "UT_Measure_001.xml");
			TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Business.Tariff.TestFiles.Input.UT_Measure_001.xml");

			var file = new FileDetails { Content = new ContentDetails { ExecutionDate = new DateTime(2019, 10, 03, 14, 15, 16) }, Filename = filePath };
			var files = new List<FileDetails>() { file };

			var errorCollector = new StringBuilder();
			var mappingProvider = new MeasureMappingTestDataProvider();

			mappingProvider.MeasureTypeMappingsForTest = new Dictionary<string, MeasureTypeMapping>
			{
				{ "690", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Countervailing, RateCode = "A45" } }
			};
			var processor = new MeasureProcessorTester(mappingProvider, true);

			processor.LoadData("", files, errorCollector);
			Assert.That(processor.Models.Count, Is.EqualTo(7));

			processor.UpdateModels("", TestHelperClasses.TestData.CreateReferenceData(), errorCollector);

			Assert.That(processor.Models.Count, Is.EqualTo(1));
		}

		[Test]
		public void MeasureHelperType()
		{
			Assert.IsInstanceOf<MeasureHelper>(sharedProcessor.MeasureHelper);
		}

		[Test]
		public void LatestDescription()
		{
			var measure = new Measure
			{
				ItemId = "0102030405",
			};

			var errorCollector = new StringBuilder();
			var processor = new MeasureProcessorTester();

			processor.Models = new List<ITariffModel> { measure };
			Assert.That(processor.Models.Count, Is.EqualTo(1));

			var data = new List<ITariffModel>
			{
				new GoodsNomenclature { ItemId = "0102030405", ProductLineSuffix = "80", Level = 2, Description = "Measure - Expired", EndDate = new DateTime(2010, 01, 01), IsForMeasure = true },
				new GoodsNomenclature { ItemId = "0102030405", ProductLineSuffix = "80", Level = 1, Description = "Measure - Valid" },

				new BaseRegulation { RegulationId = "", RegulationRoleTypeId = "", StartDate = new DateTime(2020, 01, 01) },
			};

			processor.UpdateModels("", data, errorCollector);

			Assert.That(processor.Models.Count, Is.EqualTo(1));

			var models = processor.Models.Cast<Measure>().ToList();
			Assert.That(models[0].Description, Is.EqualTo("Measure - Valid"));
		}

		[Test]
		public void FlattenHierarchy()
		{
			var filePath = Path.Combine(ContentFolder, "UT_Measure_003.xml");
			TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Business.Tariff.TestFiles.Input.UT_Measure_003.xml");

			var file = new FileDetails { Content = new ContentDetails { ExecutionDate = new DateTime(2019, 10, 03, 14, 15, 16) }, Filename = filePath };
			var files = new List<FileDetails>() { file };

			var errorCollector = new StringBuilder();
			var processor = new MeasureProcessorTester();

			processor.LoadData("", files, errorCollector);
			Assert.That(processor.Models.Count, Is.EqualTo(6));

			var refData = new List<ITariffModel>
			{
				new BaseRegulation { RegulationId = "R000001", RegulationRoleTypeId = "1", StartDate = new DateTime(2020, 01, 01) },

				new GoodsNomenclature { ItemId = "0300000000", ProductLineSuffix = "80", Indent = 0, Level = 1, Description = "FISH AND CRUSTA...", Key = "01.03", Id = 1 },
				new GoodsNomenclature { ItemId = "0306000000", ProductLineSuffix = "80", Indent = 0, Level = 2, Description = "Crustaceans, wh...", Key = "01.03.06", Id = 2, ParentId = 1 },
				new GoodsNomenclature { ItemId = "0306110000", ProductLineSuffix = "10", Indent = 1, Level = 3, Description = "Frozen", Key = "01.03.06.11", Id = 3, ParentId = 2 },
				new GoodsNomenclature { ItemId = "0306110000", ProductLineSuffix = "80", Indent = 2, Level = 4, Description = "Rock lobster an...", Key = "01.03.06.11", Id = 4, ParentId = 3 },
				new GoodsNomenclature { ItemId = "0306111000", ProductLineSuffix = "80", Indent = 3, Level = 5, Description = "Crawfish tails", Key = "01.03.06.11.10", Id = 5, ParentId = 4 },
				new GoodsNomenclature { ItemId = "0306111090", ProductLineSuffix = "80", Indent = 4, Level = 6, Description = "Other 1", Key = "01.03.06.11.10.90", Id = 6, ParentId = 5, IsForMeasure = true },
				new GoodsNomenclature { ItemId = "0306111070", ProductLineSuffix = "80", Indent = 4, Level = 6, Description = "Other 2", Key = "01.03.06.11.10.90", Id = 7, ParentId = 5, IsForMeasure = true },
			};

			processor.UpdateModels("", refData, errorCollector);

			var models = processor.Models.Cast<Measure>().ToList();

			Assert.That(models, Is.Not.Null.And.Not.Empty);
			Assert.That(models.Count, Is.EqualTo(10));

			var modelsGroup = models.GroupBy(x => x.ItemId);

			Assert.That(modelsGroup.Count, Is.EqualTo(2));

			foreach (var grp in modelsGroup)
			{
				var list = grp.ToList();

				var importControl = list.FirstOrDefault(x => x.MeasureType == "750");
				Assert.That(importControl, Is.Not.Null);
				Assert.That(importControl.GeographicalArea, Is.EqualTo("1011"));
				Assert.That(importControl.Conditions.Count, Is.EqualTo(3));
				Assert.That(importControl.Conditions.ToList()[0].ConditionCode, Is.EqualTo("B"));

				var vat = list.FirstOrDefault(x => x.MeasureType == "305");
				Assert.That(vat, Is.Not.Null);
				Assert.That(vat.Components, Is.Not.Null.And.Not.Empty);
				Assert.That(vat.Components.ToList()[0].DutyAmount, Is.EqualTo(0));

				var duty = list.FirstOrDefault(x => x.MeasureType == "103");
				Assert.That(duty, Is.Not.Null);
				Assert.That(duty.Components, Is.Not.Null.And.Not.Empty);
				Assert.That(duty.Components.ToList()[0].DutyAmount, Is.EqualTo(12.5));

				var cvd = list.FirstOrDefault(x => x.MeasureType == "CVD");
				Assert.That(cvd, Is.Not.Null);
				Assert.That(cvd.Conditions, Is.Not.Null.And.Not.Empty);
				Assert.That(cvd.Conditions.ToList()[0].ConditionCode, Is.EqualTo("B"));
			}
		}

		[Test]
		public void IncludeNonCommodityCodesWhichAreChildren()
		{
			var filePath = Path.Combine(ContentFolder, "UT_Measure_003.xml");
			TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Business.Tariff.TestFiles.Input.UT_Measure_003.xml");

			var file = new FileDetails { Content = new ContentDetails { ExecutionDate = new DateTime(2019, 10, 03, 14, 15, 16) }, Filename = filePath };
			var files = new List<FileDetails>() { file };

			var errorCollector = new StringBuilder();
			var processor = new MeasureProcessorTester();

			processor.LoadData("", files, errorCollector);
			Assert.That(processor.Models.Count, Is.EqualTo(6));

			var refData = new List<ITariffModel>
			{
				new BaseRegulation { RegulationId = "R000001", RegulationRoleTypeId = "1", StartDate = new DateTime(2020, 01, 01) },

				new GoodsNomenclature { ItemId = "0300000000", ProductLineSuffix = "80", Indent = 0, Level = 1, Description = "FISH AND CRUSTA...", Key = "01.03", Id = 1 },
				new GoodsNomenclature { ItemId = "0306000000", ProductLineSuffix = "80", Indent = 0, Level = 2, Description = "Crustaceans, wh...", Key = "01.03.06", Id = 2, ParentId = 1, IsForMeasure = true }
			};

			processor.UpdateModels("", refData, errorCollector);

			var models = processor.Models.Cast<Measure>().ToList();

			Assert.That(models, Is.Not.Null.And.Not.Empty);
			Assert.That(models.Count, Is.EqualTo(2));
		}

		[Test]
		public void SplitOverlappingDates()
		{
			var filePath = Path.Combine(ContentFolder, "UT_Measure_004.xml");
			TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Business.Tariff.TestFiles.Input.UT_Measure_004.xml");

			var file = new FileDetails { Content = new ContentDetails { ExecutionDate = new DateTime(2019, 10, 03, 14, 15, 16) }, Filename = filePath };
			var files = new List<FileDetails>() { file };

			var errorCollector = new StringBuilder();
			var processor = new MeasureProcessorTester();

			processor.TestBuilders.First().TestDateTimeProvider.TestDateTime = new DateTime(2020, 06, 01);
			processor.TestBuilders.First().TestDateTimeProvider.TestHistoricalDateTime = new DateTime(2019, 06, 01);

			processor.LoadData("", files, errorCollector);
			Assert.That(processor.Models.Count, Is.EqualTo(5));

			var refData = new List<ITariffModel>
			{
				new BaseRegulation { RegulationId = "R000001", RegulationRoleTypeId = "1", StartDate = new DateTime(2020, 01, 01) },
				new GoodsNomenclature { ItemId = "0813509900", ProductLineSuffix = "80", Indent = 0, Level = 2, Description = "TestScenario", Key = "02.08.13", Id = 2, ParentId = 1, IsForMeasure = true }
			};

			processor.UpdateModels("", refData, errorCollector);

			var models = processor.Models.Cast<Measure>().ToList();

			Assert.That(models, Is.Not.Null.And.Not.Empty);
			Assert.That(models.Count, Is.EqualTo(5));
			Assert.That(models.Any(x => x.CalcStartDate == new DateTime(2020, 01, 01) && x.CalcEndDate == CommonHelper.DefaultValues.MaximumDateTime && x.MeasureType == "103"));
			Assert.That(models.Any(x => x.CalcStartDate == new DateTime(2015, 01, 01) && x.CalcEndDate == CommonHelper.DefaultValues.MaximumDateTime && x.MeasureType == "142"));
			Assert.That(models.Any(x => x.CalcStartDate == new DateTime(1998, 01, 01) && x.CalcEndDate == new DateTime(2014, 12, 31, 23, 59, 00) && x.MeasureType == "142"));
			Assert.That(models.Any(x => x.CalcStartDate == new DateTime(2017, 01, 01) && x.CalcEndDate == CommonHelper.DefaultValues.MaximumDateTime && x.MeasureType == "103" && x.OrderNumber == "123456"));
			Assert.That(models.Any(x => x.CalcStartDate == new DateTime(2018, 01, 01) && x.CalcEndDate == CommonHelper.DefaultValues.MaximumDateTime && x.MeasureType == "103" && x.AdditionalCode == "501"));
		}

		[Test]
		public void RemoveExpiredOnUpdate()
		{
			var errorCollector = new StringBuilder();
			var processor = new MeasureProcessorTester();

			processor.Models = new List<ITariffModel>
			{
				 new Measure { ItemId = "0102030405", MeasureType = "VALID" },
				 new Measure { ItemId = "0102030405", MeasureType = "EXPIRED", EndDate = processor.TestBuilders.First().TestDateTimeProvider.UTCHistoricalDate.AddDays(-1) }
			};
			Assert.That(processor.Models.Count, Is.EqualTo(2));

			var data = new List<ITariffModel>
			{
				new GoodsNomenclature { ItemId = "0102030405", ProductLineSuffix = "80", Level = 1, Description = "Measure - Valid", IsForMeasure = true },
				new BaseRegulation { RegulationId = "", RegulationRoleTypeId = "", StartDate = new DateTime(2020, 01, 01) },
			};

			processor.UpdateModels("", data, errorCollector);

			Assert.That(processor.Models.Count, Is.EqualTo(1));

			var models = processor.Models.Cast<Measure>().ToList();
			Assert.That(models[0].Description, Is.EqualTo("Measure - Valid"));
		}

		[Test]
		public void NomenclatureDatesForTariffs()
		{
			var filePath = Path.Combine(ContentFolder, "UT_Measure_006.xml");
			TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Business.Tariff.TestFiles.Input.UT_Measure_006.xml");

			var file = new FileDetails { Content = new ContentDetails { ExecutionDate = new DateTime(2019, 10, 03, 14, 15, 16) }, Filename = filePath };
			var files = new List<FileDetails>() { file };

			var errorCollector = new StringBuilder();
			var processor = new MeasureProcessorTester();
			var startDate = new DateTime(2020, 01, 01);
			var endDate = new DateTime(2025, 12, 31);

			processor.LoadData("", files, errorCollector);
			var models = processor.Models.Cast<Measure>().ToList();
			Assert.That(models.Count, Is.EqualTo(2));

			var model1 = models.FirstOrDefault(x => x.ItemId == "0100000000" && x.MeasureType == "750");
			Assert.That(model1, Is.Not.Null);
			var model2 = models.FirstOrDefault(x => x.ItemId == "0100000000" && x.MeasureType == "103");
			Assert.That(model2, Is.Not.Null);

			Assert.That(model1.StartDate, Is.EqualTo(new DateTime(2001, 01, 01)));
			Assert.That(model1.EndDate, Is.EqualTo(new DateTime(2050, 12, 31, 23, 59, 59)));

			Assert.That(model2.StartDate, Is.EqualTo(new DateTime(2001, 01, 01)));
			Assert.That(model2.EndDate, Is.EqualTo(new DateTime(2010, 12, 31, 23, 59, 59)));

			var refData = new List<ITariffModel>
			{
				new BaseRegulation { RegulationId = "R000001", RegulationRoleTypeId = "1", StartDate = new DateTime(2020, 01, 01) },

				new GoodsNomenclature { ItemId = "0100000000", ProductLineSuffix = "80", Indent = 0, Level = 1, Description = "Level1", Key = "01.01", Id = 1 },
				new GoodsNomenclature { ItemId = "0102000000", ProductLineSuffix = "80", Indent = 0, Level = 2, Description = "Level2", Key = "01.01.02", Id = 2, ParentId = 1 },
				new GoodsNomenclature { ItemId = "0102030000", ProductLineSuffix = "10", Indent = 1, Level = 3, Description = "Level3", Key = "01.01.02.10", Id = 3, ParentId = 2, IsForMeasure = true, StartDate = startDate, EndDate = endDate },
			};

			processor.UpdateModels("", refData, errorCollector);

			models = processor.Models.Cast<Measure>().ToList();
			Assert.That(models.Count, Is.EqualTo(1));
			Assert.That(models[0].ItemId, Is.EqualTo("0102030000"));
			Assert.That(models[0].NomenclatureStartDate, Is.EqualTo(startDate));
			Assert.That(models[0].NomenclatureEndDate, Is.EqualTo(new DateTime(2025, 12, 30, 23, 59, 0)));
		}

		[Test]
		public void FilteredMeasureTypes()
		{
			var errorCollector = new StringBuilder();
			var mappingProvider = new MeasureMappingTestDataProvider();

			mappingProvider.MeasureTypeMappingsForTest = new Dictionary<string, MeasureTypeMapping>
			{
				{ "103", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Preferences = new List<string> { "100" }, AuthorisedUsePreferences = new[] { "140" } } },
				{ "488", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Skip = true } },
				{ "490", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Skip = true } }
			};
			var processor = new MeasureProcessorTester(mappingProvider);

			processor.Models = new List<ITariffModel>
			{
				 new Measure { ItemId = "0101010101", MeasureType = "103", Description = "Will remain" },
				 new Measure { ItemId = "0202020202", MeasureType = "488", Description = "Will be dropped" },
				 new Measure { ItemId = "0303030303", MeasureType = "490", Description = "Will be dropped" }
			};
			Assert.That(processor.Models.Count, Is.EqualTo(3));

			var data = new List<ITariffModel>
			{
				new GoodsNomenclature { ItemId = "0101010101", ProductLineSuffix = "80", Level = 1, Description = "Measure1", IsForMeasure = true },
				new GoodsNomenclature { ItemId = "0202020202", ProductLineSuffix = "80", Level = 1, Description = "Measure2", IsForMeasure = true },
				new GoodsNomenclature { ItemId = "0303030303", ProductLineSuffix = "80", Level = 1, Description = "Measure3", IsForMeasure = true },
				new MeasureType { Id = "103", Description = "Third Country", TradeMovementCode = "0", MeasureTypeSeries = "C" },
				new MeasureType { Id = "488", Description = "Unit Price", TradeMovementCode = "0", MeasureTypeSeries = "M" },
				new MeasureType { Id = "490", Description = "Standard Import Value", TradeMovementCode = "0", MeasureTypeSeries = "M" },

				new BaseRegulation { RegulationId = "", RegulationRoleTypeId = "", StartDate = new DateTime(2020, 01, 01) },
			};

			processor.UpdateModels("", data, errorCollector);

			Assert.That(processor.Models.Count, Is.EqualTo(1));

			var models = processor.Models.Cast<Measure>().ToList();
			Assert.That(models.Any(x => x.ItemId == "0101010101"));
		}

		[Test]
		public void InheritanceFields()
		{
			var filePath = Path.Combine(ContentFolder, "UT_Measure_005.xml");
			TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Business.Tariff.TestFiles.Input.UT_Measure_005.xml");

			var file = new FileDetails { Content = new ContentDetails { ExecutionDate = new DateTime(2019, 10, 03, 14, 15, 16) }, Filename = filePath };
			var files = new List<FileDetails>() { file };

			var errorCollector = new StringBuilder();
			var processor = new MeasureProcessorTester();

			processor.TestBuilders.First().TestDateTimeProvider.TestDateTime = new DateTime(2020, 06, 01);
			processor.TestBuilders.First().TestDateTimeProvider.TestHistoricalDateTime = new DateTime(2019, 06, 01);

			processor.LoadData("", files, errorCollector);
			Assert.That(processor.Models.Count, Is.EqualTo(7));

			var data = new List<ITariffModel>
			{
				new BaseRegulation { RegulationId = "R000001", RegulationRoleTypeId = "1", StartDate = new DateTime(2020, 01, 01) },

				new GoodsNomenclature { ItemId = "0101010000", ProductLineSuffix = "80", Indent = 0, Level = 1, Description = "Multi-inherit", Key = "01.01", Id = 1 },
				new GoodsNomenclature { ItemId = "0101010100", ProductLineSuffix = "80", Indent = 0, Level = 2, Description = "Child", Key = "01.01.01", Id = 2, ParentId = 1, IsForMeasure = true },
			};

			processor.UpdateModels("", data, errorCollector);

			Assert.That(processor.Models.Count, Is.EqualTo(6));
		}

		[Test]
		public void DuplicateInheritanceAvoidance()
		{
			var filePath = Path.Combine(ContentFolder, "UT_Measure_007.xml");
			TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Business.Tariff.TestFiles.Input.UT_Measure_007.xml");

			var file = new FileDetails { Content = new ContentDetails { ExecutionDate = new DateTime(2019, 10, 03, 14, 15, 16) }, Filename = filePath };
			var files = new List<FileDetails>() { file };

			var errorCollector = new StringBuilder();
			var processor = new MeasureProcessorTester();

			processor.TestBuilders.First().TestDateTimeProvider.TestDateTime = new DateTime(2020, 10, 30);
			processor.TestBuilders.First().TestDateTimeProvider.TestHistoricalDateTime = new DateTime(2019, 10, 30);

			processor.LoadData("", files, errorCollector);
			Assert.That(processor.Models.Count, Is.EqualTo(3));

			var data = new List<ITariffModel>
			{
				new BaseRegulation { RegulationId = "R000001", RegulationRoleTypeId = "1", StartDate = new DateTime(2010, 01, 01) },

				new GoodsNomenclature { ItemId = "0710400000", ProductLineSuffix = "80", Indent = 0, Level = 1, Description = "Multi-inherit", Key = "01.01", Id = 1, StartDate = new DateTime(2016, 01, 01), EndDate = new DateTime(2020, 07, 31, 23, 59, 59) },
				new GoodsNomenclature { ItemId = "0710400091", ProductLineSuffix = "80", Indent = 0, Level = 2, Description = "Child", Key = "01.01.01", Id = 2, ParentId = 1, IsForMeasure = true, StartDate = new DateTime(2020, 08, 01) },
			};

			processor.UpdateModels("", data, errorCollector);
			var models = processor.Models.Cast<Measure>().ToList();

			Assert.That(models.Count, Is.EqualTo(1));
			Assert.That(models[0].HJID, Is.EqualTo("9983587"));
			Assert.That(models[0].CalcStartDate, Is.EqualTo(new DateTime(2020, 01, 01)));
			Assert.That(models[0].CalcEndDate, Is.EqualTo(CommonHelper.DefaultValues.MaximumDateTime));
		}

		[Test]
		public void InheritFromCorrectParent()
		{
			var filePath = Path.Combine(ContentFolder, "UT_Measure_009.xml");
			TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Business.Tariff.TestFiles.Input.UT_Measure_009.xml");

			var file = new FileDetails { Content = new ContentDetails { ExecutionDate = new DateTime(2019, 10, 03, 14, 15, 16) }, Filename = filePath };
			var files = new List<FileDetails>() { file };

			var errorCollector = new StringBuilder();
			var processor = new MeasureProcessorTester();

			processor.TestBuilders.First().TestDateTimeProvider.TestDateTime = new DateTime(2020, 10, 30);
			processor.TestBuilders.First().TestDateTimeProvider.TestHistoricalDateTime = new DateTime(2019, 10, 30);

			processor.LoadData("", files, errorCollector);
			Assert.That(processor.Models.Count, Is.EqualTo(2), "Pre-requisite: loaded data Models.Count");

			var data = new List<ITariffModel>
			{
				new BaseRegulation { RegulationId = "X2015270", RegulationRoleTypeId = "1", StartDate = new DateTime(2010, 01, 01) },
				new BaseRegulation { RegulationId = "X2100540", RegulationRoleTypeId = "1", StartDate = new DateTime(2010, 01, 01) },

				new GoodsNomenclature { ItemId = "0602904100", ProductLineSuffix = "10", Description = "Other", Key = "02.06..02.9.40", Id = 1, StartDate = new DateTime(2016, 01, 01), EndDate = new DateTime(2020, 07, 31, 23, 59, 59) },
				new GoodsNomenclature { ItemId = "0602904100", ProductLineSuffix = "20", Description = "Outdoor plants", Key = "02.06..02.9.40.10", Id = 2, ParentId = 1, StartDate = new DateTime(2020, 08, 01) },
				new GoodsNomenclature { ItemId = "0602904100", ProductLineSuffix = "30", Description = "Trees, shrubs and bushes", Key = "02.06..02.9.40.10.10", Id = 3, ParentId = 2, StartDate = new DateTime(2020, 08, 01) },
				new GoodsNomenclature { ItemId = "0602904100", ProductLineSuffix = "80", Description = "Forest trees", Key = "02.06..02.9.40.10.10.10", Id = 4, ParentId = 3, IsForMeasure = true, StartDate = new DateTime(2020, 08, 01) },
				new GoodsNomenclature { ItemId = "0602905000", ProductLineSuffix = "80", Description = "Other outdoor plants", Key = "02.06..02.9.40.10.20", Id = 5, ParentId = 2, IsForMeasure = true, StartDate = new DateTime(2020, 08, 01) },
			};

			processor.UpdateModels("", data, errorCollector);
			var models = processor.Models.Cast<Measure>().ToList();

			Assert.That(models.Select(x => (x.ItemId, x.MeasureType)), Is.EquivalentTo(new[] {("0602904100", "710"), ("0602905000", "360") }));
		}

		[Test]
		public void CopyMeasure()
		{
			var source = new Measure
			{
				OpType = MetaInfoOpTypes.Created,
				OpDate = DateTime.Now,
				AdditionalCode = "AC",
				AdditionalCodeType = "ACT",
				CleanId = "CId",
				CompositeKey = "CKey",
				ConditionClass = "CClass",
				Description = "Descrip",
				EndDate = new DateTime(2020, 12, 31),
				ExportDescription = "ExDescrip",
				ExportItemId = "ExItem",
				Formula = "Formula",
				GeographicalArea = "GeoArea",
				HJID = "hjid",
				ItemId = "ItemId",
				Suffix = "Suffix",
				MeasureType = "MT",
				MeasureTypeDescription = "MTD",
				MeasureTypeSeries = "MTS",
				OrderNumber = "OrderNum",
				Preferences = new List<string> { "P1", "P2" },
				RateCode = "RateCode",
				RateType = "RateType",
				RegulationId = "RegId",
				RegulationRoleTypeId = "RegRoleType",
				StartDate = new DateTime(1900, 05, 06, 13, 14, 16),
				VatCode = "12313",
				NomenclatureStartDate = new DateTime(2022, 1, 1),
				NomenclatureEndDate = new DateTime(2022, 12, 31, 23, 59, 0),
				IsSupplementaryUnit = true,
				TariffTypes = new List<string> { "SDF", "ASD" },
			}
			.SetComponents(new[]
			{
				new MeasureComponent
				{
					OpType = MetaInfoOpTypes.Created,
					OpDate = DateTime.Now,
					DutyAmount = 12.3m,
					DutyExpression = "12",
					HJID = "345435",
					MeasurementUnit = "MesUn1",
					MeasurementUnitQualifier = "MUQ1",
					MonetaryUnit = "MONU1"
				},
				new MeasureComponent
				{
					OpType = MetaInfoOpTypes.Created,
					OpDate = DateTime.Now,
					DutyAmount = 52.67m,
					DutyExpression = "23",
					HJID = "4535",
					MeasurementUnit = "MesUn2",
					MeasurementUnitQualifier = "MUQ2",
					MonetaryUnit = "MONU2"
				},
			})
			.SetConditions(new[]
			{
				new MeasureCondition
				{
					OpType = MetaInfoOpTypes.Created,
					OpDate = DateTime.Now,
					CertificateCode = "CC",
					CertificateTypeCode = "CTC",
					ConditionCode = "Con123",
					ConditionCodeDescription = "ConCodeDscrip",
					DutyAmount = 45.33m,
					Formula = "conForm1",
					HJID = "3454",
					IsRateFormulaCondition = true,
					MeasureAction = "Act",
					MeasurementUnit = "MesUn3",
					MeasurementUnitQualifier = "MUQ3",
					MonetaryUnit = "MONU3",
					SequenceNumber = 45,
				}
				.SetComponents(new[]
				{
					new MeasureComponent
					{
						OpType = MetaInfoOpTypes.Created,
						OpDate = DateTime.Now,
						DutyAmount = 5452.67m,
						DutyExpression = "74",
						HJID = "4565r6",
						MeasurementUnit = "MesUn4",
						MeasurementUnitQualifier = "MUQ4",
						MonetaryUnit = "MONU4"
					}
				})
			})
			.SetExcludedGeographicalAreas(new[] { new ExcludedGeographicalArea { Value = "ABC", HJID = "1", OpType = MetaInfoOpTypes.Created, OpDate = DateTime.Now }, new ExcludedGeographicalArea { Value = "DEF", HJID = "2", OpType = MetaInfoOpTypes.Created, OpDate = DateTime.Now } })
			.SetFootnotes(new[] { new Footnote { Value = "AB123", HJID = "3", OpType = MetaInfoOpTypes.Created, OpDate = DateTime.Now }, new Footnote { Value = "CD456", HJID = "4", OpType = MetaInfoOpTypes.Created, OpDate = DateTime.Now } });

			var dest = source.Copy();

			Assert.That(TestHelperClasses.ObjectComparison.AreObjectsEqual(source, dest));
		}

		[Test]
		public void SetPreferences()
		{
			var errorCollector = new StringBuilder();
			var mappingProvider = new MeasureMappingTestDataProvider();

			mappingProvider.MeasureTypeMappingsForTest = new Dictionary<string, MeasureTypeMapping>
			{
				{ "143", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Preferences = new List<string> { "220", "225", "320", "325" }, AuthorisedUsePreferences = new[] { "223", "323" } } }
			};
			var processor = new MeasureProcessorTester(mappingProvider);

			var measure = new Measure { ItemId = "0101010101", MeasureType = "143", Description = "Unit Test", GeographicalArea = "1011" }
			.SetFootnotes(new[] { new Footnote { Value = "CD376", HJID = "1" } });

			var data = new List<ITariffModel>
			{
				new GoodsNomenclature { ItemId = "0101010101", ProductLineSuffix = "80", Level = 1, Description = "Measure1", IsForMeasure = true },
				new MeasureType { Id = "143", Description = "Something else", TradeMovementCode = "0", MeasureTypeSeries = "C" },

				new BaseRegulation { RegulationId = "", RegulationRoleTypeId = "", StartDate = new DateTime(2020, 01, 01) },
			};

			processor.Models = new List<ITariffModel> { measure };
			processor.UpdateModels("", data, errorCollector);
			var models = processor.Models.Cast<Measure>().ToList();
			Assert.That(models.Count, Is.EqualTo(1));

			var prefs = models[0].Preferences;
			Assert.That(prefs, Is.Not.Null);
			Assert.That(prefs.Count, Is.EqualTo(4));
			Assert.That(prefs.Contains("220"));
			Assert.That(prefs.Contains("225"));
			Assert.That(prefs.Contains("320"));
			Assert.That(prefs.Contains("325"));

			mappingProvider.PreferencesForTest = new List<string> { "ABC", "DEF" };
			processor.UpdateModels("", data, errorCollector);
			prefs = models[0].Preferences;
			Assert.That(prefs.Count, Is.EqualTo(6));
			Assert.That(prefs.Contains("ABC"));
			Assert.That(prefs.Contains("DEF"));
			Assert.That(prefs.Contains("220"));
			Assert.That(prefs.Contains("225"));
			Assert.That(prefs.Contains("320"));
			Assert.That(prefs.Contains("325"));
		}

		[Test]
		public void SetVatCode()
		{
			var errorCollector = new StringBuilder();
			var mappingProvider = new MeasureMappingTestDataProvider();
			var processor = new MeasureProcessorTester(mappingProvider);

			mappingProvider.VatCodeForTest = "VC4TEST";

			var measure = new Measure {	ItemId = "0101010101", MeasureType = "305",	Description = "Unit Test", GeographicalArea = "1011" };

			var data = new List<ITariffModel>
			{
				new GoodsNomenclature { ItemId = "0101010101", ProductLineSuffix = "80", Level = 1, Description = "Measure1", IsForMeasure = true },
				new MeasureType { Id = "305", Description = "Vat", TradeMovementCode = "0", MeasureTypeSeries = "C" },

				new BaseRegulation { RegulationId = "", RegulationRoleTypeId = "", StartDate = new DateTime(2020, 01, 01) },
			};

			processor.Models = new List<ITariffModel> { measure };
			processor.UpdateModels("", data, errorCollector);
			var models = processor.Models.Cast<Measure>().ToList();
			Assert.That(models.Count, Is.EqualTo(1));

			Assert.That(models[0].VatCode, Is.EqualTo("VC4TEST"));
		}

		[Test]
		public void SetRateCode()
		{
			var errorCollector = new StringBuilder();
			var mappingProvider = new MeasureMappingTestDataProvider();
			var processor = new MeasureProcessorTester(mappingProvider);

			mappingProvider.RateCodeForTest = "RC4TEST";
			mappingProvider.MeasureTypeMappingsForTest = new Dictionary<string, MeasureTypeMapping>
			{
				{ "305", new MeasureTypeMapping { RateCode = "RC01" } }
			};

			var measure = new Measure { ItemId = "0101010101", MeasureType = "305", Description = "Unit Test", GeographicalArea = "1011" };

			var data = new List<ITariffModel>
			{
				new GoodsNomenclature { ItemId = "0101010101", ProductLineSuffix = "80", Level = 1, Description = "Measure1", IsForMeasure = true },
				new MeasureType { Id = "305", Description = "Vat", TradeMovementCode = "0", MeasureTypeSeries = "C" },

				new BaseRegulation { RegulationId = "", RegulationRoleTypeId = "", StartDate = new DateTime(2020, 01, 01) },
			};

			processor.Models = new List<ITariffModel> { measure };
			processor.UpdateModels("", data, errorCollector);
			var models = processor.Models.Cast<Measure>().ToList();
			Assert.That(models.Count, Is.EqualTo(1));

			Assert.That(models[0].RateCode, Is.EqualTo("RC4TEST-RC01"));
		}

		[Test]
		public void IsChapterSpecific()
		{
			Assert.That(sharedProcessor.IsChapterSpecific, Is.EqualTo(true));
		}

		[Test]
		public void ConvertSupplementaryUnitTagToActualSupplementaryUnit()
		{
			var filePath = Path.Combine(ContentFolder, "UT_Measure_008.xml");
			TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Business.Tariff.TestFiles.Input.UT_Measure_008.xml");

			var file = new FileDetails { Content = new ContentDetails { ExecutionDate = new DateTime(2019, 10, 03, 14, 15, 16) }, Filename = filePath };
			var files = new List<FileDetails>() { file };

			var errorCollector = new StringBuilder();
			var mappingProvider = new MeasureMappingTestDataProvider();

			mappingProvider.MeasureTypeMappingsForTest = new Dictionary<string, MeasureTypeMapping>
			{
				{ "109", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty, SupplementaryUnit = true } },
				{ "482", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Class, RateType = string.Empty } }
			};
			var processor = new MeasureProcessorTester(mappingProvider);

			processor.TestBuilders.First().TestDateTimeProvider.TestDateTime = new DateTime(2020, 06, 01);
			processor.TestBuilders.First().TestDateTimeProvider.TestHistoricalDateTime = new DateTime(2019, 06, 01);

			processor.LoadData("", files, errorCollector);
			Assert.That(processor.Models.Count, Is.EqualTo(3));

			processor.UpdateModels("", TestData.CreateReferenceData(), errorCollector);
			var models = processor.Models.Cast<Measure>().ToList();

			Assert.That(models.Count, Is.EqualTo(3));

			var withFormula = models.Where(x => !string.IsNullOrEmpty(x.Formula)).ToList();
			Assert.That(withFormula.Count, Is.EqualTo(2));

			var withSup = withFormula.FirstOrDefault(x => x.ItemId == "0811109010");
			Assert.That(withSup, Is.Not.Null);
			Assert.That(withSup.Formula, Does.Contain("[LTR]").And.Not.Contain("[SUPU]"));

			var withoutSup = withFormula.FirstOrDefault(x => x.ItemId == "2105001000");
			Assert.That(withoutSup, Is.Not.Null);
			Assert.That(withoutSup.Formula, Does.Contain("[NAR]").And.Not.Contain("[SUPU]"));
		}

		[Test]
		public void IsAdditionalData()
		{
			var measure = new Measure() { Formula = "" };

			Assert.That(measure.IsAdditionalInfo, Is.EqualTo(true), "No formula or conditions");
			measure.Formula = "No longer Additional";
			Assert.That(measure.IsAdditionalInfo, Is.EqualTo(false), "Has a formula so not additional");
			measure.Formula = "";
			measure.SetConditions(new[] { new MeasureCondition { HJID = "1" } });
			Assert.That(measure.IsAdditionalInfo, Is.EqualTo(false), "Has a condition so not additional");
		}

		[Test]
		public void AddAuthorisedUsePreferences()
		{
			var mappingProvider = new MeasureMappingTestDataProvider();
			
			mappingProvider.MeasureTypeMappingsForTest = new Dictionary<string, MeasureTypeMapping>
			{
				{ "103", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Preferences = new List<string> { "100" }, AuthorisedUsePreferences = new[] { "140" } } },
				{ "112", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Preferences = new List<string> { "110" }, AuthorisedUsePreferences = new[] { "115" } } },
				{ "122", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Preferences = new List<string> { "120", "125", "128" }, AuthorisedUsePreferences = new[] { "123" } } },
				{ "142", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Preferences = new List<string> { "200", "300" }, AuthorisedUsePreferences = new[] { "240", "340" } } },
				{ "464", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }
			};
			var processor = new MeasureProcessorTester(mappingProvider);

			var data = new List<ITariffModel>
			{
				new GoodsNomenclature { ItemId = "0101010101", ProductLineSuffix = "80", Level = 1, Description = "Measure1", IsForMeasure = true },
				new GoodsNomenclature { ItemId = "0202020202", ProductLineSuffix = "80", Level = 1, Description = "Measure2", IsForMeasure = true },
				new GoodsNomenclature { ItemId = "0303030303", ProductLineSuffix = "80", Level = 1, Description = "Measure3", IsForMeasure = true },
				new GoodsNomenclature { ItemId = "0404040404", ProductLineSuffix = "80", Level = 1, Description = "Measure4", IsForMeasure = true },
				new MeasureType { Id = "103", Description = "Third Country", TradeMovementCode = "0", MeasureTypeSeries = "C" },
				new MeasureType { Id = "142", Description = "Tariff Preference", TradeMovementCode = "0", MeasureTypeSeries = "C" },
				new MeasureType { Id = "464", Description = "Declaration of subheading submitted to end-use provisions", TradeMovementCode = "0", MeasureTypeSeries = "B" },
				new BaseRegulation { RegulationId = "", RegulationRoleTypeId = "", StartDate = new DateTime(2020, 01, 01) },
			};

			var errorCollector = new StringBuilder();


			var measure103 = new Measure { ItemId = "0101010101", MeasureType = "103", Description = "Unit Test", GeographicalArea = "1011" };
			var measure464a = new Measure { ItemId = "0101010101", MeasureType = "464", Description = "Unit Test", GeographicalArea = "1011" };
			var measure142 = new Measure { ItemId = "0202020202", MeasureType = "142", Description = "Unit Test", GeographicalArea = "1011" };
			var measure464b = new Measure { ItemId = "0202020202", MeasureType = "464", Description = "Unit Test", GeographicalArea = "1011" };
			var measure112 = new Measure { ItemId = "0303030303", MeasureType = "112", Description = "Unit Test", GeographicalArea = "1011" };
			var measure464c = new Measure { ItemId = "0303030303", MeasureType = "464", Description = "Unit Test", GeographicalArea = "2020" };
			var measure122 = new Measure { ItemId = "0404040404", MeasureType = "122", Description = "Unit Test", GeographicalArea = "1011" };

			processor.Models = new List<ITariffModel> { measure103, measure464a, measure142, measure464b, measure112, measure464c, measure122 };
			var models = processor.Models.Cast<Measure>().ToList();
			Assert.That(models.Count, Is.EqualTo(7));

			processor.UpdateModels("", data, errorCollector);
			models = processor.Models.Cast<Measure>().ToList();

			Assert.That(models.Count, Is.EqualTo(7));

			Assert.That(models.First(x => x.MeasureType == "103").Preferences.Contains("140"), "Pref 140 should be added to MT 103");
			Assert.That(models.First(x => x.MeasureType == "142").Preferences.Contains("240"), "Pref 240 should be added to MT 142");
			Assert.That(models.First(x => x.MeasureType == "142").Preferences.Contains("340"), "Pref 340 should be added to MT 142");
			Assert.That(!models.First(x => x.MeasureType == "112").Preferences.Contains("115"), "Pref 115 should not be added to MT 112");
			Assert.That(!models.First(x => x.MeasureType == "122").Preferences.Contains("123"), "Pref 123 should not be added to MT 122");
		}

		#region Setup
		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);

			sharedProcessor = new MeasureProcessorTester();
		}

		[SetUp]
		public void Setup()
		{
			OutputFolder = Path.Combine(TempFolder, Path.GetRandomFileName());
			Directory.CreateDirectory(OutputFolder);
			ContentFolder = Path.Combine(TempFolder, Path.GetRandomFileName());
			Directory.CreateDirectory(ContentFolder);
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		string OutputFolder;
		string ContentFolder;
		string TempFolder;
		MeasureProcessorTester sharedProcessor;
		#endregion
	}
}
