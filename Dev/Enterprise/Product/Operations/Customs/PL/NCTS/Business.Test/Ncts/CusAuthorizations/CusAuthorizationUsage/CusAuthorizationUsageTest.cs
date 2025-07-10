using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CusAuthorizationUsage))]
sealed class CusAuthorizationUsageTest : EnterpriseBusinessObjectTestCase
{
	public void TestICusAuthorizationUsageIsCorrectlySetup()
	{
		var authorizationUsage = CreateAuthorizationUsage();
		authorizationUsage.FillWithValidTestData();
		Factory.Save();
		CombineAssertions(() =>
		{
			var newFactory = new BusinessObjectFactory();
			AssertType<CusAuthorizationUsage>(newFactory.Load<Integration.Customs.PL.INctsCusAuthorizationUsage>(authorizationUsage.PK));
			newFactory = new BusinessObjectFactory();
			AssertType<CusAuthorizationUsage>(newFactory.Load<CusAuthorizationUsage>(authorizationUsage.PK));
		});
	}

	public void TestLookups()
	{
		var cusAuthorizationUsage = CreateAuthorizationUsage();
		AssertType<CusAuthorizationUsageLookups>(cusAuthorizationUsage.Lookups);
	}

	public void TestValidation()
	{
		AssertType<CusAuthorizationUsageValidation>(CreateAuthorizationUsage().Validation);
	}

	public void TestAGC_Location_Caption()
	{
		var cusAuthorizationUsage = CreateAuthorizationUsage();
		NCTSTestHelper.AssertCaptions(cusAuthorizationUsage.AGC_LocationInfo, "Authorization Location", "Auth. Location", "Location");
	}

	public void TestAGC_Location_DefaultSet()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;

		var org = Factory.NewWithValidTestData<OrgHeader>();
		var header1 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		header1.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		header1.CPH_Number = "Number1";
		header1.CPH_Type = "ACE";
		header1.CPH_OH_PermitHolder = org.PK;
		var rule1 = header1.CusAuthorisationRules.AddNew();
		rule1.CPR_RuleCode = "LOC";
		rule1.CPR_ValueFrom = "PL11";
		var rule2 = header1.CusAuthorisationRules.AddNew();
		rule2.CPR_RuleCode = "LOC";
		rule2.CPR_ValueFrom = "PL12";
		var header2 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		header2.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		header2.CPH_Number = "Number2";
		header2.CPH_Type = "ACE";
		header2.CPH_OH_PermitHolder = org.PK;
		var rule3 = header2.CusAuthorisationRules.AddNew();
		rule3.CPR_RuleCode = "LOC";
		rule3.CPR_ValueFrom = "PL2";

		CombineAssertions(() =>
		{
			var cusAuthorizationUsage = nctsHeader.MovementHeader.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_Number = "Number1";
			AssertEquals("Default Set", cusAuthorizationUsage.AGC_Location, "PL11");
			var cusAuthorizationUsage1 = nctsHeader.MovementHeader.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage1.AGC_Number = "Number2";
			AssertEquals("Default Set", cusAuthorizationUsage1.AGC_Location, "PL2");
			var header3 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			header3.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			header3.CPH_Number = "Number3";
			header3.CPH_Type = "ACE";
			header3.CPH_OH_PermitHolder = org.PK;
			var rule4 = header3.CusAuthorisationRules.AddNew();
			rule4.CPR_RuleCode = "LOC";
			rule4.CPR_ValueFrom = "PL3";
			cusAuthorizationUsage1.AGC_Number = "Number3";
			AssertEquals("Not Default Set when location is not empty", cusAuthorizationUsage1.AGC_Location, "PL2");
			cusAuthorizationUsage1.AGC_Number = ZString.Empty;
			cusAuthorizationUsage1.AGC_Location = ZString.Empty;
			cusAuthorizationUsage1.AGC_Number = "Number3";
			AssertEquals("Default Set when change number to get a header which has just one rule", cusAuthorizationUsage1.AGC_Location, "PL3");
			cusAuthorizationUsage1.AGC_Code = ZString.Empty;
			cusAuthorizationUsage1.AGC_Location = ZString.Empty;
			cusAuthorizationUsage1.AGC_Code = "ACE";
			AssertEquals("Default Set when change code to get a header which has just one rule", cusAuthorizationUsage1.AGC_Location, "PL3");
			cusAuthorizationUsage1.AGC_OH_Owner = ZGuid.Empty;
			cusAuthorizationUsage1.AGC_Location = ZString.Empty;
			cusAuthorizationUsage1.AGC_OH_Owner = org.PK;
			AssertEquals("Default Set when change owner to get a header which has just one rule", cusAuthorizationUsage1.AGC_Location, "PL3");
		});
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();
	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
	protected override BusinessObject GetNewBusinessObject()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var cusAuthorizationUsage = CreateAuthorizationUsage();
		cusAuthorizationUsage.AGC_Code = "abc";
		cusAuthorizationUsage.AGC_Number = "123";
		cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;
		return cusAuthorizationUsage;
	}
	CusAuthorizationUsage CreateAuthorizationUsage()
	{
		var testNctsHeader = Factory.New<NctsHeader>();
		testNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		var authorization = testNctsHeader.CusAuthorizationUsages.AddNew();
		return authorization;
	}
}
