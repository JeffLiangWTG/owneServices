using CargoWise.RefDbRepo.NZReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NZReferenceData.Tests
{
	[TestFixture]
	class ApplicationConfigTests
	{
		[Test]
		public void TestActiveDateMonthOffset()
		{
			Assert.That(60, Is.EqualTo(ApplicationConfig.ActiveDateMonthOffset));
		}

		[Test]
		public void TestOutputDirectory()
		{
			Assert.That(@"..\..\UXmlFiles", Is.EqualTo(ApplicationConfig.OutputDirectory));
		}

		[Test]
		public void TestVesselListUrl()
		{
			Assert.That("https://www.customs.govt.nz/api/datafiles/craft", Is.EqualTo(ApplicationConfig.VesselListUrl));
		}

		[Test]
		public void TestSupplierUrl()
		{
			Assert.That("https://www.customs.govt.nz/api/datafiles/master-supplier-codes", Is.EqualTo(ApplicationConfig.SupplierListUrl));
		}

		[Test]
		public void TestSupplierPageUrl()
		{
			Assert.That("https://www.customs.govt.nz/business/import/lodge-your-import-entry/supplier-codes-and-names/", Is.EqualTo(ApplicationConfig.SupplierPageUrl));
		}

		[Test]
		public void TestTariffDataListUrl()
		{
			Assert.That("https://www.customs.govt.nz/api/datafiles/tariff", Is.EqualTo(ApplicationConfig.TariffDataListUrl));
		}

		[Test]
		public void TestConcessionDataListUrl()
		{
			Assert.That("https://www.customs.govt.nz/api/datafiles/concession", Is.EqualTo(ApplicationConfig.ConcessionDataListUrl));
		}

		[Test]
		public void TestConcessionDetailsOverridesPath()
		{
			Assert.That(@"NZConcessionOverride.json", Is.EqualTo(ApplicationConfig.ConcessionOverrideFileName));
		}

		[Test]
		public void TestConsolidatedListOfApprovals()
		{
			Assert.That("nz_en_concessions_paper.json", Is.EqualTo(ApplicationConfig.ConsolidatedListOfApprovalsFileName), "ConsolidatedListOfApprovalsFileName");
		}

		[Test]
		public void TestTariffDetailsOverridesPath()
		{
			Assert.That("", Is.EqualTo(ApplicationConfig.TariffOverrideFileName));
		}
	}
}
