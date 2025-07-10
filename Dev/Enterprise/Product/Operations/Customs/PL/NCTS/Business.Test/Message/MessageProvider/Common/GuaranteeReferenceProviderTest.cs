using System;
using CargoWise.Types;
using static Enterprise.Core.Constants;
using NctsGuarantee = Enterprise.Customs.EU.NCTS.Business.NctsGuarantee;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class GuaranteeReferenceProviderTest : Customs.Business.Testing.DataProviderTestCase<GuaranteeReferenceProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null NctsGuarantee", "Value cannot be null.\r\nParameter name: nctsGuarantee", () => new GuaranteeReferenceProvider(1, null));
	}

	public void TestSequenceNumber() => AssertEquals("99", Provider.SequenceNumber);

	public void TestGRN()
	{
		var c0076BondTypeList = new ZString[]
		{
			EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver
			,EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee
			,EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor
			,EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.CashDepositGuarantee
			,EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.FlatRateVoucher
			,EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiverSecuredAmountNotGreaterThan500Eur
			,EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeWithMultipleUsage
		};

		CombineAssertions(() =>
		{
			AssertEquals("Not a C0076 bond type", ZString.Empty, Provider.GRN);

			foreach (var bondType in c0076BondTypeList)
			{
				guarantee.PW_BondType = bondType;
				AssertEquals("Not a C0076 bond type", "987", GetProvider().GRN);
			}
		});
	}

	public void TestAccessCode()
	{
		var c0076BondTypeList = new ZString[]
		{
			EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver
			,EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee
			,EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor
			,EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.CashDepositGuarantee
			,EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.FlatRateVoucher
			,EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiverSecuredAmountNotGreaterThan500Eur
			,EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeWithMultipleUsage
		};

		CombineAssertions(() =>
		{
			AssertEquals("Not a C0076 bond type", ZString.Empty, Provider.AccessCode);

			foreach (var bondType in c0076BondTypeList)
			{
				guarantee.PW_BondType = bondType;
				AssertEquals("Not a C0076 bond type", "1234", GetProvider().AccessCode);
			}
		});
	}

	public void TestCurrency()
	{
		CombineAssertions(() =>
		{
			guarantee.PW_RX_NKCurrency = CurrencyCodes.Pakistan;
			AssertEquals("In transition period and AmountToBeCovered is present", CurrencyCodes.Pakistan, GetProvider().Currency);

			guarantee.PW_BondAmount = ZDecimal.Zero;
			AssertEquals("In transition period and AmountToBeCovered is not present", string.Empty, GetProvider().Currency);
		});
	}

	public void TestAmountToBeCovered() => AssertEquals(12345.0m, Provider.AmountToBeCovered);

	protected override GuaranteeReferenceProvider GetProvider() => new GuaranteeReferenceProvider(99, guarantee);

	protected override void SetUp()
	{
		base.SetUp();
		guarantee = Factory.New<NctsGuarantee>();
		guarantee.PW_BondType = "A";
		guarantee.PW_BondNumber2 = "B";
		guarantee.PW_Password = "1234";
		guarantee.PW_BondNumber = "987";
		guarantee.PW_BondAmount = 12345.0m;
		guarantee.PW_RX_NKCurrency = CurrencyCodes.Poland;
	}
	NctsGuarantee guarantee;
}
