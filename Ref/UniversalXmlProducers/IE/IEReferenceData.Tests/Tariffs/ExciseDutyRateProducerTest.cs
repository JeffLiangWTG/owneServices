using System.IO;
using CargoWise.RefDbRepo.IEReferenceData.Services;
using CargoWise.RefDbRepo.IEReferenceData.Tariffs.Business;
using CargoWise.RefDbRepo.IEReferenceData.Tests;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Logger = CargoWise.RefDbRepo.IEReferenceData.Services.Logger;

namespace CargoWise.RefDbRepo.IEReferenceData.Tariffs.Tests
{
	[TestFixture]
	class ExciseDutyRateProducerTest
	{
		[Test]
		public void TestExciseTaxes()
		{
			var logger = new Logger();
			ExciseDutyRateProducer.ExtractAndWriteToXml(TestConfig, logger);
			Assert.That(!logger.ToString().Contains(LogLevel.Error.ToString()));

			string expectedXml = TestHelper.ReadManifestResourceContent("RefCusTariff_IE_Excise_Taxes.xml");

			var outputPath = Path.Combine(TestConfig.OutputDirectory, TestConfig.ExciseDuty_OutputFileName);
			var actuallXml = File.ReadAllText(outputPath);

			Assert.That(actuallXml, Is.EqualTo(expectedXml));
		}

		[SetUp]
		public void SetUp()
		{
			TestHelper.ClearRuntimeData();
		}

		[TearDown]
		public void TearDown()
		{
			TestHelper.ClearRuntimeData();
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TestConfig = TestHelper.GetBaseMock().SetupTariffTestInputPaths().Object;
		}
		IApplicationConfig TestConfig;
	}
}
