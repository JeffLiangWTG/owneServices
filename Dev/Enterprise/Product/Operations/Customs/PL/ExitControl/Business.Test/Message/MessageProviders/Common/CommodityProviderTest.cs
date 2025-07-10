using System;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

sealed class CommodityProviderTest : DataProviderTestCase<CommodityProvider>
{
	public void TestConstructor()
	{
		var expectedMessage = string.Empty;

#if NETFRAMEWORK
		expectedMessage = "Value cannot be null.\r\nParameter name: exitConsignmentItem";
#else
		expectedMessage = "Value cannot be null. (Parameter 'exitConsignmentItem')";
#endif
		AssertExceptionThrown<ArgumentNullException>("Null CusExitConsignmentItem", expectedMessage,
			() => new CommodityProvider(null));
	}

	public void TestGoodsMeasure() => AssertNotNull(Provider.GoodsMeasure);

	protected override CommodityProvider GetProvider() => new CommodityProvider(cusExitConsignmentItem);

	protected override void SetUp()
	{
		base.SetUp();
		cusExitConsignmentItem = Factory.New<CusExitConsignmentItem>();
	}

	CusExitConsignmentItem cusExitConsignmentItem;
}
