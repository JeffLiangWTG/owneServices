using System;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC013015CAdditionalSupplyChainActorTest : Customs.Business.Testing.DataProviderTestCase<CC013015CAdditionalSupplyChainActorProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null CusSupplyChainActorReference", "Value cannot be null.\r\nParameter name: cusSupplyChainActorReference", () => new CC013015CAdditionalSupplyChainActorProvider(1, null));
	}

	public void TestSequenceNumber() => AssertEquals(99, Provider.SequenceNumber);

	public void TestRole() => AssertEquals("123", Provider.Role);

	public void TestIdentificationNumber() => AssertEquals("CFRReference", Provider.IdentificationNumber);

	protected override CC013015CAdditionalSupplyChainActorProvider GetProvider() => new CC013015CAdditionalSupplyChainActorProvider(99, cusSupplyChainActorReference);

	protected override void SetUp()
	{
		base.SetUp();
		cusSupplyChainActorReference = Factory.New<CusSupplyChainActorReference>();
		cusSupplyChainActorReference.CFR_Code = "123";
		cusSupplyChainActorReference.CFR_Reference = "CFRReference";
	}
	CusSupplyChainActorReference cusSupplyChainActorReference;
}
