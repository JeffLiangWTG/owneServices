using CargoWise.Types;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class GoodsReferenceProviderTest : Customs.Business.Testing.DataProviderTestCase<GoodsReferenceProvider>
{
	public void TestConstructor()
	{
		AssertNoExceptionThrown(() => new GoodsReferenceProvider(1, ZInt.Zero));
	}

	public void TestSequenceNumber() => AssertEquals("99", Provider.SequenceNumber);

	public void TestDeclarationGoodsItemNumber() => AssertEquals("123", Provider.DeclarationGoodsItemNumber);

	protected override GoodsReferenceProvider GetProvider() => new GoodsReferenceProvider(99, 123);
}
