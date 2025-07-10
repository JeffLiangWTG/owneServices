using System.Linq;
using CargoWise.RefDbRepo.IEReferenceData.Services;
using CargoWise.RefDbRepo.IEReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.IEReferenceData.Tariffs.Tests
{
	[TestFixture]
	class ExciseDutyRatePageReaderTobaccoProductsTest
	{

		[Test]
		public void TestGetFromPageRow()
		{
			var reader = new ExciseDutyRatePageReaderTobaccoProducts(TestConfig);

			var htmlTable = HtmlTableLoader.Read<object>(TestConfig.ExciseDuty_Url_Tobacco_Products).GetAwaiter().GetResult();
			var page = reader.GetFromPageRow(htmlTable.ValueTables.First());
			Assert.AreEqual(5, page.Length);
		}

		[Test]
		public void TestGetFormula()
		{
			var reader = new ExciseDutyRatePageReaderTobaccoProducts(TestConfig);
			var htmlTable = HtmlTableLoader.Read<object>(TestConfig.ExciseDuty_Url_Tobacco_Products).GetAwaiter().GetResult();
			var page = reader.GetFromPageRow(htmlTable.ValueTables.First());
			var formula = reader.GetFormula(page.First());
			Assert.AreEqual((true, 428.48m, "428.48 * [RSP]"), formula);
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
