using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(ZZRefCusMapCombined))]
	class ZZRefCusMapCombinedTest : EnterpriseBusinessObjectTestCase
	{
		[TestDate(2016, 09, 01)]
		public void TestMapCustomsCodeToCW1Code()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType("REL", "BTH", "Related Party Indicator", true);
			helper.CreateCusMap("REL", "E", "E", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap("REL", "Y", "R", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap("REL", "N", "N", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			var testMap = Factory.NewWithValidTestData<ZZRefCusMapCombined>();
			testMap.ZZM_CustomsValue = "E";
			testMap.ZZM_CW1orCommercialValue = "X";
			testMap.ZZM_ZZZ_NKDataGrouping = "AU";
			testMap.ZZM_ZZP_NKMapType = "REL";
			testMap.ZZM_StartDate = ZDateTime.Today;
			testMap.ZZM_EndDate = ZDateTime.MaxSmallDateTimeValue;
			Factory.Save();
			AssertEquals("Y", ZZRefCusMapCombined.MapCustomsCodeToCW1Code(Factory, "ZA", "REL", "R", ZDateTime.Today));
			AssertEquals("N", ZZRefCusMapCombined.MapCustomsCodeToCW1Code(Factory, "ZA", "REL", "N", ZDateTime.Today));
			AssertEquals("E", ZZRefCusMapCombined.MapCustomsCodeToCW1Code(Factory, "ZA", "REL", "E", ZDateTime.Today));
			AssertEquals("", ZZRefCusMapCombined.MapCustomsCodeToCW1Code(Factory, "ZA", "REL", "A", ZDateTime.Today));
			AssertEquals("", ZZRefCusMapCombined.MapCustomsCodeToCW1Code(Factory, "ZA", "CST", "R", ZDateTime.Today));
			AssertEquals("", ZZRefCusMapCombined.MapCustomsCodeToCW1Code(Factory, "ZA", "CST", "N", ZDateTime.Today));
			AssertEquals("", ZZRefCusMapCombined.MapCustomsCodeToCW1Code(Factory, "ZA", "CST", "E", ZDateTime.Today));
			AssertEquals("", ZZRefCusMapCombined.MapCustomsCodeToCW1Code(Factory, "ZA", "CST", "A", ZDateTime.Today));
			AssertEquals("", ZZRefCusMapCombined.MapCustomsCodeToCW1Code(Factory, "AU", "REL", "R", ZDateTime.Today));
			AssertEquals("", ZZRefCusMapCombined.MapCustomsCodeToCW1Code(Factory, "AU", "REL", "N", ZDateTime.Today));
			AssertEquals("X", ZZRefCusMapCombined.MapCustomsCodeToCW1Code(Factory, "AU", "REL", "E", ZDateTime.Today));
			AssertEquals("", ZZRefCusMapCombined.MapCustomsCodeToCW1Code(Factory, "AU", "REL", "A", ZDateTime.Today));
		}

		[TestDate(2016, 09, 01)]
		public void TestMapCW1CodeToCustomsCode()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType("REL", "BTH", "Related Party Indicator", true);
			helper.CreateCusMap("REL", "E", "E", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap("REL", "Y", "R", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap("REL", "N", "N", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			var testMap = Factory.NewWithValidTestData<ZZRefCusMapCombined>();
			testMap.ZZM_CustomsValue = "X";
			testMap.ZZM_CW1orCommercialValue = "E";
			testMap.ZZM_ZZZ_NKDataGrouping = "AU";
			testMap.ZZM_ZZP_NKMapType = "REL";
			Factory.Save();
			AssertEquals("R", ZZRefCusMapCombined.MapCW1CodeToCustomsCode(Factory, "ZA", "REL", "Y", ZDateTime.Today));
			AssertEquals("N", ZZRefCusMapCombined.MapCW1CodeToCustomsCode(Factory, "ZA", "REL", "N", ZDateTime.Today));
			AssertEquals("E", ZZRefCusMapCombined.MapCW1CodeToCustomsCode(Factory, "ZA", "REL", "E", ZDateTime.Today));
			AssertEquals("", ZZRefCusMapCombined.MapCW1CodeToCustomsCode(Factory, "ZA", "REL", "A", ZDateTime.Today));
			AssertEquals("X", ZZRefCusMapCombined.MapCW1CodeToCustomsCode(Factory, "AU", "REL", "E", ZDateTime.Today));
		}

		public void TestReadOnly()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType("REL", "BTH", "Related Party Indicator", true);
			helper.CreateCusMapType("ZADOC", "OUT", "ZA supporting Doc", false);
			var testMap = Factory.NewWithValidTestData<ZZRefCusMapCombined>();
			testMap.ReadOnly = false;
			testMap.ZZM_ZZP_NKMapType = "REL";
			Assert("ReadOnly", testMap.ReadOnly);
			testMap.ZZM_ZZP_NKMapType = "ZADOC";
			Assert("ReadOnly", !testMap.ReadOnly);
			testMap.ZZM_IsSystem = true;
			Assert("ReadOnly", testMap.ReadOnly);
		}

		public void TestCanDelete()
		{
			var testMap = Factory.NewWithValidTestData<ZZRefCusMapCombined>();
			Assert("CanDelete", testMap.CanDelete);
			testMap.ZZM_IsSystem = true;
			Assert("CanDelete", !testMap.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete", "You cannot delete a system-generated record.", testMap.ReasonForNotAbleToDelete);
		}

		public void TestTemplateCopy()
		{
			var testMap = Factory.NewWithValidTestData<ZZRefCusMapCombined>();
			testMap.ZZM_IsSystem = true;
			var copiedMap = (ZZRefCusMapCombined)testMap.TemplateCopy();
			Assert("ZZM_IsSystem", !copiedMap.ZZM_IsSystem);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var bo = base.GetNewBusinessObjectForDeleteTest(factory);
			((ZZRefCusMapCombined)bo).ZZM_ZZP_NKMapType = "REL";
			return bo;
		}
	}
}
