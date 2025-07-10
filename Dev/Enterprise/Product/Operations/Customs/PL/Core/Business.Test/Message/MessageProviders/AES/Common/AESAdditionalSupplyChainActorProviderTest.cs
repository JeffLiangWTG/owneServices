using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESAdditionalSupplyChainActorProviderTest : Customs.Business.Testing.DataProviderTestCase<AESAdditionalSupplyChainActorProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null CusReference", "Value cannot be null.\r\nParameter name: cusReference",
				() => new AESAdditionalSupplyChainActorProvider(null, 1));
		});
	}

	public void TestSequenceNumber()
	{
		AssertEquals(2, Provider.SequenceNumber);
	}

	public void TestRole()
	{
		AssertEquals("23", Provider.Role);
	}

	public void TestIdentificationNumber()
	{
		AssertEquals("ABC", Provider.IdentificationNumber);
	}

	protected override AESAdditionalSupplyChainActorProvider GetProvider() => new AESAdditionalSupplyChainActorProvider(cusReference, 2);

	protected override void SetUp()
	{
		base.SetUp();
		cusReference = Factory.New<CusSupplyChainActorReference>();
		cusReference.CFR_Code = "23";
		cusReference.CFR_Reference = "ABC";
	}

	CusReference cusReference;
}
