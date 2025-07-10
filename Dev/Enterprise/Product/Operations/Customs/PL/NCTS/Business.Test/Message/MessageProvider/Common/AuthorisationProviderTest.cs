using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class AuthorisationProviderTest : Customs.Business.Testing.DataProviderTestCase<AuthorisationProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null CusAuthorizationUsage", "Value cannot be null.\r\nParameter name: cusAuthorizationUsage", () => new AuthorisationProvider(null, ZInt.Zero));
		});
	}

	public void TestSequenceNumber()
	{
		AssertEquals(GetProvider().SequenceNumber, "2");
	}

	public void TestAuthorisationType()
	{
		cusAuthorizationUsage.AGC_Number = "1";
		cusAuthorizationUsage.AGC_Code = CusPermitHeaderTypesList.Codes.SSE;

		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusMapType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, MapDirectionList.Codes.OUT, "EUNAU", true);
		helper.CreateCusMap(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, CusPermitHeaderTypesList.Codes.ACR, AuthorizationTypeList.Codes.C521, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);

		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals(GetMessage(), CusPermitHeaderTypesList.Codes.SSE, GetProvider().AuthorisationType);

			cusAuthorizationUsage.AGC_Code = CusPermitHeaderTypesList.Codes.ACR;
			AssertEquals(GetMessage(), AuthorizationTypeList.Codes.C521, GetProvider().AuthorisationType);
		});

		string GetMessage() => $"{nameof(cusAuthorizationUsage.AGC_Code)} = {cusAuthorizationUsage.AGC_Code}";
	}

	public void TestReferenceNumber()
	{
		var agcNumber = "345";
		cusAuthorizationUsage.AGC_Number = agcNumber;

		AssertEquals(agcNumber, GetProvider().ReferenceNumber);
	}

	protected override AuthorisationProvider GetProvider() => new AuthorisationProvider(cusAuthorizationUsage, 2);

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.MovementHeader.CustomsOffices.RemoveAndDeleteAll();

		cusAuthorizationUsage = nctsHeader.MovementHeader.CusAuthorizationUsages.AddNew();
	}

	NctsHeader nctsHeader;
	CusAuthorizationUsage cusAuthorizationUsage;
}
