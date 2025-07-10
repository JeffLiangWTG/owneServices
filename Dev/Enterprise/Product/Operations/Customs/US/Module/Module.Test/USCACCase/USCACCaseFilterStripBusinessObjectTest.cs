using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCACCaseFilterStripBusinessObject))]
	sealed class USCACCaseFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilters()
		{
			var filter = new USCACCaseFilterStripBusinessObject();
			AssertNotNull(filter["Case Number"]);
			AssertNotNull(filter["Case Status Code"]);
			AssertNotNull(filter["Case Status Date"]);
			AssertNotNull(filter["Description"]);
			AssertNotNull(filter["Official Name"]);
			AssertNotNull(filter["Contact Name"]);
			AssertNotNull(filter["Contact Office"]);
			AssertNotNull(filter["Manufacturer ID"]);
			AssertNotNull(filter["Manufacturer Name"]);
			AssertNotNull(filter["Foreign Exporter ID"]);
			AssertNotNull(filter["Foreign Exporter Name"]);
			AssertNotNull(filter["Related Case Number"]);
			AssertNotNull(filter["Country Code"]);
			AssertNotNull(filter["Tariff Number"]);
			AssertNotNull(filter["Case Status"]);
			AssertEquals(12, filter["Tariff Number"].MaxLength);
		}

		public void TestInactiveExludedQueryAndBySideEffectTariff()
		{
			var caseNumber = Factory.New<USCACCase>();
			caseNumber.U5_CaseNumber = "ADD111";
			caseNumber.U5_CaseStatusDate = ZDateTime.Today;
			caseNumber.CaseTariffs.AddNew().U9_TariffNumber = "22030060";
			Factory.Save();
			var filter = new USCACCaseFilterStripBusinessObject();
			var flagsFilter = (ModuleFlagsFilter)filter["Case Status"];
			flagsFilter.IsActive = true;
			flagsFilter.Property0 = false;
			var tariffFilter = (ModuleTextFilter)filter["Tariff Number"];
			tariffFilter.IsActive = true;
			tariffFilter.Property = "22030060";
			AssertEquals(1, Factory.GetDatabaseCount(typeof(USCACCase), filter.Filter));
			flagsFilter.IsActive = true;
			flagsFilter.Property0 = true;
			AssertEquals(0, Factory.GetDatabaseCount(typeof(USCACCase), filter.Filter));
		}

		public void TestTariffFilter()
		{
			var caseNumber = Factory.New<USCACCase>();
			caseNumber.U5_CaseNumber = "ATEST001";
			caseNumber.U5_CaseStatusDate = ZDateTime.Today;
			caseNumber.CaseTariffs.AddNew().U9_TariffNumber = "00009999"; // Dummy tariff for testing
			var caseNumber2 = Factory.New<USCACCase>();
			caseNumber2.U5_CaseNumber = "ATEST002";
			caseNumber2.U5_CaseStatusDate = ZDateTime.Today;
			caseNumber2.CaseTariffs.AddNew().U9_TariffNumber = "00123456"; // Dummy tariff for testing
			var caseNumber3 = Factory.New<USCACCase>();
			caseNumber3.U5_CaseNumber = "ATEST003";
			caseNumber3.U5_CaseStatusDate = ZDateTime.Today;
			caseNumber3.CaseTariffs.AddNew().U9_TariffNumber = "00123456"; // Dummy tariff for testing
			Factory.Save();
			var filter = new USCACCaseFilterStripBusinessObject();
			var tariffFilter = (ModuleTextFilter)filter["Tariff Number"];
			tariffFilter.IsActive = true;
			tariffFilter.Property = "000";
			AssertEquals("Should only return Anti Dumping Case test1", 1, Factory.GetDatabaseCount(typeof(USCACCase), filter.Filter));
			tariffFilter.Property = "001";
			AssertEquals("Should return Anti Dumping Case test2 & test3", 2, Factory.GetDatabaseCount(typeof(USCACCase), filter.Filter));
			tariffFilter.Property = "00";
			AssertEquals("Should return Anti Dumping Cases 1, 2 & 3", 3, Factory.GetDatabaseCount(typeof(USCACCase), filter.Filter));
			tariffFilter.Property = "0005";
			AssertEquals(0, Factory.GetDatabaseCount(typeof(USCACCase), filter.Filter));
			tariffFilter.Property = "00009999";
			AssertEquals(1, Factory.GetDatabaseCount(typeof(USCACCase), filter.Filter));
			tariffFilter.Property = "0000999900";
			AssertEquals(1, Factory.GetDatabaseCount(typeof(USCACCase), filter.Filter));
		}

		public void TestNoExceptionThrownWhenTariffNumberExceeds10characters()
		{
			var caseNumber = Factory.New<USCACCase>();
			caseNumber.U5_CaseNumber = "ATEST001";
			caseNumber.U5_CaseStatusDate = ZDateTime.Today;
			caseNumber.CaseTariffs.AddNew().U9_TariffNumber = "7210706090";
			Factory.Save();
			var filter = new USCACCaseFilterStripBusinessObject();
			var tariffFilter = (ModuleTextFilter)filter["Tariff Number"];
			tariffFilter.IsActive = true;
			tariffFilter.Property = "72107060901";
			AssertNoExceptionThrown("Should not throw exception when tariff number exceeds 10 characters", () => Factory.GetDatabaseCount(typeof(USCACCase), filter.Filter));
			AssertEquals("7210.70.6090", tariffFilter.Property);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new USCACCaseFilterStripBusinessObject();
	}
}
