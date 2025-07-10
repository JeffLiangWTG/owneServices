using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CusAuthorizationUsagePhase5ValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckAGC_CodeIsInvalid()
	{
		var message = "Entered Authorization code is not in the list.";
		var testNctsHeader = Factory.New<NctsHeader>();
		testNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var authorization = testNctsHeader.CusAuthorizationUsages.AddNew();
		authorization.AGC_Code = "XXX";
		AssertHasMessageErrorContaining("Invalid code", authorization.AGC_CodeInfo, message);
	}

	public void TestCheckAGC_LocationMandatory()
	{
		const string errorMessage = "You have not selected Location.";

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;

		var org = Factory.NewWithValidTestData<OrgHeader>();
		var header1 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		header1.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		header1.CPH_Number = "Number2";
		header1.CPH_Type = "ACE";
		header1.CPH_OH_PermitHolder = org.PK;
		var rule1 = header1.CusAuthorisationRules.AddNew();
		rule1.CPR_RuleCode = "LOC";
		rule1.CPR_ValueFrom = "WAW";
		var rule2 = header1.CusAuthorisationRules.AddNew();
		rule2.CPR_RuleCode = "LOC";
		rule2.CPR_ValueFrom = "WBW";

		CombineAssertions(() =>
		{
			var cusAuthorizationUsages = nctsHeader.MovementHeader.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsages.AGC_Number = "Number2";
			cusAuthorizationUsages.AGC_Location = "";
			AssertHasMessageError("Location is empty", cusAuthorizationUsages.AGC_LocationInfo, errorMessage);
			cusAuthorizationUsages.AGC_Location = "WAW";
			AssertNoMessageError("Location is not empty", cusAuthorizationUsages.AGC_LocationInfo, errorMessage);
		});
	}

	public void TestCheckAGC_LocationValid()
	{
		const string errorMessage = "The Location you have selected is not in the list.";
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;

		var org = Factory.NewWithValidTestData<OrgHeader>();
		var header1 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		header1.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		header1.CPH_Number = "Number2";
		header1.CPH_Type = "ACE";
		header1.CPH_OH_PermitHolder = org.PK;
		var rule1 = header1.CusAuthorisationRules.AddNew();
		rule1.CPR_RuleCode = "LOC";
		rule1.CPR_ValueFrom = "WAW";
		var rule2 = header1.CusAuthorisationRules.AddNew();
		rule2.CPR_RuleCode = "LOC";
		rule2.CPR_ValueFrom = "WBW";
		Factory.Save();

		CombineAssertions(() =>
		{
			var cusAuthorizationUsages = nctsHeader.MovementHeader.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsages.AGC_Number = "Number2";
			cusAuthorizationUsages.AGC_Code = "ACE";
			cusAuthorizationUsages.AGC_OH_Owner = org.PK;
			cusAuthorizationUsages.AGC_Location = "WWW";
			AssertHasMessageError("Location is Invalid", cusAuthorizationUsages.AGC_LocationInfo, errorMessage);
			cusAuthorizationUsages.AGC_Location = "WAW";
			AssertNoMessageError("Location is Valid", cusAuthorizationUsages.AGC_LocationInfo, errorMessage);
		});
	}
}
