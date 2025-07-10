using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.NL.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(CusGoodsLocation))]
sealed class CusGoodsLocationTest : EnterpriseBusinessObjectTestCase
{
	public void TestSetDefaultValues()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var goodsLocation = nctsHeader.MovementHeader.GoodsLocation;

		CombineAssertions("Departure declaration, no ACR-authorization", () =>
		{
			AssertEquals("Default Qualifier = 'T'", "T", goodsLocation.CGL_Qualifier);
			AssertEquals("Default Type = 'A' (no ACR-authorization)", "A", goodsLocation.CGL_Type);
			AssertEquals("Default Country = 'NL'", "NL", goodsLocation.Address.E2_RN_NKCountryCode);
		});

		CombineAssertions("Departure declaration, with ACR-authorization", () =>
		{
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var authorization = nctsHeader.MovementHeader.CusAuthorizationUsages.AddNew();
			authorization.AGC_Code = Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
			goodsLocation = nctsHeader.MovementHeader.GoodsLocation;

			AssertEquals("Default Qualifier = 'T'", "T", goodsLocation.CGL_Qualifier);
			AssertEquals("Default Type = 'B'", "B", goodsLocation.CGL_Type);
			AssertEquals("Default Country = 'NL'", "NL", goodsLocation.Address.E2_RN_NKCountryCode);
		});

		CombineAssertions("Arrival declaration, Goods Location on arrival header", () =>
		{
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			goodsLocation = nctsHeader.ArrivalMovementHeader.GoodsLocation;

			AssertEquals("Default Qualifier = 'T'", "T", goodsLocation.CGL_Qualifier);
			AssertEquals("Default Type = 'A'", "A", goodsLocation.CGL_Type);
			AssertEquals("Default Country = 'NL'", "NL", goodsLocation.Address.E2_RN_NKCountryCode);
		});

		CombineAssertions("Arrival declaration, with ACE-authorization", () =>
		{
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var authorization = nctsHeader.CusAuthorizationUsages.AddNew();
			authorization.AGC_Code = Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
			goodsLocation = nctsHeader.ArrivalMovementHeader.GoodsLocation;

			AssertEquals("Default Qualifier = 'T'", "T", goodsLocation.CGL_Qualifier);
			AssertEquals("Default Type = 'B'", "B", goodsLocation.CGL_Type);
			AssertEquals("Default Country = 'NL'", "NL", goodsLocation.Address.E2_RN_NKCountryCode);
		});

		CombineAssertions("Arrival declaration, Goods Location on arrival incident", () =>
		{
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ExportFlag = "Y";
			var incident = nctsHeader.EnRouteIncidents.AddNew();
			goodsLocation = incident.GoodsLocation as CusGoodsLocation;

			AssertEquals("Qualifier should not be defaulted", string.Empty, goodsLocation.CGL_Qualifier);
			AssertEquals("Type should not be defaulted", string.Empty, goodsLocation.CGL_Type);
			AssertEquals("Country should not be defaulted", string.Empty, goodsLocation.Address.E2_RN_NKCountryCode);
		});
	}

	public void TestSetDefaultsFromAuthorizationIfNeeded_Departure()
	{
		var authorizationHeader = Factory.New<CusAuthorisationHeader>();
		authorizationHeader.CPH_Number = "1523625B02";
		authorizationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
		authorizationHeader.CPH_RN_NKCountryCode = "NL";
		var authorizationRule = authorizationHeader.CusAuthorisationRules.AddNew();
		authorizationRule.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
		authorizationRule.GoodsLocation.CGL_Qualifier = Customs.Business.CusGoodsLocationQualifierList.Codes.PostcodeAddress;
		authorizationRule.GoodsLocation.CGL_Type = Customs.Business.CusGoodsLocationTypeList.Codes.AuthorizedPlace;
		authorizationRule.GoodsLocation.AdditionalIdentifier = "42";
		authorizationRule.GoodsLocation.Address.E2_RN_NKCountryCode = "NL";
		authorizationRule.GoodsLocation.Address.E2_Postcode = "4950 LC";
		authorizationRule.CPR_ValueFrom = "T;B;4950 LC;42;NL";

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var goodsLocation = nctsHeader.MovementHeader.GoodsLocation;

		CombineAssertions("No AuthorizationHeader passed to procedure", () =>
		{
			goodsLocation.SetDefaultsFromAuthorizationIfNeeded(null);
			AssertEquals("Qualifier = 'T'", "T", goodsLocation.CGL_Qualifier);
			AssertEquals("Type = 'A'", "A", goodsLocation.CGL_Type);
			AssertEquals("Additional Identifier is empty", string.Empty, goodsLocation.CGL_AdditionalIdentifier);
			AssertEquals("Address.E2_PostCode is empty", string.Empty, goodsLocation.Address.E2_Postcode);
			AssertEquals("Address.E2_NK_RNCountryCode = 'NL'", "NL", goodsLocation.Address.E2_RN_NKCountryCode);
		});

		var authorizationUsage = nctsHeader.MovementHeader.CusAuthorizationUsages.AddNew();
		authorizationUsage.AGC_Code = Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
		authorizationUsage.AGC_Number = "1523625B02";

		CombineAssertions("AuthorizationHeader passed to procedure on departure declaration", () =>
		{
			goodsLocation.SetDefaultsFromAuthorizationIfNeeded(authorizationHeader);
			AssertEquals("Qualifier = 'T'", "T", goodsLocation.CGL_Qualifier);
			AssertEquals("Type = 'A'", "A", goodsLocation.CGL_Type);
			AssertEquals("Additional Identifier = '42'", "42", goodsLocation.CGL_AdditionalIdentifier);
			AssertEquals("Address.E2_PostCode = '4950 LC'", "4950 LC", goodsLocation.Address.E2_Postcode);
			AssertEquals("Address.E2_NK_RNCountryCode = 'NL'", "NL", goodsLocation.Address.E2_RN_NKCountryCode);
		});

		goodsLocation.AdditionalIdentifier = string.Empty;
		goodsLocation.Address.E2_Postcode = string.Empty;

		var authorizationRule2 = authorizationHeader.CusAuthorisationRules.AddNew();
		authorizationRule2.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
		authorizationRule2.GoodsLocation.CGL_Qualifier = Customs.Business.CusGoodsLocationQualifierList.Codes.PostcodeAddress;
		authorizationRule2.GoodsLocation.CGL_Type = Customs.Business.CusGoodsLocationTypeList.Codes.AuthorizedPlace;
		authorizationRule2.GoodsLocation.AdditionalIdentifier = "12";
		authorizationRule2.GoodsLocation.Address.E2_RN_NKCountryCode = "NL";
		authorizationRule2.GoodsLocation.Address.E2_Postcode = "4952 AK";
		authorizationRule2.CPR_ValueFrom = "T:B;4952 AK;12;NL";

		CombineAssertions("AuthorizationHeader with multiple locations (rules) passed to procedure on departure declaration", () =>
		{
			goodsLocation.SetDefaultsFromAuthorizationIfNeeded(authorizationHeader);
			AssertEquals("Qualifier = 'T'", "T", goodsLocation.CGL_Qualifier);
			AssertEquals("Type = 'A'", "A", goodsLocation.CGL_Type);
			AssertEquals("Additional Identifier is empty", string.Empty, goodsLocation.CGL_AdditionalIdentifier);
			AssertEquals("Address.E2_PostCode is empty", string.Empty, goodsLocation.Address.E2_Postcode);
			AssertEquals("Address.E2_NK_RNCountryCode = 'NL'", "NL", goodsLocation.Address.E2_RN_NKCountryCode);
		});
	}

	public void TestSetDefaultsFromAuthorizationIfNeeded_Arrival()
	{
		var authorizationHeader = Factory.New<CusAuthorisationHeader>();
		authorizationHeader.CPH_Number = "1523625B02";
		authorizationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
		authorizationHeader.CPH_RN_NKCountryCode = "NL";
		var authorizationRule = authorizationHeader.CusAuthorisationRules.AddNew();
		authorizationRule.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
		authorizationRule.GoodsLocation.CGL_Qualifier = Customs.Business.CusGoodsLocationQualifierList.Codes.PostcodeAddress;
		authorizationRule.GoodsLocation.CGL_Type = Customs.Business.CusGoodsLocationTypeList.Codes.AuthorizedPlace;
		authorizationRule.GoodsLocation.AdditionalIdentifier = "42";
		authorizationRule.GoodsLocation.Address.E2_RN_NKCountryCode = "NL";
		authorizationRule.GoodsLocation.Address.E2_Postcode = "4950 LC";
		authorizationRule.CPR_ValueFrom = "T;B;4950 LC;42;NL";

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		var goodsLocation = nctsHeader.ArrivalMovementHeader.GoodsLocation;

		CombineAssertions("No AuthorizationHeader passed to procedure", () =>
		{
			goodsLocation.SetDefaultsFromAuthorizationIfNeeded(null);
			AssertEquals("Qualifier = 'T'", "T", goodsLocation.CGL_Qualifier);
			AssertEquals("Type = 'A'", "A", goodsLocation.CGL_Type);
			AssertEquals("Additional Identifier is empty", string.Empty, goodsLocation.CGL_AdditionalIdentifier);
			AssertEquals("Address.E2_PostCode is empty", string.Empty, goodsLocation.Address.E2_Postcode);
			AssertEquals("Address.E2_NK_RNCountryCode = 'NL'", "NL", goodsLocation.Address.E2_RN_NKCountryCode);
		});

		var authorizationUsage = nctsHeader.CusAuthorizationUsages.AddNew();
		authorizationUsage.AGC_Code = Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
		authorizationUsage.AGC_Number = "1523625B02";

		CombineAssertions("AuthorizationHeader passed to procedure on arrival declaration", () =>
		{
			goodsLocation.SetDefaultsFromAuthorizationIfNeeded(authorizationHeader);
			AssertEquals("Qualifier = 'T'", "T", goodsLocation.CGL_Qualifier);
			AssertEquals("Type = 'A'", "A", goodsLocation.CGL_Type);
			AssertEquals("Additional Identifier = '42'", "42", goodsLocation.CGL_AdditionalIdentifier);
			AssertEquals("Address.E2_PostCode = '4950 LC'", "4950 LC", goodsLocation.Address.E2_Postcode);
			AssertEquals("Address.E2_NK_RNCountryCode = 'NL'", "NL", goodsLocation.Address.E2_RN_NKCountryCode);
		});

		goodsLocation.AdditionalIdentifier = string.Empty;
		goodsLocation.Address.E2_Postcode = string.Empty;

		var authorizationRule2 = authorizationHeader.CusAuthorisationRules.AddNew();
		authorizationRule2.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
		authorizationRule2.GoodsLocation.CGL_Qualifier = Customs.Business.CusGoodsLocationQualifierList.Codes.PostcodeAddress;
		authorizationRule2.GoodsLocation.CGL_Type = Customs.Business.CusGoodsLocationTypeList.Codes.AuthorizedPlace;
		authorizationRule2.GoodsLocation.AdditionalIdentifier = "12";
		authorizationRule2.GoodsLocation.Address.E2_RN_NKCountryCode = "NL";
		authorizationRule2.GoodsLocation.Address.E2_Postcode = "4952 AK";
		authorizationRule2.CPR_ValueFrom = "T:B;4952 AK;12;NL";

		CombineAssertions("AuthorizationHeader with multiple locations (rules) passed to procedure on arrival declaration", () =>
		{
			goodsLocation.SetDefaultsFromAuthorizationIfNeeded(authorizationHeader);
			AssertEquals("Qualifier = 'T'", "T", goodsLocation.CGL_Qualifier);
			AssertEquals("Type = 'A'", "A", goodsLocation.CGL_Type);
			AssertEquals("Additional Identifier is empty", string.Empty, goodsLocation.CGL_AdditionalIdentifier);
			AssertEquals("Address.E2_PostCode is empty", string.Empty, goodsLocation.Address.E2_Postcode);
			AssertEquals("Address.E2_NK_RNCountryCode = 'NL'", "NL", goodsLocation.Address.E2_RN_NKCountryCode);
		});
	}

	public void TestLookups()
	{
		var cusGoodsLocation = GetNewBusinessObject(Factory);
		AssertType<CusGoodsLocationLookups>(cusGoodsLocation.Lookups);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

	CusGoodsLocation GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		return nctsHeader.ArrivalMovementHeader.GoodsLocation;
	}
}
