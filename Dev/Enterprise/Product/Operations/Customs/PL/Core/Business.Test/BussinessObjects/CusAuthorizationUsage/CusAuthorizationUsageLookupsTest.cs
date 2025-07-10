using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class CusAuthorizationUsageLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCodeList_InvoiceLine_NotMapped()
	{
		var invoiceLine = Factory.New<Declaration.JobComInvoiceLine>();
		var authorizationUsage = invoiceLine.CusAuthorizationUsages.AddNew();
		var lookups = authorizationUsage.Lookups;

		var codes = (CodeDescriptionPairList)lookups.CodeList;
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("Codes", new ZString[] { "BTI", "BOI" }, codes.GetAllCodes());
			AssertEquals("1 Description",
				"C626 - Decision relating to Binding Tariff Information (Column 1a, Annex A of Delegated Regulation (EU) 2015/2446)",
				codes.GetDescriptionFromCode("BTI"));
			AssertEquals("2 Description",
				"C627 - Decision relating to Binding Origin Information (Column 1b, Annex A of Delegated Regulation (EU) 2015/2446)",
				codes.GetDescriptionFromCode("BOI"));
		});
	}

	public void TestCodeList_InvoiceLine_Mapped()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eunau = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU;
		helper.CreateCusMapType(eunau, MapDirectionList.Codes.OUT, "EUNAU", true);
		helper.CreateCusMap(eunau, "cw1", "C626", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateCusMap(eunau, "cw2", "C627", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		Factory.Save();

		var invoiceLine = Factory.New<Declaration.JobComInvoiceLine>();
		var authorizationUsage = invoiceLine.CusAuthorizationUsages.AddNew();
		var lookups = authorizationUsage.Lookups;

		var codes = (CodeDescriptionPairList)lookups.CodeList;
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("Codes", new ZString[] { "cw1", "cw2" }, codes.GetAllCodes());
			AssertEquals("1 Description",
				"C626 - Decision relating to Binding Tariff Information (Column 1a, Annex A of Delegated Regulation (EU) 2015/2446)",
				codes.GetDescriptionFromCode("cw1"));
			AssertEquals("2 Description",
				"C627 - Decision relating to Binding Origin Information (Column 1b, Annex A of Delegated Regulation (EU) 2015/2446)",
				codes.GetDescriptionFromCode("cw2"));
		});
	}
}
