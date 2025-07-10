using System;
using System.Linq;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class GuaranteeProviderTest : Customs.Business.Testing.DataProviderTestCase<GuaranteeProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null NctsGuarantee List", "Value cannot be null.\r\nParameter name: nctsGuarantees", () => new GuaranteeProvider(1, null));
	}

	public void TestSequenceNumber() => AssertEquals("99", Provider.SequenceNumber);

	public void TestGuaranteeType() => AssertEquals(EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiverByAgreement, Provider.GuaranteeType);

	public void TestOtherGuaranteeReference() => AssertEquals("B", Provider.OtherGuaranteeReference);

	public void TestGuaranteeReference()
	{
		CombineAssertions(() =>
		{
			AssertEquals("no type in the GuaranteeTypeWithReference", 0, GetProvider().GuaranteeReference.Count);

			guarantee1.PW_BondType = EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver;
			AssertEquals("one type is in the GuaranteeTypeWithReference", 1, GetProvider().GuaranteeReference.Count);

			guarantee2.PW_BondType = EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver;
			var provider = GetProvider();
			AssertEquals("both type are in the GuaranteeTypeWithReference", 2, provider.GuaranteeReference.Count);
			AssertEquals("Should contain 2 guarantees", 2, provider.GuaranteeReference.Count);
			AssertEquals("Sequence number should start with 1", "1", provider.GuaranteeReference.First().SequenceNumber);
			AssertEquals("2nd sequence number should be 2", "2", provider.GuaranteeReference.Last().SequenceNumber);
		});
	}

	protected override GuaranteeProvider GetProvider() => new GuaranteeProvider(99, nctsGuarantees);

	protected override void SetUp()
	{
		base.SetUp();
		guarantee1 = Factory.New<NctsGuarantee>();
		guarantee1.PW_BondType = EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiverByAgreement;
		guarantee1.PW_BondNumber2 = "B";
		guarantee2 = Factory.New<NctsGuarantee>();
		nctsGuarantees = new NctsGuarantee[] { guarantee1, guarantee2 };
	}
	NctsGuarantee[] nctsGuarantees;
	NctsGuarantee guarantee1;
	NctsGuarantee guarantee2;
}
