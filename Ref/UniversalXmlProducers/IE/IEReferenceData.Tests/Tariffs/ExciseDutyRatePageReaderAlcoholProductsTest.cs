using System.Linq;
using CargoWise.RefDbRepo.IEReferenceData.Services;
using CargoWise.RefDbRepo.IEReferenceData.Tests;
using iText.Layout.Element;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.IEReferenceData.Tariffs.Tests
{
	[TestFixture]
	class ExciseDutyRatePageReaderAlcoholProductsTest
	{
		[Test]
		public void TestGetFromPageRow()
		{
			var reader = new ExciseDutyRatePageReaderAlcoholProducts(TestConfig);

			var htmlTable = HtmlTableLoader.Read<object>(TestConfig.ExciseDuty_Url_Alcohol_Products).GetAwaiter().GetResult();
			var page = reader.GetFromPageRow(htmlTable.ValueTables.First());
			Assert.AreEqual(8, page.Length);
			Assert.AreEqual("Spirits", page[0].SearchText);
			Assert.AreEqual("€42.57 per litre of alcohol in the spirits", page[0].TaxRate);
		}

		[Test]
		public void TestGetFormula()
		{
			var reader = new ExciseDutyRatePageReaderAlcoholProducts(TestConfig);
			var htmlTable = HtmlTableLoader.Read<object>(TestConfig.ExciseDuty_Url_Alcohol_Products).GetAwaiter().GetResult();
			var page = reader.GetFromPageRow(htmlTable.ValueTables.First());
			var formula = reader.GetFormula(page.First());
			Assert.AreEqual((true, 0.4257m, "0.4257 * [LPA]"), formula);
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
