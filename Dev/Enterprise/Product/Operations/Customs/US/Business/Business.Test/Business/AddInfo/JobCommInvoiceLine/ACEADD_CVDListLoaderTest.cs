using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEADD_CVDListLoaderTest : TestCaseWithFactory
	{
		public void TestCaseNumberList()
		{
			var caseNumber = Factory.New<USCACCase>();
			caseNumber.U5_CaseNumber = "ADD111";
			caseNumber.U5_CaseStatusDate = ZDateTime.Today;
			caseNumber.U5_ISOCountryCode = "KR";
			caseNumber.CaseTariffs.AddNew().U9_TariffNumber = "22030060";
			var caseNumber2 = Factory.New<USCACCase>();
			caseNumber2.U5_CaseNumber = "CVD111";
			caseNumber2.U5_CaseStatusDate = ZDateTime.Today;
			caseNumber2.U5_ISOCountryCode = "KR";
			caseNumber2.CaseTariffs.AddNew().U9_TariffNumber = "22030060";
			var caseNumber3 = Factory.New<USCACCase>();
			caseNumber3.U5_CaseNumber = "CVD112";
			caseNumber3.U5_CaseStatusDate = ZDateTime.Today;
			caseNumber3.U5_ISOCountryCode = "KR";
			caseNumber3.CaseTariffs.AddNew().U9_TariffNumber = "33030060";
			var caseNumber4 = Factory.New<USCACCase>();
			caseNumber4.U5_CaseNumber = "CVD113";
			caseNumber4.U5_CaseStatusDate = ZDateTime.Today;
			caseNumber4.U5_ISOCountryCode = "MX";
			Factory.Save();
			var collection = new ACEADD_CVDListLoader(Factory).GetCaseNumberList("KR", "ADD111", "A", "22030060");
			AssertNotNull(collection.FilterBusinessObjectDefaults["Case Number:Property"]);
			collection = new ACEADD_CVDListLoader(Factory).GetCaseNumberList("KR", "CVD111", "C", "22030060");
			AssertNotNull(collection.FilterBusinessObjectDefaults["Case Number:Property"]);
			collection = new ACEADD_CVDListLoader(Factory).GetCaseNumberList("MX", "CVD113", "C", "11111111");
			AssertEquals(1, collection.Count);
			collection = new ACEADD_CVDListLoader(Factory).GetCaseNumberList("KR", "", "A", "22030060");
			AssertNotNull(collection.FilterBusinessObjectDefaults["Country Code:Property"]);
			AssertNotNull(collection.FilterBusinessObjectDefaults["Tariff Number:Property"]);
			AssertNotNull(collection.FilterBusinessObjectDefaults["Case Number:Property"]);
			collection.FilterBusinessObjectDefaults.RemoveAll();
			Assert("When default filter is removed, it should still display the biz obj, but should be unselectable", !caseNumber3.MatchesFilter(collection.CompleteFilter));
		}

		public void TestCaseNumberListForCA()
		{
			var addCollection = new ACEADD_CVDListLoader(Factory).GetCaseNumberList("XC", "", "A", "22030060");
			AssertEquals("Country Code should be [CA]", "CA", addCollection.FilterBusinessObjectDefaults["Country Code:Property"].Value);
			var cvdCollection = new ACEADD_CVDListLoader(Factory).GetCaseNumberList("XC", "", "C", "22030060");
			AssertEquals("Country Code should be [CA]", "CA", cvdCollection.FilterBusinessObjectDefaults["Country Code:Property"].Value);
		}
	}
}
