using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.NL.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(CusAuthorizationUsage))]
sealed class CusAuthorizationUsageTest : EnterpriseBusinessObjectTestCase
{
	public void TestDefaultGoodsLocationBasedOnAuthorization()
	{
		var authorizationHeader = Factory.New<CusAuthorisationHeader>();
		authorizationHeader.CPH_Number = "1523625B02";
		authorizationHeader.CPH_Type = NLCusAuthorisationHeaderTypeList.Codes.AuthorizedConsignorTransit;
		var authorizationRule = authorizationHeader.CusAuthorisationRules.AddNew();
		authorizationRule.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
		authorizationRule.GoodsLocation.CGL_Qualifier = Customs.Business.CusGoodsLocationQualifierList.Codes.PostcodeAddress;
		authorizationRule.GoodsLocation.CGL_Type = Customs.Business.CusGoodsLocationTypeList.Codes.AuthorizedPlace;
		authorizationRule.GoodsLocation.AdditionalIdentifier = "42";
		authorizationRule.GoodsLocation.Address.E2_RN_NKCountryCode = "NL";
		authorizationRule.GoodsLocation.Address.E2_Postcode = "4950 LC";

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var authorizationUsage = nctsHeader.MovementHeader.CusAuthorizationUsages.AddNew();
		authorizationUsage.AGC_Code = NLCusAuthorisationHeaderTypeList.Codes.AuthorizedConsignorTransit;
		authorizationUsage.AGC_Number = "1523625B02";

		CombineAssertions(() =>
		{
			AssertEquals("GoodsLocation is defaulted when AdditionalIdentifier is equal to 42 (housenumber from address in authorizationHeader)", "42", nctsHeader.MovementHeader.GoodsLocation.AdditionalIdentifier);
			AssertEquals("GoodsLocationDescription is updated", "T;B;4950 LC;42;NL", nctsHeader.MovementHeader.GoodsLocationDescription);
		});
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		var cusAuthorizationUsage = nctsHeader.MovementHeader.CusAuthorizationUsages.AddNew();
		cusAuthorizationUsage.AGC_Code = "abc";
		cusAuthorizationUsage.AGC_Number = "123";
		cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;
		return cusAuthorizationUsage;
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
}
