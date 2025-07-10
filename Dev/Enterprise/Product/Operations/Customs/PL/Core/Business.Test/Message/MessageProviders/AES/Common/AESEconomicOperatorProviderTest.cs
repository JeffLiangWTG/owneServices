using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESEconomicOperatorProviderTest : DataProviderTestCase<AESEconomicOperatorProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null JobDocAddress", "Value cannot be null.\r\nParameter name: jobDocAddress",
				() => new AESEconomicOperatorProvider(null));
		});
	}

	public void TestIdentificationNumber()
	{
		CombineAssertions(() =>
		{
			goodsLocationAddress.E2_GovRegNum = "";
			AssertEquals("No EORI", string.Empty, GetProvider().IdentificationNumber);
			goodsLocationAddress.E2_GovRegNum = "PL123";
			AssertEquals("EORI Mapped", "PL123", GetProvider().IdentificationNumber);
		});
	}

	protected override AESEconomicOperatorProvider GetProvider() => new AESEconomicOperatorProvider(goodsLocationAddress);

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		goodsLocationAddress = declaration.GoodsLocation.Address;
	}

	CusGoodsLocationAddress goodsLocationAddress;
}
