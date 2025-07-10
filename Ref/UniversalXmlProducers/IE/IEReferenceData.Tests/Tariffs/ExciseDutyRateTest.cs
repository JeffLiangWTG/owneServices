using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.IEReferenceData.Services;
using CargoWise.RefDbRepo.IEReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.IEReferenceData.Tariffs.Tests
{
	[TestFixture]
	class ExciseDutyRateTest
	{
		[Test]
		public void TestExtractAndTryMapping()
		{
			var logger = new Logger();
			var result = ExciseDutyRate.ExtractAndTryMapping(TestConfig, logger);

			Assert.AreEqual(57, result.Tariffs.Count);
			Assert.AreEqual(3, result.UpdateDates.Count());
		}

		[Test]
		public void TestExtractAndTryMapping_UnRecognizableItem()
		{
			var directory = TestHelper.GetRunningDirectory();
			var configMock = TestHelper.GetBaseMock().SetupTariffTestInputPaths();
			configMock.Setup(x => x.ExciseDuty_Url_Alcohol_Products).Returns(Path.Combine(directory, TestHelper.ExciseDutyTariffTestInputPath, "Alcohol Products Tax - With Unrecognizable Item.html"));
			var config = configMock.Object;

			var logger = new Logger();
			var result = ExciseDutyRate.ExtractAndTryMapping(config, logger);

			Assert.That(logger.HasErrors);

			var errorMessage = $@"[Error]: Unable to find mapping for the following page items:
  URL: {directory}\Tariffs\TestFiles\Input\Alcohol Products Tax - With Unrecognizable Item.html, text: New Alcohol Product 1
  URL: {directory}\Tariffs\TestFiles\Input\Alcohol Products Tax - With Unrecognizable Item.html, text: Intermediate beverages - New Alcohol Product 2
";

			Assert.AreEqual(errorMessage, logger.GetErrors());
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
