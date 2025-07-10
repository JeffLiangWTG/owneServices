using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Common.Tests;
using NUnit.Framework;
using static CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Helpers.Tests.TestHelperClasses;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using System.Collections.Generic;
using System;
using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.GBReferenceData.Business.Tariff.Tests
{
	[TestFixture]
	class GBMeasureProcessorTests
	{
		[Test]
		public void MeasureHelperType()
		{
			var processor = new GBMeasureProcessor(new CommonHelpers.DateTimeProvider(), new MeasureMappingTestDataProvider(), new IRefXmlBuilder[] { new VirtualBuilder() });
			Assert.IsInstanceOf<GBMeasureHelper>(processor.MeasureHelper);
		}

		[TestCase("484")] // measure type mapped in MeasureMappingProvider
		[TestCase("XXX")] // measure type not mapped in MeasureMappingProvider
		public void MappedAndUnmappedMeasureTypesAreSupported(string measureType)
		{
			var errorCollector = new StringBuilder();
			var mappingProvider = new MeasureMappingProvider() as IMeasureMappingProvider;
			var measureTypeHelper = new MeasureTypeHelper(mappingProvider);
			var processor = new GBMeasureProcessorForTest(new CommonHelpers.DateTimeProvider(), mappingProvider, new IRefXmlBuilder[] { new VirtualBuilder() });

			Assert.That(measureTypeHelper.ShouldProcessMeasureType(measureType, false), Is.True);

			var startDate = new DateTime(2024, 10, 01, 15, 14, 13);

			var measure = new Measure
			{
				ItemId = "1234560000",
				CleanId = "123456",
				StartDate = startDate,
				NomenclatureStartDate = startDate,
				Description = "Measure Type mapping test",
				CompositeKey = "03.12.34",
				MeasureType = measureType,
				MeasureTypeSeries = "R",
				MeasureTypeDescription = "Initial measure type description",
				RegulationId = "BR1",
				RegulationRoleTypeId = "1",
				Preferences = new List<string> { "100" },
				ConditionClass = MeasureHelper.ConditionClass.Class,
				TariffTypes = new List<string> { "IMP" },
			};

			var models = new List<ITariffModel>()
			{
				new GoodsNomenclature { ItemId = "GN1" },
				new BaseRegulation { RegulationId = "BR1", RegulationRoleTypeId = "1" },
				new ModificationRegulation { RegulationId = "MR1", RegulationRoleTypeId = "1" },
				new MeasureType { Id = measureType, Description = "Updated measure type description", TradeMovementCode = "1", MeasureTypeSeries = "R", StartDate = startDate },
			};

			processor.Models = new List<ITariffModel>()	{ measure };
			processor.UpdateModels(string.Empty, models, errorCollector);

			Assert.That(errorCollector.ToString(), Is.Empty);
			Assert.That(measure.ConditionClass, Is.EqualTo(measureTypeHelper.GetConditionClass(measureType)));
			Assert.That(measure.MeasureTypeDescription, Is.EqualTo("Updated measure type description"));
		}

		class GBMeasureProcessorForTest : GBMeasureProcessor
		{
			public GBMeasureProcessorForTest(IDateTimeProvider dateTimeProvider, IMeasureMappingProvider measureMappingProvider, IRefXmlBuilder[] builders) : base(dateTimeProvider, measureMappingProvider, builders)
			{
			}

			public new List<ITariffModel> Models { get { return base.Models; } set { base.Models = value; } }
		}
	}
}
