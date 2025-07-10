using System.Linq;
using CargoWise.RefDbRepo.IEReferenceData.Services;
using CargoWise.RefDbRepo.IEReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.IEReferenceData.Tariffs.Tests
{
	[TestFixture]
	class ExciseDutyRatePageReaderMineralOilTest
	{
		[Test]
		public void TestGetFromPageRow()
		{
			var reader = new ExciseDutyRatePageReaderMineralOil(TestConfig);

			var htmlTable = HtmlTableLoader.Read<object>(TestConfig.ExciseDuty_Url_Mineral_Oil).GetAwaiter().GetResult();
			var page = reader.GetFromPageRow(htmlTable.ValueTables.First());
			Assert.AreEqual(30, page.Length);
			Assert.AreEqual("Light Oil - Petrol", page[0].SearchText);
			Assert.AreEqual("€476.80", page[0].TaxRate);
		}

		[Test]
		public void TestGetFormula()
		{
			var reader = new ExciseDutyRatePageReaderMineralOil(TestConfig);
			var htmlTable = HtmlTableLoader.Read<object>(TestConfig.ExciseDuty_Url_Mineral_Oil).GetAwaiter().GetResult();
			var page = reader.GetFromPageRow(htmlTable.ValueTables.First());
			var formula = reader.GetFormula(page.First());
			Assert.AreEqual((true, 476.8m, "476.80 * [HLT]"), formula);
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
