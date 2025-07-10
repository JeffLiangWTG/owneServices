using System;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.NL.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(GuaranteeReferenceProvider))]
sealed class GuaranteeReferenceProviderTest : Customs.Business.Testing.DataProviderTestCase<GuaranteeReferenceProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new GuaranteeReferenceProvider(null, 1));
	}

	public void TestSequenceNumeric()
	{
		AssertEquals(1, provider.SequenceNumeric);
	}

	public void TestAccessCode()
	{
		guarantee.PW_Password = "abc";
		AssertEquals("abc", provider.AccessCode);
	}

	public void TestCurrency()
	{
		guarantee.PW_BondAmount = 100;
		guarantee.PW_RX_NKCurrency = "EUR";
		AssertEquals("EUR", provider.Currency);
	}

	public void TestCurrency_AmountToBeCoveredIs0()
	{
		AssertNullOrEmpty(provider.Currency);
	}

	public void TestCurrency_RetrieveFromGuarantee()
	{
		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var org1 = Factory.NewWithValidTestData<OrgHeader>();
		var cusGuarantee = Factory.NewWithValidTestData<CusGuaranteeHeader>();
		cusGuarantee.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusGuarantee.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
		cusGuarantee.CPH_Number = "GUA001";
		cusGuarantee.CPH_UnitOfMeasure = Core.Constants.CurrencyCodes.UnitedStates;
		cusGuarantee.CPH_OH_PermitHolder = org1.PK;

		var nctsHeader = guarantee.NctsHeader;
		nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;

		guarantee.PW_BondNumber = "GUA001";
		guarantee.PW_BondAmount = 100;

		provider = new GuaranteeReferenceProvider(guarantee, 1);
		AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, provider.Currency);
	}

	public void TestCurrency_DefaultEUR()
	{
		guarantee.PW_BondAmount = 100;

		AssertEquals(Core.Constants.CurrencyCodes.EuropeanUnion, provider.Currency);
	}

	public void TestGRN()
	{
		guarantee.PW_BondNumber = "BondNr";
		AssertEquals("BondNr", provider.GRN);
	}

	public void TestAmount()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", decimal.Zero, provider.Amount);

			guarantee.PW_BondAmount = 12.3456;
			AssertEquals("Amount to be covered must be rounded on 2 decimals", 12.35m, provider.Amount);
		});
	}

	public void TestId()
	{
		AssertEquals(string.Empty, provider.Id);
	}

	public void TestReferenceId()
	{
		AssertEquals(string.Empty, provider.ReferenceId);
	}

	public void TestGuaranteeOffice()
	{
		AssertEquals(string.Empty, provider.GuaranteeOffice);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		guarantee = header.MovementHeader.Guarantees.AddNew();

		provider = new GuaranteeReferenceProvider(guarantee, 1);
	}

	GuaranteeReferenceProvider provider;
	NctsGuarantee guarantee;

	protected override GuaranteeReferenceProvider GetProvider() => provider;
}
