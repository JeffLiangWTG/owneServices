using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(AuthorizationProvider))]
sealed class AuthorizationProviderTest : Customs.Business.Testing.DataProviderTestCase<AuthorizationProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new AuthorizationProvider(null, ZInt.Zero));
	}

	public void TestSequenceNumeric()
	{
		AssertEquals(provider.SequenceNumeric, 123);
	}

	public void TestType() => CombineAssertions(() =>
	{
		cusAuthorizationUsage.AGC_Number = "1";
		cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;

		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusMapType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, MapDirectionList.Codes.OUT, "EUNAU", true);
		helper.CreateCusMap(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir, NLConstants.AuthorisationTypes.C520, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateCusMap(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, NLConstants.CusPermitHeaderTypes.ACR, NLConstants.AuthorisationTypes.C521, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateCusMap(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit, NLConstants.AuthorisationTypes.C522, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateCusMap(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, NLConstants.CusPermitHeaderTypes.SSE, NLConstants.AuthorisationTypes.C523, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateCusMap(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, NLConstants.CusPermitHeaderTypes.TransitOperation, NLConstants.AuthorisationTypes.C524, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		Factory.Save();

		AssertEquals(GetMessage(), NLConstants.AuthorisationTypes.C520, provider.Type);

		cusAuthorizationUsage.AGC_Code = NLConstants.CusPermitHeaderTypes.ACR;
		AssertEquals(GetMessage(), NLConstants.AuthorisationTypes.C521, provider.Type);

		cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
		AssertEquals(GetMessage(), NLConstants.AuthorisationTypes.C522, provider.Type);

		cusAuthorizationUsage.AGC_Code = NLConstants.CusPermitHeaderTypes.SSE;
		AssertEquals(GetMessage(), NLConstants.AuthorisationTypes.C523, provider.Type);

		cusAuthorizationUsage.AGC_Code = NLConstants.CusPermitHeaderTypes.TransitOperation;
		AssertEquals(GetMessage(), NLConstants.AuthorisationTypes.C524, provider.Type);

		cusAuthorizationUsage.AGC_Code = "123";
		AssertEquals(GetMessage(), string.Empty, provider.Type);

		string GetMessage() => $"{nameof(cusAuthorizationUsage.AGC_Code)} = {cusAuthorizationUsage.AGC_Code}";
	});

	public void TestReferenceNumber()
	{
		var agcNumber = "345";
		cusAuthorizationUsage.AGC_Number = agcNumber;

		AssertEquals(agcNumber, provider.ReferenceNumber);
	}

	public void TestHolderOfAuthorisation()
	{
		AssertNull(provider.HolderOfAuthorisation);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var header = Factory.New<NctsHeader>();
		cusAuthorizationUsage = header.CusAuthorizationUsages.AddNew();
		provider = new AuthorizationProvider(cusAuthorizationUsage, 123);
	}

	AuthorizationProvider provider;
	CusAuthorizationUsage cusAuthorizationUsage;

	protected override AuthorizationProvider GetProvider() => provider;
}
