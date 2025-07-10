using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CargoWise.RefDbRepo.GBReferenceData.Tests;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using NUnit.Framework;
using static CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Helpers.Tests.TestHelperClasses;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Helpers.Tests.TestHelperClasses;

namespace CargoWise.RefDbRepo.GBReferenceData.Business.Tariff.Tests
{
	[TestFixture]
	class GBMeasureHelperTests
	{
		[TestCase("00", "Beer (2023 broken rules) 2203000100", "If([ASV] <= 3.490, 0 * [ASV] + 0 * [ASVX], If([ASV] <= 8.490, 21.010 * [ASVX] + 0 * [ASV], 0 * [ASV] + 0 * [ASVX]))")]
		[TestCase("01", "Beer (fixed 2023 rules) 2203000100", "If([ASV] <= 3.490, 0 * [ASV] + 0 * [ASVX], If([ASV] <= 8.490, 21.010 * [ASVX] + 0 * [ASV], 0 * [ASV] + 0 * [ASVX]))")]
		[TestCase("02", "Small ASV without ASVX", "If([ASV] <= 1.200, 9.270 * [LPA] + 0 * [ASV], 0 * [ASV] + 0 * [LPA])")]
		public void RateFormula(string skipStr, string description, string expected)
		{
			var errorCollector = new StringBuilder();
			var mappingProvider = new MeasureMappingTestDataProvider();

			var processor = new MeasureProcessorTester(mappingProvider);

			int skip = int.Parse(skipStr, CultureInfo.CurrentCulture);
			var xElement = TestHelper.GetXmlElement("Measure", "CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.TestFiles.Input.UT_RateFormula.xml", skip);
			var measure = loader.ConvertXElementToModel(xElement);

			processor.Models = new List<ITariffModel>() { measure };
			processor.UpdateModels(string.Empty, TestData.CreateReferenceData(), errorCollector);

			var actual = measureHelper.GenerateFormula(measure, errorCollector);

			Assert.That(actual, Is.EqualTo(expected), description);
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			measureHelper = new GBMeasureHelper();
			loader = new MeasureLoaderTester();
		}

		GBMeasureHelper measureHelper;
		MeasureLoaderTester loader;
	}
}
