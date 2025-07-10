using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

sealed class GoodsMeasureProviderTest : DataProviderTestCase<GoodsMeasureProvider>
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
			() => new GoodsMeasureProvider(null));
	}

	public void TestGrossMassValue() => CombineAssertions(() =>
	{
		AssertEquals("CCI_GrossMass is not set", ZDecimal.Zero, GetProvider().GrossMassValue);

		cusExitConsignmentItem.CCI_GrossMass = 2.04m;
		AssertEquals("CCI_GrossMass is 2.04", 2.04m, GetProvider().GrossMassValue);
	});

	public void TestNetMass() => CombineAssertions(() =>
	{
		AssertEquals("CCI_NetMass is not set", ZDecimal.Zero, GetProvider().NetMass);

		cusExitConsignmentItem.CCI_NetMass = 5.32m;
		AssertEquals("CCI_NetMass is 5.32", 5.32m, GetProvider().NetMass);
	});

	public void TestSupplementaryUnitsValue() => AssertNull(Provider.SupplementaryUnitsValue);

	protected override GoodsMeasureProvider GetProvider() => new GoodsMeasureProvider(cusExitConsignmentItem);

	protected override void SetUp()
	{
		base.SetUp();
		cusExitConsignmentItem = Factory.New<CusExitConsignmentItem>();
	}

	CusExitConsignmentItem cusExitConsignmentItem;
}
